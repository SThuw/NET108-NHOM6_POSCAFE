# Giai đoạn 4 — Kết nối SQL Server (EF Core)

Bản chụp ngày 07/11/2026. Kế thừa toàn bộ Giai đoạn 3; không phá vỡ hợp đồng
API hay giao diện đã kiểm thử.

## 1. Mục tiêu

| Trước (GĐ 3) | Sau (GĐ 4) |
|---|---|
| Dữ liệu sống trong tiến trình (static list) | Dữ liệu được lưu vào SQL Server |
| Mất khi restart | Bền vững qua nhiều lần restart |
| Mã đơn tăng từ in-memory sequence | Mã đơn dựa trên MAX(OrderNumber) trong DB |
| Chống gửi trùng bằng in-memory dictionary | Chống gửi trùng bằng unique index (RequestId) |

## 2. Các tệp thay đổi / thêm mới

| Tệp | Trạng thái |
|---|---|
| `src/.../Data/CafePosDbContext.cs` | **Từ GĐ 4**: DbContext, mapping EF Core, conversion OrderType/PaymentMethod |
| `src/.../Data/DbInitializer.cs` | **Từ GĐ 4**: seed data qua `HasData` migration-safe |
| `src/.../Data/CafePosDbContextFactory.cs` | **Từ GĐ 4**: factory cho EF CLI |
| `src/.../Data/Migrations/` | **Từ GĐ 4**: 2 migration (InitialPosSchema + NormalizeVietnameseNames) |
| `src/.../Models/Entities/Category.cs` | Mở rộng: thêm `Code`, `DisplayOrder`, `IsActive`, navigation |
| `src/.../Models/Entities/Product.cs` | Mở rộng: thêm `Description`, navigation |
| `src/.../Models/Entities/Discount.cs` | **Mới GĐ 4**: entity DB cho bảng Discounts |
| `src/.../Models/Entities/Order.cs` | Mở rộng: thêm `Id`, `DiscountId`, `Status`, navigation |
| `src/.../Models/Entities/OrderItem.cs` | Mở rộng: thêm `Id`, `OrderId`, navigation |
| `src/.../Models/ViewModels/CreateProductDto.cs` | Thêm trường `Description` |
| `src/.../Services/Implementations/DbProductService.cs` | **Mới GĐ 4**: IProductService dùng EF |
| `src/.../Services/Implementations/DbOrderService.cs` | **Mới GĐ 4**: IOrderService dùng EF + transaction |
| `src/.../Program.cs` | Chuyển DI sang `DbProductService` / `DbOrderService` |
| `src/.../Views/Pos/Index.cshtml` | Cập nhật badge "Giai đoạn 4 · SQL Server" |
| `scripts/ef.ps1` | **Mới GĐ 4**: wrapper EF CLI dùng local dotnet |
| `docs/GIAI-DOAN-4-migration.sql` | Idempotent SQL script để kiểm tra migration |

## 3. Thiết kế kỹ thuật

### 3.1 DbContext và mapping

`CafePosDbContext` định nghĩa mapping đầy đủ cho 5 bảng:

```
Categories → Products → OrderItems → Orders → Discounts
```

Điểm đáng chú ý:
- Collation `Vietnamese_100_CI_AI` mặc định; cột tên dùng `Latin1_General_100_CI_AI`
  để tìm kiếm không dấu ngay trên DB khi cần.
- `Order.OrderType` / `Order.PaymentMethod` lưu nhãn tiếng Việt trong SQL,
  nhưng API vẫn trả/nhận mã kỹ thuật (`pickup`, `delivery`, `dinein`, `cash`, `card`, `wallet`).
  Conversion xảy ra trong `HasConversion()`.
- `OrderItem.LineTotal` là **computed column** (`PERSISTED`) do SQL Server tính; EF không insert.

### 3.2 DbProductService

```csharp
// Đọc danh mục đang hoạt động, sắp theo DisplayOrder
GetCategories() → WHERE IsActive ORDER BY DisplayOrder, Name

// Lọc danh mục trên server; tìm không dấu chạy phía client sau khi lọc
GetProducts(categoryId?, search?) → AsNoTracking, client-side Normalize()

// Kiểm tra danh mục, trùng tên (Normalize), insert; Id = "p<unix-ms>"
Create(CreateProductDto) → db.Products.Add + SaveChanges
```

### 3.3 DbOrderService

```csharp
// Đọc Discount.IsActive, trả DiscountCode DTO tương thích GĐ 3
GetDiscountCodes() → WHERE IsActive, map sang DiscountCode(Code, Name, Percent)

// Ước tính dựa trên MAX OrderNumber; không giữ chỗ
GetNextOrderNumber() → MAX(OrderNumber) + 1

// Tính tạm tính, không lưu DB
Preview() → Calculate() [không SaveChanges]

// Idempotent: kiểm tra RequestId trước, dùng transaction + unique index
Checkout() →
  1. SELECT Orders WHERE RequestId (nếu tìm thấy, trả lại ngay)
  2. BEGIN TRANSACTION (ReadCommitted)
  3. Lấy MAX(OrderNumber) trong lock → seq+1
  4. db.Orders.Add + SaveChanges
  5. COMMIT
  6. Nếu DbUpdateException (2601/2627) → ROLLBACK, trả đơn trùng
```

Giao diện JavaScript không thay đổi: vẫn gửi `RequestId` và nhận lại đơn đã lưu.

## 4. Cấu hình và chạy

### 4.1 Yêu cầu

- SQL Server (Express, Developer, hoặc Azure SQL) đã được tạo database `CafePOS`.
- Tài khoản SQL có quyền `CREATE TABLE`, `INSERT`, `SELECT`, `UPDATE`.

### 4.2 Đặt connection string

```powershell
# Dùng User Secrets (khuyến nghị cho dev):
dotnet user-secrets set "ConnectionStrings:CafePos" "Server=.;Database=CafePOS;Trusted_Connection=True;TrustServerCertificate=True;"

# Hoặc biến môi trường:
$env:ConnectionStrings__CafePos = "Server=.;Database=CafePOS;Trusted_Connection=True;TrustServerCertificate=True;"
```

### 4.3 Chạy migration

```powershell
# Dùng local dotnet + script ef.ps1:
.\scripts\ef.ps1 database update

# Hoặc nếu máy đã cài .NET SDK + EF Tools:
dotnet ef database update --project src/CafePOSManagement.Web
```

Lệnh trên sẽ tạo các bảng và nạp seed data (7 danh mục, 16 sản phẩm, 4 mã giảm giá, 3 đơn mẫu).

### 4.4 Build và chạy

```powershell
$env:DOTNET_CLI_HOME = Join-Path (Get-Location) '.tools\cli-home'
$env:NUGET_PACKAGES  = Join-Path (Get-Location) '.tools\nuget'
.\.tools\dotnet\dotnet.exe build CafePOSManagement.sln --no-restore
.\.tools\dotnet\dotnet.exe run --project src/CafePOSManagement.Web --no-build --launch-profile http
```

Mở `http://localhost:5088`. Nếu chưa cấu hình connection string, ứng dụng sẽ ném exception
ngay khi khởi động với thông báo rõ ràng bằng tiếng Việt.

**Fallback in-memory**: Nếu muốn chạy không cần SQL Server, chú thích 2 dòng
`DbProductService`/`DbOrderService` trong `Program.cs` và bỏ chú thích 2 dòng
`ProductService`/`OrderService` phía dưới.

## 5. Kiểm thử

### 5.1 Build

```
dotnet build CafePOSManagement.sln --no-restore → 0 Error(s), 0 Warning(s)
```

### 5.2 Kịch bản kiểm tra thủ công (cần SQL Server)

| ID | Thao tác | Kết quả mong đợi |
|---|---|---|
| DB01 | Mở POS sau migration | 16 món, badge "Giai đoạn 4 · SQL Server" |
| DB02 | Thanh toán một đơn | Đơn xuất hiện trong bảng Orders, OrderItems |
| DB03 | Restart server, mở lại POS | Món và mã đơn tiếp theo vẫn đúng |
| DB04 | Thêm món mới | Lưu vào bảng Products, còn khi restart |
| DB05 | Gửi lại cùng RequestId | Server trả lại chính đơn đó, không tạo thêm |
| DB06 | Mã giảm giá đầy đủ | NONE/SALE10/THANK15/SALE20 hiển thị từ DB |
| DB07 | Chưa cấu hình connection | Thông báo lỗi tiếng Việt rõ ràng khi khởi động |

### 5.3 Kiểm tra SQL trực tiếp

```sql
SELECT * FROM Categories ORDER BY DisplayOrder;
SELECT * FROM Products ORDER BY CategoryId, Name;
SELECT * FROM Discounts ORDER BY Id;
SELECT o.*, oi.ProductName, oi.Quantity, oi.LineTotal
FROM Orders o JOIN OrderItems oi ON o.Id = oi.OrderId
ORDER BY o.Id, oi.Id;
```

## 6. Giới hạn còn lại (Giai đoạn 5+)

- Chưa có đăng nhập/phân quyền; chỉ chạy cục bộ.
- Giỏ hàng và đơn vừa chốt không lưu qua F5 (tính năng client-side).
- Chưa tích hợp ngân hàng/ví hoặc hóa đơn điện tử.
- Chưa có trang quản lý đơn hàng lịch sử / báo cáo doanh thu.
- Chưa thử máy in nhiệt vật lý; in PDF qua trình duyệt hoạt động tốt.

# CafePOS Management

Khung website POS tiếng Việt trên ASP.NET Core MVC **.NET 8**, C#, Service Layer,
Entity Framework Core SQL Server 8.0.33 và Bootstrap 5.3.8.

Đã triển khai **Giai đoạn 4**: toàn bộ dữ liệu (danh mục, sản phẩm, giảm giá, đơn hàng)
được lưu vào **SQL Server** qua EF Core. Chốt đơn dùng transaction + unique index trên
RequestId để chống gửi trùng bền vững qua các lần restart máy chủ.
Giữ nguyên hợp đồng API và giao diện của Giai đoạn 3; chỉ cần cấu hình connection string
và chạy migration một lần.
Xem [hướng dẫn Giai đoạn 4](docs/GIAI-DOAN-4.md) và
[hướng dẫn Giai đoạn 3](docs/GIAI-DOAN-3.md).

## Yêu cầu môi trường

| Phần mềm | Phiên bản yêu cầu | Ghi chú |
|---|---|---|
| **.NET 8 SDK** | 8.0.x | Dự án target `net8.0`; không dùng được SDK 9/10 đơn thuần |
| **SQL Server** | 2019+ / Express / LocalDB / Azure SQL | Cần cho Giai đoạn 4+ |
| **Node.js** | 18+ (tùy chọn) | Chỉ cần để chạy script kiểm thử Playwright |

> **Máy này có SDK 10 nhưng không có Runtime 8?**
> Dùng local `dotnet` trong `.tools/dotnet/` (đã cài .NET 8.0.427) thay cho lệnh `dotnet` hệ thống.
> Xem [Chạy với .tools/dotnet](#chạy-với-toolsdotnet) bên dưới.

## Bắt đầu nhanh (SQL Server + local dotnet)

```powershell
# 1. Đặt connection string (một lần):
.\.tools\dotnet\dotnet.exe user-secrets set "ConnectionStrings:CafePos" `
    "Server=.;Database=CafePOS;Trusted_Connection=True;TrustServerCertificate=True;" `
    --project src/CafePOSManagement.Web

# 2. Chạy migration (một lần):
.\scripts\ef.ps1 database update

# 3. Khởi động ứng dụng:
.\run.ps1          # Script tự động dùng .tools/dotnet, không cần build lại
# Mở: http://localhost:5088
```

---

## Chạy với .tools/dotnet

SDK 8.0.427 cùng ASP.NET Core Runtime 8.0.33 đã được cài sẵn trong `.tools/dotnet`.
Thư mục `.tools` không đưa vào Git (gitignored).

```powershell
# Build:
$env:DOTNET_CLI_HOME = Join-Path (Get-Location) '.tools\cli-home'
$env:NUGET_PACKAGES  = Join-Path (Get-Location) '.tools\nuget'
.\.tools\dotnet\dotnet.exe build CafePOSManagement.sln --no-restore

# Chạy:
.\.tools\dotnet\dotnet.exe run --project src/CafePOSManagement.Web --no-build --launch-profile http
```

Hoặc chỉ cần dùng script ngắn gọn:

```powershell
.\run.ps1           # Chạy ngay (giả sử đã build)
.\run.ps1 -Build    # Build trước rồi chạy
```

> **Dừng ứng dụng:** `Ctrl+C` tại terminal đang chạy.
> **Lỗi "file bị khóa" (MSB3027/MSB3021):** Dừng phiên CafePOS đang chạy trước khi build lại.

---

## Chạy trên máy khác có .NET 8 SDK

```powershell
dotnet restore CafePOSManagement.sln
dotnet build CafePOSManagement.sln --no-restore
dotnet user-secrets set "ConnectionStrings:CafePos" "..." --project src/CafePOSManagement.Web
dotnet ef database update --project src/CafePOSManagement.Web
dotnet run --project src/CafePOSManagement.Web --no-build --launch-profile http
```

---

## Cấu hình SQL Server

Giai đoạn 4 yêu cầu SQL Server. Nếu thiếu connection string, ứng dụng sẽ ném exception
khi khởi động với thông báo tiếng Việt rõ ràng — không tự fallback sang InMemory.

```powershell
# Windows Authentication (SQL Server cục bộ):
.\.tools\dotnet\dotnet.exe user-secrets set "ConnectionStrings:CafePos" `
    "Server=.;Database=CafePOS;Trusted_Connection=True;TrustServerCertificate=True;" `
    --project src/CafePOSManagement.Web

# LocalDB:
.\.tools\dotnet\dotnet.exe user-secrets set "ConnectionStrings:CafePos" `
    "Server=(localdb)\MSSQLLocalDB;Database=CafePOS;Integrated Security=True;TrustServerCertificate=True;" `
    --project src/CafePOSManagement.Web
```

Tên biến môi trường: **`ConnectionStrings__CafePos`** (hai dấu gạch dưới).
Không ghi mật khẩu vào source, Git, log hoặc screenshot.

**Fallback in-memory** (không cần SQL Server): Chú thích 2 dòng `DbProductService`/`DbOrderService`
và bỏ chú thích 2 dòng `ProductService`/`OrderService` trong
[`Program.cs`](src/CafePOSManagement.Web/Program.cs).

---

## Kiến trúc

```text
Razor View / Fetch API
    → PosController (CSRF, validation, lỗi 400)
        → IProductService / IOrderService (DI Scoped)
            → DbProductService / DbOrderService (EF Core)
                → CafePosDbContext → SQL Server
IPosService / PosService → PosIndexViewModel → Razor View (render ban đầu)
```

- **Controller** điều phối và trả lỗi validation; service quản lý nghiệp vụ.
- **DbContext** lifetime Scoped; mỗi request nhận một instance riêng.
- **Request culture** cố định `vi-VN`; tiền hiển thị `45.000 ₫`.
- **Múi giờ** `Asia/Ho_Chi_Minh`; `CreatedAt` lưu DateTimeOffset UTC trong DB.
- **Chống gửi trùng** bằng `RequestId` (unique index) + transaction trong `DbOrderService`.
- Các POST có kiểm tra **CSRF** bằng token Razor và header `X-CSRF-TOKEN`.
- Development dùng HTTP localhost; production bật HTTPS redirect/HSTS.

---

## Tài liệu

| Giai đoạn | Hướng dẫn | Mã nguồn |
|---|---|---|
| Giai đoạn 4 — SQL Server | [GIAI-DOAN-4.md](docs/GIAI-DOAN-4.md) | — |
| Giai đoạn 3 — Chức năng POS | [GIAI-DOAN-3.md](docs/GIAI-DOAN-3.md) | [MA-NGUON-GIAI-DOAN-3.md](docs/MA-NGUON-GIAI-DOAN-3.md) |
| Giai đoạn 2 — Giao diện POS | [GIAI-DOAN-2.md](docs/GIAI-DOAN-2.md) | — |
| Giai đoạn 1 — CLI cơ bản | [GIAI-DOAN-1.md](docs/GIAI-DOAN-1.md) | — |
| Kiểm thử Giai đoạn 1 | [KIEM-THU-GIAI-DOAN-1.md](docs/KIEM-THU-GIAI-DOAN-1.md) | — |

Nguồn thư viện: [EF Core SQL Server](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/8.0.33),
[EF Core Tools](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Tools/8.0.33),
[Bootstrap 5.3](https://getbootstrap.com/docs/5.3/getting-started/introduction/).
Bootstrap được phân phối theo MIT; giữ nguyên LICENSE trong thư mục thư viện.
Không dùng tài sản hoặc logo bên thứ ba ngoài ảnh minh họa từ Unsplash.

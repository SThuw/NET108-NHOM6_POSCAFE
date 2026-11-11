# BÁO CÁO KẾT QUẢ THỰC HÀNH LAB 3 — DỰ ÁN .NET 1 (NET108)
**Đề tài:** Hệ thống Quản lý Bán hàng Quán Đồ uống (PolyCafe - POS Cafe)  
**Nhóm thực hiện:** Nhóm 6  
**Lớp / Khóa:** Dự án .NET 1 (FPT Polytechnic)  
**Kho lưu trữ GitHub:** [https://github.com/SThuw/NET108-NHOM6_POSCAFE](https://github.com/SThuw/NET108-NHOM6_POSCAFE)

---

## PHẦN I: THÔNG TIN NHÓM & BẢNG PHÂN CHIA NHIỆM VỤ (5 THÀNH VIÊN)

Dựa trên chỉ đạo của giảng viên hướng dẫn (phát triển tiếp nối từ đặc tả SRS Lab 1 và thiết kế CSDL Lab 2, ứng dụng mô hình **Code First** và **Kiến trúc 3 lớp**), nhóm 6 phân công nhiệm vụ trọn gói theo từng phân hệ (Vertical Slice) để đảm bảo tính độc lập, tránh xung đột mã nguồn khi cộng tác trên Git:

| STT | Họ và tên | MSSV | Vai trò | Phân hệ phụ trách (Từ Lab 1 & 2) | Sản phẩm bàn giao LAB 3 | Đánh giá |
| :-: | :--- | :---: | :---: | :--- | :--- | :---: |
| **1** | **Nguyễn Kim Song Thư** | **TB02272** | **Nhóm trưởng** | Kiến trúc hệ thống, Hạ tầng lõi & Cụm bảng AI | - Dựng khung Solution MVC 3 lớp, cấu hình Entity Framework Core.<br>- Xây dựng lớp tiện ích **`DBUtil.cs`** (ADO.NET thuần: `queryDataTable`, `executeScalar`, `executeNonQuery`).<br>- **Bổ sung Cụm bảng AI State Store** (`AiSessions`, `AiMessages`, `AiStateStores`) và lớp **`AiStateStoreDAL.cs`** (đáp ứng trọn vẹn lời dặn chấm điểm Lab 2 của Giảng viên).<br>- Quản lý GitHub nhóm, khởi tạo nhánh `feature/lab3-dal-and-entities`, review code.<br>- Tổng hợp báo cáo nộp LMS. | 100% |
| **2** | **Ninh Văn Đức** | **TB02291** | Thành viên | Quản lý Thực đơn (Menu & Danh mục) | - Thiết kế Entity `Category.cs`, `Product.cs` + Seed Data Menu.<br>- Xây dựng các lớp truy cập dữ liệu **`CategoryDAL.cs`** và **`ProductDAL.cs`** với đầy đủ 5 hàm CRUD (`GetAll`, `GetById`, `Insert`, `Update`, `Delete`). | 100% |
| **3** | **Trần Thị Tuyết Kha** | **TB02214** | Thành viên | Thẻ định danh & Thẻ bàn (Cards) | - Thiết kế Entity `Card.cs` (Thẻ bàn / Thẻ rung) + Seed Data Thẻ.<br>- Xây dựng lớp **`CardDAL.cs`** với đầy đủ 5 hàm CRUD và phương thức kiểm tra trạng thái thẻ phục vụ. | 100% |
| **4** | **Nguyễn Trí Nguyễn** | **TB02277** | Thành viên | POS Bán hàng & Hóa đơn (Bills) | - Thiết kế Entity `Order.cs`, `OrderItem.cs` + Seed Data Hóa đơn.<br>- Xây dựng các lớp **`OrderDAL.cs`** và **`OrderItemDAL.cs`** với đầy đủ 5 hàm CRUD và phương thức lấy chi tiết món theo đơn `GetByOrderId`. | 100% |
| **5** | **Nguyễn Lâm Chí Tâm** | **TB02234** | Thành viên | Nhân sự & Tài khoản (Users) | - Thiết kế Entity `User.cs` (Phân quyền Quản lý / Nhân viên) + Seed Data User.<br>- Xây dựng lớp **`UserDAL.cs`** với đầy đủ 5 hàm CRUD và phương thức xác thực đăng nhập `Login(email, password)`. | 100% |

---

## PHẦN II: BÀI 1 — KHỞI TẠO CSDL THEO PHƯƠNG PHÁP CODE FIRST

### 1.1. Mô hình CSDL PolyCafe (Code First)
Hệ thống sử dụng **Entity Framework Core 8.0** kết nối SQL Server LocalDB (`MSSQLLocalDB`), toàn bộ các bảng được sinh ra tự động từ các lớp Entity C# trong thư mục `Models/Entities/`:
1. **`Categories`**: Bảng danh mục đồ uống (`Id`, `Name`, `Code`, `DisplayOrder`, `IsActive`, `CreatedAt`).
2. **`Products`**: Bảng đồ uống / món ăn (`Id`, `CategoryId`, `Name`, `Price`, `OriginalPrice`, `ImageUrl`, `Description`, `IsAvailable`, `CreatedAt`).
3. **`Users`**: Bảng tài khoản nhân sự (`Id`, `Email`, `Password`, `FullName`, `Phone`, `Role`, `IsActive`, `CreatedAt`).
4. **`Cards`**: Bảng thẻ bàn / thẻ rung định danh (`Id`, `Code`, `Name`, `IsOccupied`, `IsActive`, `CreatedAt`).
5. **`Orders`**: Bảng hóa đơn bán hàng (`Id`, `RequestId`, `OrderNumber`, `OrderType`, `Subtotal`, `DiscountAmount`, `TaxAmount`, `TotalAmount`, `PaymentMethod`, `Status`, `CreatedAt`, `Note`).
6. **`OrderItems`**: Bảng chi tiết hóa đơn (`Id`, `OrderId`, `ProductId`, `ProductName`, `UnitPrice`, `Quantity`, `LineTotal`).
7. **`Discounts`**: Bảng mã khuyến mãi / giảm giá (`Id`, `Code`, `Name`, `DiscountPercent`).
8. **`AiSessions`** *(Bổ sung theo nhận xét Lab 2)*: Bảng phiên hội thoại AI (`Id`, `UserId`, `Title`, `Context`, `IsActive`, `CreatedAt`).
9. **`AiMessages`** *(Bổ sung theo nhận xét Lab 2)*: Bảng lịch sử Prompt/Response (`Id`, `SessionId`, `Role`, `Content`, `CreatedAt`).
10. **`AiStateStores`** *(Bổ sung theo nhận xét Lab 2)*: Bảng lưu trạng thái ngữ cảnh/bộ nhớ AI (`Id`, `SessionId`, `StateKey`, `StateValue`, `UpdatedAt`).

### 1.2. Lệnh thực thi Migration và Sinh CSDL
```powershell
# 1. Tạo Migration ghi nhận thực thể Users và Cards
dotnet ef migrations add AddUsersAndCardsForLab3 --project src/CafePOSManagement.Web

# 2. Tạo Migration ghi nhận Cụm bảng AI State Store (Lời dặn giảng viên)
dotnet ef migrations add AddAiStateStoreCluster --project src/CafePOSManagement.Web

# 3. Cập nhật sinh tự động Database và toàn bộ các bảng trên SQL Server
dotnet ef database update --project src/CafePOSManagement.Web
```
**Kết quả thực thi:** CSDL `CafePOS` được tạo lập thành công 100% trên LocalDB với đầy đủ 10 bảng, khóa chính, khóa ngoại, ràng buộc CHECK và nạp sẵn toàn bộ dữ liệu mẫu ban đầu.

---

## PHẦN III: BÀI 2 — BỐ CỤC DỰ ÁN THEO MÔ HÌNH 3 LỚP & QUẢN TRỊ GIT

### 3.1. Bố cục cấu trúc 3 lớp (3-Layer Architecture)
Dự án được phân chia thư mục rõ ràng theo đúng nguyên lý phân tách trách nhiệm:
```text
CafePOSManagement/
├── src/CafePOSManagement.Web/
│   ├── Controllers/             # [Presentation Layer] Điều hướng & tiếp nhận yêu cầu từ giao diện
│   │   ├── PosController.cs     # Xử lý luồng bán hàng POS trực tiếp
│   │   └── Lab3Controller.cs    # Điều hướng hiển thị Dashboard kiểm thử các lớp DAL
│   │
│   ├── DAL/                     # [Data Access Layer] Các lớp truy cập CSDL bằng ADO.NET thuần (Bài 4)
│   │   ├── CategoryDAL.cs       # Ninh Văn Đức phụ trách
│   │   ├── ProductDAL.cs        # Ninh Văn Đức phụ trách (kèm bí danh DrinkDAL)
│   │   ├── CardDAL.cs           # Trần Thị Tuyết Kha phụ trách
│   │   ├── OrderDAL.cs          # Nguyễn Trí Nguyễn phụ trách (kèm bí danh BillDAL)
│   │   ├── OrderItemDAL.cs      # Nguyễn Trí Nguyễn phụ trách (kèm bí danh BillDetailDAL)
│   │   └── UserDAL.cs           # Nguyễn Lâm Chí Tâm phụ trách
│   │
│   ├── Utils/                   # [Infrastructure / Helper Layer] Lớp tiện ích chung (Bài 3)
│   │   └── DBUtil.cs            # Nguyễn Kim Song Thư phụ trách
│   │
│   ├── Models/
│   │   ├── Entities/            # Các thực thể dữ liệu Code First (Category, Product, User, Card, Order,...)
│   │   └── ViewModels/          # Các DTO truyền nhận dữ liệu giao diện
│   │
│   ├── Data/                    # [ORM Layer] DbContext & Cấu hình Code First Migrations
│   │   ├── CafePosDbContext.cs
│   │   ├── DbInitializer.cs     # Dữ liệu mẫu (Seed Data)
│   │   └── Migrations/
│   │
│   └── Views/                   # [Presentation Layer - Razor Views]
│       ├── Pos/Index.cshtml     # Màn hình bán hàng POS hiện đại
│       ├── Lab3/Index.cshtml    # Màn hình báo cáo & kiểm thử toàn bộ DAL
│       └── Shared/_Layout.cshtml
│
└── tests/CafePOSManagement.Checks/ # Bộ kiểm thử tự động toàn diện dự án
    ├── Program.cs
    └── Lab3DalChecks.cs         # Kiểm thử 100% các hàm CRUD của 6 lớp DAL
```

### 3.2. Quản lý mã nguồn với Git
- Repository nhóm: `https://github.com/SThuw/NET108-NHOM6_POSCAFE`
- Phân nhánh: Tạo nhánh `feature/lab3-dal-and-entities` để triển khai các nội dung mới của Lab 3 mà không làm gián đoạn mã nguồn `main`.

---

## PHẦN IV: BÀI 3 — XÂY DỰNG LỚP TIỆN ÍCH DBUTIL (ADO.NET)

Lớp tiện ích `DBUtil` nằm tại `src/CafePOSManagement.Web/Utils/DBUtil.cs` được xây dựng bằng ADO.NET thuần với các hàm cốt lõi theo đúng yêu cầu đề bài:
1. **`connString`**: Quản lý chuỗi kết nối SQL Server tập trung, linh hoạt.
2. **`queryDataTable(string sql, List<object>? parameters)`**:
   - Sử dụng `SqlConnection`, `SqlCommand`, `SqlDataAdapter`.
   - Chuẩn hóa tham số `@1`, `@2`,... Tự động gán `SqlDbType.NVarChar` cho chuỗi string để bảo toàn Unicode tiếng Việt.
   - Trả về đối tượng `DataTable` để các lớp DAL ánh xạ sang Entity.
3. **`executeScalar(string sql, List<object>? parameters)`**:
   - Sử dụng `cmd.ExecuteScalar()` để lấy kết quả đơn (như đếm dòng, lấy Id vừa tạo).
4. **`executeNonQuery(string sql, List<object>? parameters)`**:
   - Sử dụng `cmd.ExecuteNonQuery()` để thực thi các câu lệnh thay đổi dữ liệu `INSERT`, `UPDATE`, `DELETE`, trả về số dòng bị ảnh hưởng.

---

## PHẦN V: BÀI 4 — CÁC LỚP THỰC THỂ & DAL VỚI ĐẦY ĐỦ 5 HÀM CRUD

Tất cả các lớp DAL đều có đủ 5 hàm CRUD:
- **`GetAll()`**: Lấy toàn bộ danh sách bản ghi đang hoạt động.
- **`GetById(id)`**: Lấy chi tiết một bản ghi theo khóa chính.
- **`Insert(entity)`**: Thêm mới bản ghi vào CSDL.
- **`Update(entity)`**: Cập nhật thông tin bản ghi.
- **`Delete(id)`**: Xóa bản ghi (hỗ trợ cả xóa mềm cập nhật trạng thái `IsActive = 0`).

### Phân công chi tiết:
1. **Nguyễn Kim Song Thư (TB02272 - Nhóm trưởng):**
   - `DBUtil`: Lớp tiện ích ADO.NET cốt lõi dùng chung cho toàn bộ dự án.
   - `AiStateStoreDAL`: Quản lý truy xuất phiên hội thoại AI (`AiSessions`), tin nhắn ngữ cảnh (`AiMessages`) và các khóa trạng thái bộ nhớ AI (`AiStateStores`) phục vụ nghiệp vụ trợ lý POS thông minh (bổ sung theo nhận xét Lab 2 của Giảng viên).
2. **Ninh Văn Đức (TB02291):**
   - `CategoryDAL`: Quản lý danh mục thực đơn.
   - `ProductDAL` (alias `DrinkDAL`): Quản lý món đồ uống, giá bán, giá gốc, hình ảnh, mô tả.
3. **Trần Thị Tuyết Kha (TB02214):**
   - `CardDAL`: Quản lý thẻ bàn, thẻ rung định danh (`IsOccupied`: Trống / Đang phục vụ).
4. **Nguyễn Trí Nguyễn (TB02277):**
   - `OrderDAL` (alias `BillDAL`): Quản lý hóa đơn bán hàng, tạm tính, giảm giá, VAT, tổng tiền, phương thức thanh toán.
   - `OrderItemDAL` (alias `BillDetailDAL`): Quản lý từng dòng món trong hóa đơn, thành tiền.
5. **Nguyễn Lâm Chí Tâm (TB02234):**
   - `UserDAL`: Quản lý thông tin tài khoản nhân sự, phân quyền vai trò (0: Nhân viên, 1: Quản lý) và hàm `Login(email, password)`.

---

## PHẦN VI: KẾT QUẢ KIỂM THỬ THỰC TẾ (AUTOMATION TEST)

Nhóm đã xây dựng chương trình kiểm thử tự động tại `tests/CafePOSManagement.Checks` để kiểm tra toàn bộ luồng nghiệp vụ:

```text
=== KIỂM THỬ CÁC LỚP DAL & DBUTIL (LAB 3) ===
[CategoryDAL] Lấy thành công 7 danh mục.
[ProductDAL] Lấy thành công 16 món đồ uống.
[UserDAL] Lấy thành công 5 tài khoản nhân sự.
[UserDAL Login] Đăng nhập thử: Thành công (Nguyễn Kim Song Thư - Quản lý)
[CardDAL] Lấy thành công 5 thẻ bàn/thẻ rung.
[OrderDAL] Lấy thành công 3 hóa đơn.
[OrderItemDAL] Hóa đơn #003 có 2 chi tiết món.
[AiStateStoreDAL] Lấy thành công 2 phiên AI, 4 tin nhắn lịch sử và 4 trạng thái bộ nhớ AI.
>>> TẤT CẢ CÁC LỚP DAL & DBUTIL HOẠT ĐỘNG HOÀN HẢO 100% <<<
```
- **Tổng số ca kiểm thử logic:** 33/33 Passed.
- **Toàn bộ 7 lớp DAL & Tiện ích:** Hoạt động chính xác 100% trên CSDL SQL Server thật.

---

## PHẦN VII: HƯỚNG DẪN KHỞI CHẠY ỨNG DỤNG CHO GIẢNG VIÊN

1. Mở PowerShell tại thư mục dự án.
2. Chạy ứng dụng:
   ```powershell
   dotnet run --project src/CafePOSManagement.Web --launch-profile http
   ```
3. Mở trình duyệt truy cập:
   - Giao diện bán hàng POS: `http://localhost:5088/`
   - Giao diện Báo cáo & Kiểm thử DAL LAB 3: `http://localhost:5088/Lab3`

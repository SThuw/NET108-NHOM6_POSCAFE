# PolyCafe — Hệ thống Quản lý Bán hàng Quán Đồ uống (CafePOS)

> **Môn học:** Dự án .NET 1 (NET108) — Cao đẳng FPT Polytechnic  
> **Nhóm thực hiện:** Nhóm 6  
> **Thành viên:**  
> 1. Nguyễn Kim Song Thư (MSSV: TB02272) — Nhóm trưởng  
> 2. Ninh Văn Đức (MSSV: TB02291)  
> 3. Trần Thị Tuyết Kha (MSSV: TB02214)  
> 4. Nguyễn Trí Nguyễn (MSSV: TB02277)  
> 5. Nguyễn Lâm Chí Tâm (MSSV: TB02234)  

---

## 1. Giới thiệu dự án

**PolyCafe (CafePOS Management)** là hệ thống phần mềm quản lý bán hàng quán cà phê và đồ uống, phục vụ cho cả quầy thu ngân (POS) và công tác quản trị thực đơn, nhân sự, bàn/thẻ định danh và lịch sử đơn hàng.

Hệ thống được phát triển theo **Kiến trúc 3 Lớp (3-Layer Architecture)** kết hợp:
- **Entity Framework Core 8 (Code First)** để khởi tạo và quản lý cấu trúc cơ sở dữ liệu trên SQL Server.
- **Lớp tiện ích ADO.NET thuần (`DBUtil`)** cùng các lớp truy xuất dữ liệu chuyên biệt (`*DAL`) thực hiện đầy đủ các thao tác CRUD.
- **Trợ lý AI bán hàng (`AiStateStore`)** hỗ trợ tư vấn chọn món và gợi ý combo thông minh cho thu ngân.

---

## 2. Cấu trúc Kiến trúc 3 Lớp

```text
CafePOSManagement/
├── src/CafePOSManagement.Web/
│   ├── Controllers/             # Presentation Layer: Xử lý request từ người dùng
│   │   ├── PosController.cs     # Màn hình bán hàng POS
│   │   └── Lab3Controller.cs    # Màn hình quản trị hệ thống
│   ├── Views/                   # Giao diện người dùng Razor View & Bootstrap 5
│   │   ├── Pos/Index.cshtml     # Giao diện order món tại quầy
│   │   └── Lab3/Index.cshtml    # Giao diện quản trị danh mục, nhân sự, thẻ, hóa đơn
│   ├── DAL/                     # Data Access Layer: Thao tác dữ liệu qua DBUtil (ADO.NET)
│   │   ├── CategoryDAL.cs       # Quản lý danh mục đồ uống
│   │   ├── ProductDAL.cs        # Quản lý món đồ uống
│   │   ├── CardDAL.cs           # Quản lý thẻ bàn / thẻ rung định danh
│   │   ├── UserDAL.cs           # Quản lý nhân sự & đăng nhập
│   │   ├── OrderDAL.cs          # Quản lý hóa đơn bán hàng
│   │   ├── OrderItemDAL.cs      # Quản lý chi tiết món theo hóa đơn
│   │   └── AiStateStoreDAL.cs   # Quản lý phiên hội thoại & bộ nhớ trợ lý AI
│   ├── Data/                    # Data Layer & Entity Framework Core
│   │   ├── CafePosDbContext.cs  # DbContext quản lý ánh xạ thực thể
│   │   ├── DbInitializer.cs     # Nạp dữ liệu mẫu ban đầu (Seed Data)
│   │   └── Migrations/          # Lịch sử di chuyển schema CSDL Code First
│   ├── Utils/                   # Tiện ích dùng chung
│   │   └── DBUtil.cs            # Lớp tiện ích ADO.NET thuần (queryDataTable, executeScalar, executeNonQuery)
│   └── Models/                  # Các lớp thực thể (Entities) và DTO
└── tests/
    └── CafePOSManagement.Checks # Bộ kiểm thử tự động các nghiệp vụ và lớp DAL
```

---

## 3. Các phân hệ chức năng chính

1. **Quầy Bán hàng POS (`/Pos`):**
   - Tìm kiếm món theo tên không dấu, lọc theo danh mục nhanh chóng.
   - Thêm món vào giỏ hàng, tùy chỉnh số lượng, ghi chú đơn hàng.
   - Áp dụng mã khuyến mãi (`SALE10`), tính thuế VAT và tổng tiền tự động.
   - Chọn loại đơn (Tại bàn / Mang đi / Giao hàng) và hình thức thanh toán (Tiền mặt / Chuyển khoản QR).
2. **Quản lý Thực đơn & Danh mục (`/Lab3`):**
   - Quản lý danh mục loại nước uống.
   - Quản lý danh sách món, giá niêm yết, hình ảnh và trạng thái phục vụ.
3. **Quản lý Bàn & Thẻ định danh:**
   - Theo dõi trạng thái thẻ bàn / thẻ rung (Sẵn sàng phục vụ / Đang phục vụ khách).
4. **Quản lý Nhân sự & Phân quyền:**
   - Quản lý thông tin nhân viên, số điện thoại, email và phân quyền (Quản lý / Thu ngân).
5. **Lịch sử Hóa đơn:**
   - Truy xuất danh sách hóa đơn, chi tiết từng dòng món, tạm tính, thuế và giảm giá.
6. **Trợ lý AI Bán hàng (AI Assistant & State Store):**
   - Gợi ý combo món ăn kèm đồ uống theo sở thích của khách hàng.
   - Lưu trữ lịch sử hội thoại và bộ nhớ ngữ cảnh phiên làm việc.

---

## 4. Hướng dẫn cài đặt và khởi chạy

### Yêu cầu môi trường:
- **.NET 8 SDK** (phiên bản 8.0.x trở lên).
- **SQL Server** (LocalDB, SQL Express hoặc SQL Server 2019+).

### Các bước thực hiện:

1. **Khôi phục gói thư viện và xây dựng dự án:**
   ```powershell
   dotnet restore CafePOSManagement.sln
   dotnet build CafePOSManagement.sln
   ```

2. **Cấu hình chuỗi kết nối SQL Server (nếu cần đổi kết nối):**
   ```powershell
   dotnet user-secrets set "ConnectionStrings:CafePos" "Server=(localdb)\MSSQLLocalDB;Database=CafePOS;Integrated Security=True;TrustServerCertificate=True;" --project src/CafePOSManagement.Web
   ```

3. **Cập nhật Cơ sở dữ liệu (EF Core Code First):**
   ```powershell
   dotnet ef database update --project src/CafePOSManagement.Web
   ```

4. **Khởi chạy ứng dụng:**
   ```powershell
   dotnet run --project src/CafePOSManagement.Web --launch-profile http
   ```

5. **Truy cập ứng dụng:**
   - Giao diện Bán hàng POS: [http://localhost:5088/Pos](http://localhost:5088/Pos)
   - Giao diện Quản lý Hệ thống: [http://localhost:5088/Lab3](http://localhost:5088/Lab3)
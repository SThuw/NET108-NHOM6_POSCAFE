# BÁO CÁO THỰC HÀNH LAB 8 — TÀI LIỆU KỸ THUẬT & TRIỂN KHAI HỆ THỐNG
**Đề tài:** Hệ thống Quản lý Bán hàng Quán Đồ uống (PolyCafe - CafePOS Management)  
**Nhóm thực hiện:** Nhóm 6  
**Lớp / Khóa:** Dự án .NET 1 (NET108 — FPT Polytechnic)  
**Kho lưu trữ GitHub:** [https://github.com/SThuw/NET108-NHOM6_POSCAFE](https://github.com/SThuw/NET108-NHOM6_POSCAFE)  
**Nhóm trưởng:** Nguyễn Kim Song Thư (MSSV: TB02272)

---

## MỤC TIÊU LAB 8
✓ Hiểu vai trò và tầm quan trọng của tài liệu kỹ thuật trong dự án phần mềm.  
✓ Viết được tài liệu kỹ thuật cơ bản, chuẩn xác cho dự án Web ASP.NET Core C#.  
✓ Trình bày rõ kiến trúc hệ thống, công nghệ và cấu trúc source code thực tế của PolyCafe.  
✓ Trình bày đầy đủ các bước cài đặt, cấu hình, triển khai và kiểm tra hệ thống chạy thành công.  
✓ Ứng dụng AI để rà soát, phát hiện và sửa lỗi thuật ngữ/công nghệ trong tài liệu kỹ thuật.

---

## BÀI 1 (3 ĐIỂM): PHÂN TÍCH VÀ LẬP DANH MỤC TÀI LIỆU KỸ THUẬT

### 1.1. Xác định đối tượng đọc tài liệu kỹ thuật của dự án
Tài liệu kỹ thuật của hệ thống **PolyCafe** phục vụ 4 nhóm đối tượng chính:
1. **Lập trình viên (Developers hiện tại & thành viên mới):** Cần nắm kiến trúc 3 lớp + MVC, quy ước đặt tên, luồng dữ liệu EF Core/DAL để bảo trì hoặc phát triển thêm tính năng mà không gây xung đột.
2. **Kiểm thử viên (QA / Testers):** Cần hiểu luồng nghiệp vụ, phân quyền (Manager/Employee/Kiosk), cấu trúc CSDL và các điểm tích hợp để viết Test Plan và Test Case chính xác.
3. **Nhân sự vận hành & Triển khai (DevOps / IT Support):** Cần hướng dẫn cài đặt môi trường, chuỗi kết nối SQL Server, cấu hình SMTP và cách kiểm tra hệ thống hoạt động ổn định trên server/VPS hoặc máy trạm.
4. **Giảng viên hướng dẫn / Quản lý dự án:** Cần tài liệu tổng quan, kiến trúc và báo cáo tiến độ để đánh giá chất lượng thiết kế, mức độ hoàn thiện và tính đồng bộ của sản phẩm.

### 1.2. Danh mục tài liệu kỹ thuật cần có & Mục tiêu từng loại

| STT | Loại tài liệu kỹ thuật | File lưu trữ trong dự án | Mục tiêu của tài liệu |
| :-: | :--- | :--- | :--- |
| **1** | **Tài liệu tổng quan hệ thống** | `README.md`, `docs/BAO_CAO_LAB3.md` | Giới thiệu mục tiêu nghiệp vụ, phạm vi chức năng, đối tượng sử dụng và luồng hoạt động tổng thể của hệ thống PolyCafe. |
| **2** | **Tài liệu kiến trúc hệ thống** | `docs/BAO_CAO_LAB8.md`, `docs/ASM1_DUC_AUTH_EMPLOYEES.md` | Mô tả kiến trúc Client–Server, mô hình ASP.NET Core MVC kết hợp 3-Layer (DAL/Services/EF Core), cấu trúc source code và quy ước lập trình. |
| **3** | **Tài liệu cài đặt & triển khai** | `README.md`, `CHAY_WEB.bat`, `run.ps1` | Hướng dẫn yêu cầu phần mềm, cấu hình ConnectionStrings/SMTP, chạy EF Core Migration và cách kiểm tra ứng dụng khởi chạy thành công. |
| **4** | **Tài liệu thiết kế CSDL & Dữ liệu mẫu** | `database/CafePOS_Data_Script.sql`, `Data/Migrations/` | Lưu trữ lược đồ 10 bảng (Products, Categories, Orders, OrderItems, Users, Cards, Discounts, cụm AI State Store) và script khởi tạo dữ liệu. |
| **5** | **Tài liệu kiểm thử phần mềm** | `docs/BAO_CAO_LAB7.md`, `tests/CafePOSManagement.Checks/` | Ghi nhận Test Plan, Test Case, Bug Report và bộ 35 kịch bản kiểm thử tự động đảm bảo chất lượng phần mềm. |

---

## BÀI 2 (2 ĐIỂM): GIỚI THIỆU DỰ ÁN VÀ KIẾN TRÚC HỆ THỐNG

### 2.1. Giới thiệu dự án
- **Tên dự án:** **PolyCafe — Hệ thống Quản lý Bán hàng Quán Đồ uống (`CafePOSManagement`)**.
- **Mục tiêu nghiệp vụ:** Số hóa toàn bộ quy trình bán hàng tại quầy (POS), đặt món tự phục vụ (Kiosk), quản lý thực đơn, quản lý thẻ bàn/thẻ rung, quản lý nhân sự phân quyền, theo dõi hóa đơn và thống kê doanh thu trực quan theo thời gian thực.
- **Phạm vi chức năng chính:**
  - *Bán hàng & Kiosk:* Đặt món, tìm kiếm không dấu, giỏ hàng, tính giảm giá thành viên VIP theo SĐT & số lần ghé, thuế VAT 5%, xuất biên lai/số thứ tự.
  - *Quản trị hệ thống (Manager):* CRUD Đồ uống (kèm lọc danh mục/tìm kiếm), CRUD Danh mục, CRUD Nhân viên, CRUD Thẻ bàn, Quản lý Hóa đơn & In bill 80mm.
  - *Thống kê & AI:* Thống kê doanh thu tùy chọn theo khoảng thời gian, Top 5 thức uống bán chạy với biểu đồ `Chart.js`, và Trợ lý AI gợi ý món ăn/đồ uống.
- **Đối tượng sử dụng:**
  1. **Quản lý (Manager):** Toàn quyền truy cập quản trị thực đơn, nhân sự, thẻ bàn, hóa đơn, thống kê doanh thu và cấu hình hệ thống.
  2. **Nhân viên thu ngân (Employee):** Đăng nhập để thực hiện bán hàng tại quầy POS, xem/cập nhật trạng thái đơn và xem chi tiết hóa đơn.
  3. **Khách hàng (Kiosk Self-Service):** Tự đặt món tại màn hình Kiosk mà không cần đăng nhập tài khoản nội bộ.
- **Tổng quan luồng hoạt động hệ thống:**
  `Khách hàng / Thu ngân` ➔ Chọn món trên giao diện POS/Kiosk ➔ Server kiểm tra giá, tồn tại sản phẩm, tính giảm giá/thuế & chống trùng đơn (`RequestId`) ➔ Lưu vào SQL Server (`Orders` & `OrderItems`) ➔ Cập nhật trạng thái phục vụ ➔ Tổng hợp số liệu lên trang Quản lý Hóa đơn & Biểu đồ Thống kê (`RevenueReport`, `TopSelling`).

### 2.2. Kiến trúc hệ thống
- **Mô hình Client – Server:**
  - **Client (Trình duyệt Web):** Gửi yêu cầu HTTP/HTTPS (GET/POST) và nhận phản hồi HTML (Razor Views) hoặc JSON (AJAX `fetch`), chạy Bootstrap 5, JavaScript (`pos.js`, `kiosk.js`, `revenue-report.js`) và `Chart.js`.
  - **Server (ASP.NET Core 8.0 Web Server - Kestrel):** Tiếp nhận request qua Routing, xác thực Cookie Authentication & phân quyền Policy (`ManagerOnly`, `EmployeeOrManager`), xử lý nghiệp vụ ở tầng Controller/Service và truy xuất dữ liệu.
  - **Database (Microsoft SQL Server / LocalDB):** Lưu trữ bền vững toàn bộ dữ liệu giao dịch, tài khoản nhân sự (mật khẩu đã hash), thực đơn, thẻ bàn và lịch sử AI State Store.
- **Mô hình MVC kết hợp Kiến trúc 3 Lớp:**
  - **Model:** Thực thể dữ liệu (`Entities`: `Product`, `Category`, `Order`, `OrderItem`, `User`, `Card`, `Discount`) và `ViewModels`.
  - **View:** Giao diện Razor Views (`.cshtml`) hiển thị dữ liệu thân thiện, hỗ trợ Responsive và In ấn hóa đơn.
  - **Controller:** Tiếp nhận Request, kiểm tra tính hợp lệ (`ModelState`, `[ValidateAntiForgeryToken]`), gọi tầng `Services` / `DAL` và trả về View/JSON.
- **Nguyên tắc thiết kế áp dụng:**
  - *Separation of Concerns (Phân tách trách nhiệm):* Tách riêng tầng Giao diện (Controllers/Views), tầng Nghiệp vụ (`Services/Interfaces` & `Implementations`) và tầng Dữ liệu (`Data`, `DAL`).
  - *Dependency Injection (DI):* Đăng ký `CafePosDbContext`, `IProductService`, `IOrderService`, `IPosService` qua IoC Container trong `Program.cs`.
  - *Security & Integrity:* Mã hóa mật khẩu bằng `PasswordHasher`, chống CSRF bằng `AutoValidateAntiforgeryTokenAttribute`, chống lặp giao dịch bằng `RequestId` (Idempotency) và Soft-Delete bảo toàn hóa đơn cũ.

---

## BÀI 3 (2 ĐIỂM): MÔ TẢ CÔNG NGHỆ VÀ CẤU TRÚC SOURCE CODE

### 3.1. Danh sách công nghệ sử dụng trong dự án

| Thành phần | Công nghệ / Thư viện sử dụng trong PolyCafe |
| :--- | :--- |
| **Ngôn ngữ lập trình** | **C# 12** (.NET 8), HTML5, CSS3, JavaScript (ES6+) |
| **Framework / Công nghệ Web** | **ASP.NET Core 8.0 MVC**, Razor Views (`.cshtml`), **Entity Framework Core 8.0** (Code First), **ADO.NET** (`Microsoft.Data.SqlClient`), Bootstrap 5, **Chart.js** |
| **Application Server** | **Kestrel** (tích hợp sẵn trong .NET 8) / IIS Express / IIS |
| **Hệ quản trị Cơ sở dữ liệu** | **Microsoft SQL Server** (`LocalDB MSSQLLocalDB` / SQL Server 2019+) |
| **Công cụ hỗ trợ** | Visual Studio 2022 / VS Code, .NET 8 SDK, Git & GitHub, EF Core CLI (`dotnet ef`), PowerShell |

### 3.2. Cấu trúc source code theo mô hình MVC (`.NET Core`)
```text
D:\NET108\LAB03\
├── CafePOSManagement.sln                  # File Solution tổng quản lý toàn dự án
├── database\CafePOS_Data_Script.sql       # Script CSDL SQL Server
├── docs\                                  # Thư mục lưu tài liệu kỹ thuật & báo cáo Lab
├── src\CafePOSManagement.Web\
│   ├── Controllers\                       # [Controller] Điều hướng: Pos, Kiosk, Products, Categories, Employees, Login, Lab3, OrderDetail, RevenueReport, TopSelling
│   ├── Services\                          # [Service] Logic nghiệp vụ: DbOrderService, DbProductService, PosService, RevenueReportService, TopSellingService, EmailService
│   ├── DAL\ & Data\                       # [DAL / Repository & DbContext] CafePosDbContext (EF Core), Migrations, DbInitializer, DBUtil & các lớp *DAL (ADO.NET)
│   ├── Models\                            # [Entity & ViewModel] Entities (Order, Product, User...) và ViewModels
│   ├── Views\                             # [View - Razor .cshtml] Giao diện tương ứng từng Controller & Shared/_Layout.cshtml
│   └── wwwroot\                           # Tài nguyên tĩnh: css/, js/, images/, lib/ (bootstrap, chart.js)
└── tests\CafePOSManagement.Checks\        # Dự án kiểm thử tự động (35 Automated Checks)
```

### 3.3. Các quy ước code (Coding Conventions) quan trọng
1. **Quy ước đặt tên (Naming Convention):**
   - Tên Class, Method, Property, Controller dùng **PascalCase** (`OrderDetailController`, `GetReportAsync`).
   - Biến cục bộ, tham số hàm dùng **camelCase** (`fromDate`, `categoryId`).
   - Tên nhánh Git theo cấu trúc `feature/<ten-thanh-vien>-<ten-chuc-nang>` để tránh xung đột khi làm việc nhóm.
2. **An toàn dữ liệu & Bảo mật:**
   - Luôn sử dụng `AsNoTracking()` cho các truy vấn chỉ đọc (Read-only) để tối ưu hiệu năng EF Core.
   - Mọi form POST đều có Anti-Forgery Token; mọi truy vấn ADO.NET trong `DBUtil` phải dùng tham số (`@1`, `@2`...) để chống SQL Injection.
   - Không lưu mật khẩu ứng dụng Gmail hoặc chuỗi kết nối thật có mật khẩu lên Git công khai.

---

## BÀI 4A (2 ĐIỂM): HƯỚNG DẪN CÀI ĐẶT VÀ TRIỂN KHAI

### 4.1. Yêu cầu phần mềm
- Hệ điều hành: Windows 10/11 (hoặc Linux/macOS tương thích .NET 8).
- **.NET 8.0 SDK** (`dotnet --version` >= `8.0.x`).
- **SQL Server LocalDB** (`MSSQLLocalDB`) hoặc SQL Server 2019 trở lên.
- Trình duyệt web hiện đại: Google Chrome hoặc Microsoft Edge.

### 4.2. Các bước cài đặt & cấu hình
1. **Bước 1 — Tải mã nguồn từ GitHub:**
   ```powershell
   git clone https://github.com/SThuw/NET108-NHOM6_POSCAFE.git
   cd NET108-NHOM6_POSCAFE
   ```
2. **Bước 2 — Cấu hình chuỗi kết nối CSDL (nếu không dùng LocalDB mặc định):**
   Kiểm tra file `src/CafePOSManagement.Web/appsettings.json` mục `"ConnectionStrings": { "CafePos": "..." }`. Mặc định hệ thống tự kết nối tới `Server=.;Database=CafePOS;Integrated Security=True;TrustServerCertificate=True;` (hoặc LocalDB).
   *(Tùy chọn: Cấu hình SMTP Email qua `dotnet user-secrets` nếu cần test tính năng gửi mật khẩu qua Gmail).*
3. **Bước 3 — Khôi phục gói thư viện, Build & Cập nhật Cơ sở dữ liệu:**
   ```powershell
   dotnet restore CafePOSManagement.sln
   dotnet build CafePOSManagement.sln --no-restore
   dotnet ef database update --project src\CafePOSManagement.Web --no-build
   ```
   *(Lưu ý: Trong `Program.cs` cũng đã tích hợp sẵn cơ chế tự động chạy `Migrate()` và nạp dữ liệu mẫu ngay khi khởi động lần đầu).*
4. **Bước 4 — Khởi chạy ứng dụng:**
   Chạy lệnh sau hoặc nhấp đúp trực tiếp file `CHAY_WEB.bat` ở thư mục gốc:
   ```powershell
   dotnet run --project src\CafePOSManagement.Web --launch-profile http
   ```

### 4.3. Cách kiểm tra ứng dụng chạy thành công
1. **Kiểm tra bộ test tự động (Automation Check):**
   ```powershell
   dotnet run --project tests\CafePOSManagement.Checks\CafePOSManagement.Checks.csproj
   ```
   Kết quả hiển thị **`ALL 35 CHECKS PASSED`** là tầng nghiệp vụ và tính toán chính xác 100%.
2. **Kiểm tra trên trình duyệt Web:**
   - Truy cập `http://localhost:5088/Kiosk` ➔ Hiển thị màn hình Kiosk tự phục vụ PolyKaFé.
   - Truy cập `http://localhost:5088/Login/Login` ➔ Đăng nhập tài khoản Quản lý (`thu@polycafe.vn` / `123456`).
   - Truy cập `http://localhost:5088/Lab3` ➔ Góc trên hiển thị nhãn xanh **`SQL Server: Kết nối tốt`**.
   - Kiểm tra đầy đủ các trang: `/Pos`, `/Products`, `/Employees`, `/Lab3?tab=orders`, `/OrderDetail/1`, `/RevenueReport` và `/TopSelling` (hiển thị đầy đủ biểu đồ Chart.js).

---

## BÀI 4B (1 ĐIỂM): ỨNG DỤNG AI TRONG KIỂM TRA, ĐÁNH GIÁ TÀI LIỆU

### 4.4. Quy trình dùng AI rà soát & sửa lỗi tài liệu kỹ thuật
Nhóm đã sử dụng AI để kiểm tra chéo bản nháp tài liệu kỹ thuật của Lab 8 đối chiếu với mã nguồn thực tế trong `src/CafePOSManagement.Web`.

#### Prompt đã sử dụng:
> *"Hãy đóng vai Technical Lead kiểm tra lại toàn bộ tài liệu kỹ thuật & kiến trúc hệ thống PolyCafe vừa soạn. Đối chiếu với cấu trúc project ASP.NET Core 8 MVC thực tế, hãy chỉ ra và sửa toàn bộ các lỗi đánh máy, sai tên công nghệ, nhầm lẫn thuật ngữ giữa Java Web và .NET Core C# (nếu có)."*

#### Kết quả các lỗi AI phát hiện và đã chuẩn hóa sửa lại:

| STT | Nội dung lỗi / nhầm lẫn phát hiện ở bản nháp | Nguyên nhân | Nội dung đã sửa chuẩn theo thực tế dự án |
| :-: | :--- | :--- | :--- |
| **1** | Ghi nhầm tầng giao diện là **`View (JSP)`** | Nhầm lẫn thuật ngữ từ đề mẫu Java Servlet/JSP | Sửa thành **`View (Razor Views - .cshtml)`** |
| **2** | Ghi nhầm tầng dữ liệu là **`DAO / Repository`** | Nhầm lẫn thuật ngữ kiến trúc Java | Sửa thành **`DAL (ADO.NET DBUtil) & EF Core DbContext`** |
| **3** | Ghi nhầm Application Server là **`Apache Tomcat`** | Nhầm lẫn máy chủ chạy Java | Sửa thành **`Kestrel Web Server (.NET 8) / IIS`** |
| **4** | Ghi chung chung là **`.NET Framework`** | Không đúng phiên bản SDK của dự án | Chuẩn hóa thành **`.NET 8.0 SDK / ASP.NET Core 8.0 MVC`** |
| **5** | Thiếu tên thư viện vẽ biểu đồ và công cụ test | Chưa liệt kê đủ công nghệ mới của Lab 6 & Lab 7 | Bổ sung đầy đủ **`Chart.js`**, **`Bootstrap 5`**, **`Microsoft.Data.SqlClient`** và **`CafePOSManagement.Checks`** |

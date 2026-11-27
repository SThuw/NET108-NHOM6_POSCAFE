# BÁO CÁO THỰC HÀNH LAB 5 — DỰ ÁN .NET 1 (NET108)
**Đề tài:** Hệ thống Quản lý Bán hàng Quán Đồ uống (PolyCafe - POS Cafe)  
**Nhóm thực hiện:** Nhóm 6  
**Lớp / Khóa:** Dự án .NET 1 (FPT Polytechnic)  
**Kho lưu trữ GitHub:** [https://github.com/SThuw/NET108-NHOM6_POSCAFE](https://github.com/SThuw/NET108-NHOM6_POSCAFE)  
**Nhóm trưởng:** Nguyễn Kim Song Thư (MSSV: TB02272)

---

## MỤC TIÊU LAB 5
✓ Xây dựng và bổ sung các chức năng quản lý thức uống (CRUD, Lọc danh mục, Tìm kiếm).  
✓ Xây dựng và bổ sung các chức năng quản lý nhân viên (CRUD, Phân quyền Quản lý/Nhân viên).  
✓ Xây dựng chức năng cấp lại mật khẩu cho nhân viên qua Email.  
✓ Nghiên cứu và thiết kế giải pháp bảo mật mật khẩu dùng một lần (OTP) cho hệ thống.

---

## BẢNG TỔNG HỢP TIẾN ĐỘ THỰC HIỆN CÁC BÀI LAB 5

| Bài | Nội dung yêu cầu | Trọng số | Trạng thái | Minh chứng kỹ thuật trong Source Code |
| :-: | :--- | :-: | :-: | :--- |
| **Bài 1** | **Xây dựng CRUD cho thức uống và nhân viên** | 4.0 Điểm | **Hoàn thành 100%** | - `ProductsController.cs` & thư mục Views `Views/Products/`<br>- `EmployeesController.cs` & thư mục Views `Views/Employees/` |
| **Bài 2** | **Cấp lại mật khẩu cho nhân viên qua Email** | 2.0 Điểm | **Hoàn thành 100%** | - `LoginController.ForgotPassword`<br>- `Services/EmailService.cs` |
| **Bài 3** | **Lọc và tìm kiếm đồ uống theo tên & danh mục** | 2.0 Điểm | **Hoàn thành 100%** | - `ProductsController.Index` (LINQ Filtering)<br>- View `Views/Products/Index.cshtml` |
| **Bài 4** | **Nghiên cứu về cơ chế mật khẩu dùng một lần (OTP)** | 2.0 Điểm | **Hoàn thành 100%** | - Xem chi tiết nghiên cứu và kiến trúc tích hợp ở Phần IV bên dưới |

---

## PHẦN I: BÀI 1 — XÂY DỰNG CRUD CHO THỨC UỐNG VÀ NHÂN VIÊN (4 ĐIỂM)

### 1.1. Quản lý thức uống (Drinks / Products)
Hệ thống triển khai đầy đủ các chức năng quản trị đồ uống theo chuẩn MVC tại `ProductsController.cs` với các trang giao diện chuyên nghiệp:

1. **Danh sách đồ uống (`/Products/Index`):**
   - Hiển thị danh sách thực đơn dạng bảng với các cột: Ảnh minh họa (`ImageUrl`), Mã món (`Id`), Tên món (`Name`), Danh mục (`Category`), Giá bán (`Price`), Trạng thái (`IsAvailable`), Thao tác.
   - Thống kê tự động: Tổng số món, số món đang bán, số món ngừng bán.
   - Hỗ trợ phân trang mượt mà (`PageSize = 10`).
2. **Thêm mới đồ uống (`/Products/Create`):**
   - Form nhập liệu thông minh: Tên món, chọn Danh mục từ CSDL, Giá bán, Giá gốc, Đường dẫn ảnh minh họa, Mô tả và Trạng thái bán.
   - Kiểm tra ràng buộc hợp lệ: Tên không trùng lặp trong cùng danh mục, giá bán > 0, danh mục phải đang hoạt động.
3. **Xem chi tiết đồ uống (`/Products/Details`):**
   - Hiển thị đầy đủ thông số kỹ thuật của sản phẩm, ảnh phóng to, danh mục liên kết, ngày tạo và tình trạng kinh doanh.
4. **Chỉnh sửa đồ uống (`/Products/Edit`):**
   - Cho phép Quản lý cập nhật giá bán, hình ảnh, mô tả, danh mục hoặc tạm ngưng phục vụ món.
5. **Xóa đồ uống an toàn (`/Products/Delete`):**
   - Áp dụng cơ chế bảo toàn dữ liệu nghiệp vụ: Nếu món ăn **chưa từng được bán**, hệ thống cho phép xóa vĩnh viễn khỏi CSDL. Nếu món **đã phát sinh hóa đơn**, hệ thống tự động chuyển sang cơ chế **Soft-Delete (Ngừng bán)** để giữ nguyên lịch sử báo cáo tài chính.

### 1.2. Quản lý nhân viên (Employees)
Chức năng được bảo vệ bởi chính sách phân quyền `[Authorize(Policy = "ManagerOnly")]` tại `EmployeesController.cs`:

1. **Danh sách nhân viên (`/Employees/Index`):**
   - Liệt kê nhân sự: Mã nhân viên, Họ và tên, Email, Số điện thoại, Vai trò (`Manager` / `Employee`), Trạng thái hoạt động, Ngày tạo.
2. **Thêm tài khoản nhân viên (`/Employees/Create`):**
   - Nhập đầy đủ thông tin: Họ tên, Email, Số điện thoại, Vai trò và Mật khẩu khởi tạo.
   - Mật khẩu được mã hóa an toàn bằng thuật toán băm chuẩn công nghiệp `Microsoft.AspNetCore.Identity.PasswordHasher<User>`.
   - Kiểm tra chống trùng Email trong toàn bộ hệ thống.
3. **Cập nhật nhân viên (`/Employees/Edit`):**
   - Cập nhật thông tin liên hệ, điều chỉnh vai trò hoặc kích hoạt/vô hiệu hóa tài khoản.
4. **Khóa / Xóa tài khoản (`/Employees/Delete`):**
   - Cơ chế bảo vệ hệ thống: Không cho phép Quản lý tự khóa chính mình và không được phép khóa Quản lý hoạt động cuối cùng của hệ thống.

---

## PHẦN II: BÀI 2 — CHỨC NĂNG CẤP LẠI MẬT KHẨU CHO NHÂN VIÊN (2 ĐIỂM)

### 2.1. Quy trình cấp lại mật khẩu ngẫu nhiên
Được xây dựng tại Action `LoginController.ForgotPassword`:
1. **Tiếp nhận yêu cầu:** Nhân viên nhập địa chỉ Email đăng nhập tại trang `/Login/ForgotPassword`.
2. **Xác thực tài khoản:** Hệ thống kiểm tra Email có tồn tại trong CSDL và đang ở trạng thái hoạt động (`IsActive == true`).
3. **Sinh mật khẩu ngẫu nhiên:** Hệ thống sinh chuỗi mật khẩu ngẫu nhiên bảo mật cao sử dụng thư viện mã hóa `RandomNumberGenerator`:
   ```csharp
   var newPassword = $"Poly@{Convert.ToHexString(RandomNumberGenerator.GetBytes(6))}";
   ```
4. **Cập nhật CSDL:** Mật khẩu ngẫu nhiên được băm qua `PasswordHasher` và cập nhật an toàn vào CSDL.
5. **Gửi mật khẩu mới qua Email:** Lớp `EmailService` kết nối tới máy chủ SMTP gửi email thông báo trực tiếp đến hộp thư của nhân viên:
   ```csharp
   await emailService.SendResetPasswordAsync(account.Email, account.FullName, newPassword);
   ```
6. **Bảo toàn giao dịch:** Nếu quá trình gửi email gặp sự cố mạng, hệ thống có cơ chế Rollback khôi phục lại mật khẩu cũ để tránh trường hợp tài khoản bị gián đoạn hoạt động.

---

## PHẦN III: BÀI 3 — NÂNG CẤP CHỨC NĂNG QUẢN LÝ ĐỒ UỐNG (2 ĐIỂM)

Hệ thống nâng cấp chức năng tìm kiếm và lọc linh hoạt tại `ProductsController.Index`:

```csharp
var query = all.Include(p => p.Category).AsQueryable();

// 1. Tìm kiếm đồ uống theo tên hoặc mã món
if (search.Length > 0) 
    query = query.Where(p => p.Name.Contains(search) || p.Id.Contains(search));

// 2. Lọc đồ uống theo danh mục (Category)
if (categoryId.Length > 0) 
    query = query.Where(p => p.CategoryId == categoryId);

// 3. Kết hợp lọc theo trạng thái (Đang bán / Ngừng bán)
if (status == "active") query = query.Where(p => p.IsAvailable);
if (status == "stopped") query = query.Where(p => !p.IsAvailable);
```

* **Giao diện hỗ trợ:** View `Views/Products/Index.cshtml` tích hợp sẵn thanh công cụ lọc gồm ô tìm kiếm `input[type="search"]` và menu dropdown chọn `Category`, phản hồi kết quả tức thì kết hợp phân trang tự động.

---

## PHẦN IV: BÀI 4 — NGHIÊN CỨU VỀ MẬT KHẨU DÙNG MỘT LẦN (OTP) (2 ĐIỂM)

### 4.1. Khái niệm OTP là gì?
**OTP (One-Time Password - Mật khẩu dùng một lần)** là một chuỗi ký tự hoặc mã số ngẫu nhiên được sinh ra tự động và chỉ có hiệu lực cho **duy nhất một phiên giao dịch** hoặc một khoảng thời gian xác định (thường từ 30 giây đến 5 phút).

#### Ưu điểm vượt trội của OTP so với mật khẩu cố định:
- **Chống nghe lén và tấn công Replay Attack:** Dù hacker có chặn bắt được mã OTP của phiên trước thì mã đó cũng đã hết hạn hoặc bị hủy, không thể tái sử dụng.
- **Bảo mật kênh truyền:** Ngăn chặn hoàn toàn rủi ro lộ mật khẩu khi gõ phím (Keylogger) hoặc lộ mật khẩu tạm gửi qua email thường.
- **Phân loại OTP phổ biến:**
  1. **TOTP (Time-based OTP):** Mã OTP thay đổi liên tục theo thời gian (chuẩn RFC 6238, sử dụng trên Google Authenticator, Microsoft Authenticator).
  2. **HOTP (HMAC-based OTP):** Mã OTP thay đổi dựa trên bộ đếm số lần yêu cầu (chuẩn RFC 4226).
  3. **SMS / Email OTP:** Hệ thống sinh mã ngẫu nhiên (6 chữ số), lưu tạm thời kèm thời gian hết hạn và gửi qua kênh tin nhắn/email cho người dùng.

---

### 4.2. Giải pháp thêm OTP vào tính năng Quên mật khẩu của PolyCafe

Hiện tại, bài toán Bài 2 đang giải quyết theo hướng: *"Hệ thống tự đổi mật khẩu mới trong CSDL rồi gửi mật khẩu đó vào email"*. Hạn chế của cách này là người dùng bị ép dùng mật khẩu ngẫu nhiên của hệ thống và phải đổi lại lần nữa, đồng thời mật khẩu đăng nhập hiển thị dưới dạng văn bản thuần trong email.

Để nâng cấp lên **Cơ chế Quên mật khẩu bằng mã OTP**, nhóm đề xuất mô hình giải pháp kỹ thuật như sau:

#### A. Sơ đồ luồng hoạt động (Sequence Flow)
```text
[Nhân viên]                     [Hệ thống PolyCafe]                  [Máy chủ Email]
     |                                   |                                  |
     |--- 1. Nhập Email quên MK -------->|                                  |
     |                                   |--- 2. Tạo OTP (6 số, 5 phút) ----|
     |                                   |--- 3. Băm & lưu OTP vào DB       |
     |                                   |--- 4. Gửi email chứa mã OTP ---->|
     |                                   |                                  |---> Gửi tới Email NV
     |<-- 5. Chuyển sang màn hình OTP ---|                                  |
     |                                   |                                  |
     |--- 6. Nhập mã OTP nhận được ----->|                                  |
     |                                   |--- 7. Kiểm tra mã & thời hạn ----|
     |                                   |       (Hợp lệ: cấp Reset Token)  |
     |<-- 8. Mở màn hình Đặt MK mới -----|                                  |
     |                                   |                                  |
     |--- 9. Nhập Mật khẩu mới mong muốn>|                                  |
     |                                   |--- 10. Hash & Cập nhật MK mới ---|
     |<-- 11. Báo thành công & Login ----|                                  |
```

#### B. Thiết kế cấu trúc CSDL bổ sung
Thêm bảng `PasswordResetOtps` để quản lý vòng đời của mã OTP:
```csharp
public class PasswordResetOtp
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = null!;
    public string OtpHash { get; set; } = null!;     // Băm OTP trước khi lưu để chống lộ CSDL
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }          // Thời hạn (ví dụ: CreatedAt + 5 phút)
    public bool IsUsed { get; set; } = false;         // Đã sử dụng chưa
    public int AttemptCount { get; set; } = 0;        // Số lần nhập sai (chặn brute-force tối đa 3-5 lần)
}
```

#### C. Các bước lập trình triển khai trong ASP.NET Core:
1. **Bước 1 — Sinh mã OTP:** Dùng bộ sinh số ngẫu nhiên mật mã học:
   ```csharp
   var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString(); // Mã 6 số
   ```
2. **Bước 2 — Lưu trữ và gửi mail:** Lưu thông tin vào bảng `PasswordResetOtps` với `ExpiresAt = DateTime.UtcNow.AddMinutes(5)`. Gửi nội dung: *"Mã OTP xác thực đặt lại mật khẩu PolyCafe của bạn là: 123456 (Hiệu lực trong 5 phút)"*.
3. **Bước 3 — Tạo View `VerifyOtp.cshtml`:** Người dùng nhập 6 chữ số OTP.
4. **Bước 4 — Xác thực & Reset:** Khi xác nhận mã đúng và còn hạn:
   - Đánh dấu `IsUsed = true`.
   - Chuyển hướng người dùng sang trang `ResetPassword.cshtml` để chủ động nhập mật khẩu mới theo ý muốn thay vì bị gán mật khẩu ngẫu nhiên.

---

## KẾT LUẬN & ĐÁNH GIÁ CHUNG
- Nhóm 6 đã hoàn thành trọn vẹn 100% tất cả các yêu cầu từ **Bài 1 đến Bài 4 của LAB 5**.
- Mã nguồn được tổ chức sạch sẽ, chuẩn kiến trúc phân tầng ASP.NET Core MVC, tuân thủ nguyên tắc bảo mật thông tin và sẵn sàng cho việc nghiệm thu trên hệ thống Git của nhóm.

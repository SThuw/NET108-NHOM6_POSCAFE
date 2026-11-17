# ASM1 — Xác thực, phân quyền và quản lý nhân viên

**Phụ trách:** Ninh Văn Đức  
**Nhánh:** `feature/duc-asm1-auth-employees`

## 1. Cơ chế đăng nhập

- Hệ thống dùng ASP.NET Core Cookie Authentication.
- Email và mật khẩu được đối chiếu với tài khoản đang hoạt động trong bảng `Users`.
- Mật khẩu được lưu bằng `PasswordHasher`; dữ liệu mẫu cũ được nâng cấp sang hash sau lần đăng nhập hợp lệ.
- Cookie chứa mã tài khoản, họ tên, email và role `Manager` hoặc `Employee`.
- Mỗi request xác thực lại trạng thái hoạt động và role trong cơ sở dữ liệu. Tài khoản bị khóa hoặc đổi role sẽ bị đăng xuất.
- Các form POST dùng anti-forgery token. Reset mật khẩu bị giới hạn 5 lần trong 10 phút cho mỗi địa chỉ IP.

## 2. Ma trận phân quyền

| Chức năng | Manager | Employee | Chưa đăng nhập |
|---|:---:|:---:|:---:|
| Đăng nhập, quên mật khẩu | Có | Có | Có |
| Quầy bán hàng POS | Có | Có | Không |
| Xem và đổi mật khẩu cá nhân | Có | Có | Không |
| Quản lý nhân viên `/Employees` | Có | Không (403) | Không (chuyển tới đăng nhập) |
| Quản lý sản phẩm, danh mục | Có | Không (403) | Không (chuyển tới đăng nhập) |
| Tổng quan, hóa đơn và doanh thu `/Lab3` | Có | Không (403) | Không (chuyển tới đăng nhập) |

## 3. Quản lý nhân viên

Manager có thể xem danh sách, thêm tài khoản, sửa họ tên/email/số điện thoại/role/trạng thái và khóa tài khoản. Hệ thống chặn email trùng, tự khóa tài khoản đang đăng nhập và khóa Manager đang hoạt động cuối cùng.

## 4. Quên và đổi mật khẩu

- Quên mật khẩu tạo mật khẩu tạm ngẫu nhiên, lưu bản hash và gửi qua SMTP. Nếu gửi thất bại, hệ thống khôi phục hash cũ.
- Người dùng đã đăng nhập có thể đổi mật khẩu sau khi xác nhận đúng mật khẩu hiện tại.
- Cấu hình SMTP phải đặt trong .NET User Secrets hoặc biến môi trường; không lưu mật khẩu ứng dụng Gmail trong Git.

## 5. Minh chứng cần chụp

Ảnh kiểm thử được lưu tại `artifacts/asm1-duc-auth/` sau khi chạy kiểm thử trình duyệt:

1. `01-login.png` — giao diện đăng nhập.
2. `02-manager-employees.png` — Manager truy cập CRUD nhân viên.
3. `03-change-password.png` — giao diện đổi mật khẩu cá nhân.
4. `04-access-denied-403.png` — Employee bị từ chối chức năng Manager với HTTP 403.

## 6. Lệnh kiểm tra

```powershell
dotnet build CafePOSManagement.sln --no-restore --nologo
dotnet build tests\CafePOSManagement.Checks\CafePOSManagement.Checks.csproj --no-restore --nologo
dotnet run --project tests\CafePOSManagement.Checks\CafePOSManagement.Checks.csproj --no-build
dotnet run --project src\CafePOSManagement.Web --launch-profile http
```

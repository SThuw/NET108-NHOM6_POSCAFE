# Kiểm chứng Giai đoạn 1

Ngày thực hiện: 06/11/2026. Môi trường Windows, .NET SDK 8.0.427 cục bộ,
ASP.NET Core Runtime 8.0.33, Microsoft Edge qua Playwright CLI.

## Đã chạy thực tế

| Kiểm tra | Kết quả |
| --- | --- |
| `dotnet restore CafePOSManagement.sln` bằng SDK cục bộ | Thành công, EF Core SqlServer/Tools 8.0.33 |
| `dotnet build CafePOSManagement.sln --no-restore` | Thành công, 0 warning, 0 error; output `net8.0` |
| `dotnet run --project src/CafePOSManagement.Web --no-build --launch-profile http` | Khởi động tại http://localhost:5088, môi trường Development |
| Trang khi connection string rỗng | Chạy được vì trang khung không yêu cầu DbContext |
| `/` và `/Pos/Index` | Hiển thị trang MVC đúng, luồng Controller → Service → ViewModel → View hoạt động |
| HTTP trang chủ và `/Pos/Error` | Trang chủ trả 200; trang lỗi trả 500 với thông báo tiếng Việt, không đưa chi tiết exception ra giao diện |
| Ngôn ngữ trình duyệt gửi `en-US` | Trang vẫn tiếng Việt, HTML `lang=vi`, tiền `0 ₫` |
| Màu chủ đạo và Bootstrap | Cam `#f4511e`, navy `#2b4c7e`; Bootstrap JS nhận phiên bản 5.3.8 |
| Nút tìm món, thêm món, thanh toán | Disabled, có thông báo chưa triển khai |
| Nút Làm mới | Tải lại trang thành công |
| Logo CafePOS | Điều hướng về trang gốc |
| Liên kết Đến nội dung chính | Chuyển focus đến main bằng bàn phím |
| Responsive 1440, 1024, 768, 390, 320 px | Không overflow ngang ở các kích thước đã kiểm tra |
| Ảnh chụp desktop 1440px và mobile 390px | Đã mở xem trực tiếp, không thấy nội dung chồng lấp hoặc bị cắt |
| Browser console | 0 error, 0 warning tại lần kiểm tra |
| Network trong luồng kiểm thử | Không request thất bại; không gọi CDN hoặc dịch vụ ngoài |
| Bootstrap tải về | CSS/JS đối chiếu khớp SHA384 trong tài liệu Bootstrap chính thức |

Lần khởi động server đầu tiên trong môi trường công cụ hạn chế bị lỗi quyền ghi
Data Protection key; lần chạy lại với quyền được cho phép khởi động bình thường.
Trình duyệt Edge cũng cần chạy ngoài môi trường hạn chế để khởi động. Không sửa
cơ chế bảo vệ dữ liệu của ứng dụng để né lỗi môi trường này.

## Chưa kiểm tra và ngoài phạm vi

- Chưa xác minh SQL Server instance, đăng nhập SQL, quyền database hoặc truy vấn thực.
- Chưa tạo entity, migration, bảng dữ liệu hoặc seed. DbContext hiện chỉ cấu hình provider.
- Chưa có đăng nhập/phân quyền, CRUD thực đơn, giỏ hàng, VAT/giảm giá, thanh toán, gửi email, in hoặc báo cáo.
- Không có phép thử nhập số âm/double submit nghiệp vụ vì chưa có biểu mẫu hoạt động.
- Chưa kiểm tra Chrome, Firefox, Safari, thiết bị di động thật, máy in hoặc triển khai HTTPS production.
- Thời gian được cấu hình về Asia/Ho_Chi_Minh; đồng hồ trên trình duyệt dùng giờ hệ điều hành
  của thiết bị. Chưa có đồng bộ thời gian giao dịch vì chưa tạo giao dịch.

## Checklist tự kiểm tra

1. Chạy build/run theo README; mở http://localhost:5088.
2. Kiểm tra tiêu đề CafePOS Management, layout ba vùng ở desktop, nền trắng/xám và màu cam.
3. Kiểm tra tổng tiền `0 ₫`, thời gian Việt Nam và thông báo chưa triển khai.
4. Bấm Làm mới và logo; trang phải tải lại/điều hướng bình thường.
5. Nhấn Tab từ đầu trang, dùng liên kết Đến nội dung chính và kiểm tra viền focus.
6. Dùng DevTools đổi chiều rộng 1440, 1024, 768, 390 và 320 px; không cuộn ngang.
7. Mở Console/Network, tải lại trang; không lỗi JavaScript và không CSS/JS/ảnh bị 404.
8. Giữ connection string trống để xác nhận khung UI không yêu cầu database.

Nếu chạy lại script kiểm tra tự động trên chính máy này:

```powershell
playwright-cli -s=cafepos-stage1 open http://localhost:5088 --browser=msedge
playwright-cli -s=cafepos-stage1 run-code --filename=artifacts/stage1/browser-check.js
playwright-cli -s=cafepos-stage1 console
playwright-cli -s=cafepos-stage1 close
```

Script và ảnh chụp nằm trong `artifacts/stage1`, phục vụ kiểm chứng cục bộ và bị
loại khỏi Git. Nếu chỉ sao chép source qua Git thì dùng checklist thủ công bên trên.

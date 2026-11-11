# Giai đoạn 3 — Chức năng POS

Đã triển khai trên ASP.NET Core MVC .NET 8 và Bootstrap hiện có. Không thêm thư viện,
không đổi layout 3 cột, không tạo bảng hoặc chạy migration.

## 1. Danh sách tệp tạo mới và chỉnh sửa

Đường dẫn dưới đây tính từ `src/CafePOSManagement.Web/`, trừ các tệp test/tài liệu.

| Tệp | Trạng thái và trách nhiệm |
|---|---|
| `Models/Entities/Category.cs` | Mới: danh mục |
| `Models/Entities/Product.cs` | Mới: sản phẩm, giá bán/giá gốc, ảnh, trạng thái bán |
| `Models/Entities/DiscountCode.cs` | Mới: cấu hình mã giảm giá |
| `Models/Entities/Order.cs` | Mới: bản ghi đơn và tổng tiền |
| `Models/Entities/OrderItem.cs` | Mới: tên/giá tại lúc chốt đơn |
| `Models/ViewModels/CreateProductDto.cs` | Mới: validation thêm món bằng tiếng Việt |
| `Models/ViewModels/CheckoutOrderDto.cs` | Mới: validation đơn, chi tiết và RequestId |
| `Models/ViewModels/PosIndexViewModel.cs` | Sửa: tổng hợp danh mục, sản phẩm, giảm giá, VAT, mã đơn, thời gian |
| `Services/Interfaces/IProductService.cs` | Mới: hợp đồng thực đơn |
| `Services/Interfaces/IOrderService.cs` | Mới: hợp đồng tính tiền, tạm tính và chốt đơn |
| `Services/Implementations/ProductService.cs` | Mới: 12 món ban đầu, tìm không dấu, thêm món, khóa đồng thời |
| `Services/Implementations/OrderService.cs` | Mới: tính tiền decimal, cấp số đơn, chống gửi trùng |
| `Services/Implementations/PosService.cs` | Sửa: tổng hợp dữ liệu vào ViewModel |
| `Controllers/PosController.cs` | Sửa: API JSON và lỗi đầu vào |
| `Program.cs` | Sửa: DI và header antiforgery |
| `Views/Pos/Index.cshtml` | Sửa: dữ liệu Razor, modal thêm món, VAT và hóa đơn |
| `wwwroot/js/pos.js` | Sửa: Fetch, render giỏ, loading/error/retry, thanh toán và in |
| `wwwroot/css/site.css` | Bổ sung: VAT, disabled, bản in K80 |
| `Models/Entities/README.md`, `README.md` | Cập nhật trạng thái thực tế của dự án |
| `tests/CafePOSManagement.Checks/` | Mới: console self-check, không thêm package test |
| `tests/pos-stage3-ui.cjs` | Mới: 35 kiểm tra giao diện với Playwright CLI |
| `tests/pos-stage3-api.cjs` | Mới: 27 kiểm tra HTTP/validation/CSRF |
| `tests/pos-stage3-recovery.cjs` | Mới: 11 kiểm tra mất phản hồi, thử lại, lọc lỗi và ảnh lỗi |

Giữ tên `PosIndexViewModel` và `IPosService` đang dùng; không tạo thêm một ViewModel
trùng chức năng chỉ để đổi tên thành `PosViewModel`.

## 2. Mã nguồn chi tiết và quy tắc nghiệp vụ

[Mã nguồn đầy đủ theo từng tệp](MA-NGUON-GIAI-DOAN-3.md) chứa toàn bộ Models, DTO,
Interfaces, Services, Controller, Program, Razor, JavaScript, CSS và test đã giao.
Tệp chạy thực tế nằm trong `src/`; tài liệu là bản chụp mã nguồn tại thời điểm bàn giao.

### Luồng và API

| Route | Phương thức | Chức năng |
|---|---|---|
| `/Pos/Index` hoặc `/` | GET | Render ViewModel, thực đơn và token CSRF |
| `/Pos/Products?categoryId=tea&search=tra` | GET | Lọc theo danh mục và từ khóa đồng thời |
| `/Pos/CreateProduct` | POST JSON | Kiểm tra và thêm món, trả sản phẩm vừa lưu |
| `/Pos/Preview` | POST JSON | Tính phiếu tạm tính trên server, không tạo đơn |
| `/Pos/Checkout` | POST JSON | Ghi nhận đơn, trả `order` và `nextOrderNumber` |

POST gửi `X-CSRF-TOKEN` lấy từ token Razor, cùng cookie phiên antiforgery.
Giới hạn body 32 KiB. JavaScript chỉ gửi productId/số lượng và cấu hình đơn;
giá bán và phần trăm giảm giá được tra lại từ dữ liệu máy chủ.

Giá là số nguyên đồng từ 1 đến 10.000.000; giá gốc tùy chọn và không nhỏ hơn giá bán.
Mỗi dòng 1–99 phần, tối đa 100 món khác nhau; server chặn dòng trùng, món không tồn tại,
món ngừng bán, ghi chú quá 200 ký tự, loại đơn/phương thức thanh toán lạ.
Tên món trùng trong cùng danh mục được so sánh không dấu/không phân biệt hoa thường.

Mã giảm giá hiện có `SALE10`, `SALE20`. VAT chọn từ 0/5/8/10%, mặc định 0.
Đây là các giá trị cấu hình cho giai đoạn chức năng, không phải cơ chế xác định thuế tự động.

```text
Tạm tính = tổng(giá máy chủ × số lượng)
Giảm giá = làm tròn(Tạm tính × % giảm / 100)
VAT = làm tròn((Tạm tính − Giảm giá) × % VAT / 100)
Tổng cộng = Tạm tính − Giảm giá + VAT
```

Làm tròn đến đồng, 0,5 làm tròn lên. Server dùng decimal/AwayFromZero,
frontend dùng số nguyên trong giới hạn trên và Math.round.
Ví dụ: 2 cà phê sữa đá + 1 Cappuccino = 145.000; giảm 10% = 14.500;
VAT 8% trên 130.500 = 10.440; tổng = **140.940 ₫**.

Mã đơn đầu tiên `#004`. Mã hiển thị trước khi chốt là dự kiến, không giữ chỗ;
đơn thành công lấy mã chính thức trong lock của service. `RequestId` duy nhất cho
một lần chốt, retry cùng payload trả lại chính đơn trước đó. Khi mất phản hồi,
UI giữ giỏ, khóa sửa đơn và cho bấm **Thử xác nhận lại đơn**. Không đóng/tải lại trang
trong tình huống chưa xác nhận kết quả vì RequestId phía trình duyệt chưa được lưu bền vững.

Thêm ảnh bằng URL HTTPS hoặc để trống dùng ảnh dự phòng; không upload tệp ảnh ở giai đoạn này.
Ảnh 12 món ban đầu được giữ nguyên trong wwwroot. Tên/ghi chú được đưa vào DOM bằng
textContent và dữ liệu JSON Razor được mã hóa an toàn. Ảnh tải lỗi có fallback.

Nút in mở bản tạm tính từ máy chủ; sau checkout có thể in lại đơn vừa chốt trong cùng
trang. CSS giấy rộng 80 mm, vùng in 74 mm sau lề; PDF kiểm thử dùng trang 80 × 297 mm.
Đơn dài có thể phân trang. Máy in thực tế cần chọn khổ K80, bỏ header/footer của trình duyệt.
Ngày giờ hiển thị Việt Nam; CreatedAt lưu DateTimeOffset UTC.

## 3. Build, chạy và kịch bản kiểm thử

### Lệnh trên máy hiện tại

Mở PowerShell tại thư mục dự án. Dừng đúng phiên máy chủ đang chạy bằng Ctrl+C trước
khi build để tránh khóa EXE/DLL. Không chạy lại lệnh tạo project hoặc thêm package.

```powershell
$env:DOTNET_CLI_HOME = Join-Path (Get-Location) '.tools\cli-home'
$env:NUGET_PACKAGES = Join-Path (Get-Location) '.tools\nuget'
.\.tools\dotnet\dotnet.exe build CafePOSManagement.sln --no-restore
.\.tools\dotnet\dotnet.exe run --project src/CafePOSManagement.Web --no-build --launch-profile http
```

Mở `http://localhost:5088`. Máy có SDK .NET 8 bình thường có thể dùng:

```powershell
dotnet restore CafePOSManagement.sln
dotnet build CafePOSManagement.sln --no-restore
dotnet run --project src/CafePOSManagement.Web --no-build --launch-profile http
```

### Bộ kiểm tra tự động

Chạy kiểm tra C# khi đã dừng web server. Restore chỉ dùng các dependency có sẵn của web,
không thêm xUnit/NUnit hay package mới. Với cache cục bộ hiện có:

```powershell
.\.tools\dotnet\dotnet.exe restore tests/CafePOSManagement.Checks --source .tools/nuget
.\.tools\dotnet\dotnet.exe build tests/CafePOSManagement.Checks --no-restore
.\.tools\dotnet\dotnet.exe tests/CafePOSManagement.Checks/bin/Debug/net8.0/CafePOSManagement.Checks.dll
node --check src/CafePOSManagement.Web/wwwroot/js/pos.js
```

Trên máy khác dùng `dotnet restore tests/CafePOSManagement.Checks` với nguồn NuGet
đã cấu hình thay cho `--source .tools/nuget`.

Các script UI/API **tạo món và đơn kiểm thử trong bộ nhớ**; chạy trên tiến trình
phát triển mới khởi động, không dùng một phiên đang phục vụ bán hàng. UI script cần
12 món ban đầu. Không xóa cookie/localStorage của người dùng.

```powershell
playwright-cli -s=cafepos3 open http://localhost:5088 --browser=msedge
playwright-cli -s=cafepos3 run-code --filename=tests/pos-stage3-ui.cjs
playwright-cli -s=cafepos3 run-code --filename=tests/pos-stage3-api.cjs
playwright-cli -s=cafepos3 run-code --filename=tests/pos-stage3-recovery.cjs
playwright-cli -s=cafepos3 console
playwright-cli -s=cafepos3 requests
```

### Kịch bản kiểm tra thủ công

| ID | Thao tác | Kết quả mong đợi |
|---|---|---|
| TC01 | Mở POS mới | 12 món, giỏ trống, mã dự kiến #004 |
| TC02 | Chọn Cà phê truyền thống | 2 món, không tải lại trang |
| TC03 | Gõ `CA PHE SUA DA` | Tìm đúng Cà phê sữa đá; kết hợp danh mục hiện tại |
| TC04 | Gõ từ khóa không có rồi Xem tất cả | Empty state rồi trở về thực đơn |
| TC05 | Bấm cùng món 2 lần, thêm món khác | Gom đúng dòng, số lượng và viền cam |
| TC06 | Bấm +, − tới 0, thùng rác | Tăng/giảm/xóa đúng; viền cam cập nhật |
| TC07 | Xóa giỏ, chọn Giữ lại rồi xác nhận xóa | Hủy giữ dữ liệu; xác nhận đặt lại giỏ/loại/ghi chú/giảm/VAT/thanh toán |
| TC08 | 2 cà phê sữa đá + 1 Cappuccino, giảm 10%, VAT 8% | 145.000 − 14.500 + 10.440 = 140.940 ₫ |
| TC09 | Chọn Giao hàng, ví, nhập ghi chú | Xuất hiện đúng trên phiếu |
| TC10 | Thanh toán khi giỏ trống | Thông báo lỗi, không tạo đơn |
| TC11 | Thanh toán hoặc bấm nhanh hai lần | Một đơn; mã tiếp theo; giỏ và cấu hình trở về mặc định |
| TC12 | Thêm món tên rỗng/giá âm/giá gốc thấp/danh mục sai | Chặn ở trình duyệt hoặc trả lỗi server; không lưu |
| TC13 | Thêm món hợp lệ, ảnh trống | Món hiện ngay, có ảnh fallback, đặt được, F5 vẫn còn món |
| TC14 | In trước và sau checkout | Tạm tính chưa chốt / đơn đã ghi nhận; tên, ngày giờ, tiền đúng |
| TC15 | Desktop/Tablet/Mobile, Tab bàn phím | Không tràn ngang; focus thấy rõ; thao tác được |
| TC16 | Mất phản hồi sau khi server lưu, rồi thử lại | Giữ giỏ trong lúc chưa biết kết quả; nhận lại cùng mã đơn |
| TC17 | Món ngừng bán, sửa giá/tổng trong JSON | Không cho bán món ngừng bán; tiền tính từ server |
| TC18 | Lỗi lọc hoặc lỗi ảnh | Hết loading, có hướng dẫn thử lại/ảnh dự phòng |

### Kết quả đã kiểm chứng ngày 07/11/2026

- `dotnet build CafePOSManagement.sln --no-restore`: thành công, 0 lỗi, 0 cảnh báo.
- Console self-check C#: **33/33**, gồm làm tròn, validation, 20 retry đồng thời
  cùng key và 20 đơn độc lập đồng thời.
- Playwright Edge UI: **35/35**; kích thước 1440/1024/768/390 px không tràn ngang.
- HTTP API: **27/27**, gồm giả giá/tổng/giảm giá, CSRF, 12 retry đồng thời, null/JSON sai.
- Giả lập lỗi và phục hồi: **11/11**, gồm mất phản hồi sau ghi nhận, giữ RequestId,
  retry không tạo thêm đơn, lỗi lọc và fallback ảnh.
- Đã xem ảnh desktop, mobile và phiếu in; đã tạo PDF K80. Không có pageerror JavaScript.
- Console/Network có các lỗi 400/404/ERR_CONNECTION_RESET chủ động tạo bởi kịch bản
  validation/lỗi mạng/ảnh lỗi, không coi các lỗi dự kiến này là lỗi luồng hợp lệ.
- Môi trường sandbox ban đầu chặn cache Playwright, Windows Event Log và NuGet.Config;
  đã chạy lại với quyền mở rộng để hoàn thành kiểm thử.

Ảnh/PDF nằm trong `artifacts/stage3-desktop.png`, `stage3-mobile.png`,
`stage3-receipt.png`, `stage3-receipt.pdf` (thư mục artifacts được gitignore từ trước).

### Giới hạn còn lại

- Chưa có SQL Server/migration; dữ liệu, mã đơn và chống trùng chỉ tồn tại trong một
  tiến trình. Không dùng nhiều instance. Danh sách tăng theo số món/đơn đến khi restart.
- Chưa có đăng nhập/phân quyền; chỉ chạy cục bộ. CSRF không thay thế xác thực.
- Ghi nhận phương thức thanh toán nội bộ, chưa tích hợp ngân hàng/ví hoặc hóa đơn điện tử.
- Giỏ và hóa đơn vừa chốt không lưu qua F5; không thay đổi localStorage/cookie người dùng.
- Đã xuất bản in/PDF và kiểm tra lời gọi window.print; chưa thử máy in nhiệt vật lý
  hay hộp thoại in hệ điều hành. URL ảnh HTTPS do người dùng nhập sẽ được tải bởi trình duyệt.

# Giai đoạn 2 — Giao diện POS CafePOS Management

## 1. Danh sách tệp và phạm vi

Các tệp đã sửa:
- `src/CafePOSManagement.Web/Views/Shared/_Layout.cshtml`
- `src/CafePOSManagement.Web/Views/Pos/Index.cshtml`
- `src/CafePOSManagement.Web/wwwroot/css/site.css`
- `src/CafePOSManagement.Web/wwwroot/js/site.js`

Các tệp mới:
- `src/CafePOSManagement.Web/Views/Shared/_PosIcons.cshtml`
- `src/CafePOSManagement.Web/wwwroot/js/pos.js`
- `src/CafePOSManagement.Web/wwwroot/images/product-fallback.svg`
- Tám ảnh JPG trong `src/CafePOSManagement.Web/wwwroot/images/menu/`.
- Tài liệu này. README được cập nhật đường dẫn đến hướng dẫn giai đoạn 2.

Không sửa Controller, ViewModel, Service Layer, Program.cs, DbContext, csproj hoặc cấu hình SQL Server.
SHA256 Program.cs và DbContext trước/sau trùng nhau. Không cài thêm thư viện.

## 2. Thiết kế và tương tác đã triển khai

- Header có thương hiệu CafePOS, tìm kiếm, xóa giỏ, thêm món, làm mới, máy tính, thông báo, in và quay lại.
- Desktop có ba vùng: danh mục / cấu hình đơn và lưới món / giỏ hàng. Lưới 4 cột từ 1351px; thu còn 3 hoặc 2 cột khi hẹp hơn. Danh sách món và giỏ cuộn độc lập.
- Tablet chuyển danh mục thành hàng ngang có cuộn; footer giỏ thu gọn ở màn hình thấp để không cắt nút thanh toán.
- Mobile xếp dọc, có liên kết cố định đến giỏ hàng. Màn hình desktop rất thấp cho phép cuộn trang để vẫn truy cập mọi thao tác.
- Ba kiểu phục vụ và ba phương thức thanh toán có trạng thái chọn rõ ràng. Phương thức thanh toán dùng radio hỗ trợ bàn phím.
- Tìm kiếm bỏ qua dấu tiếng Việt, chữ hoa/thường và khoảng trắng đầu/cuối; kết hợp với danh mục đang chọn. Không tìm thấy có nút đặt lại bộ lọc.
- Thêm món, tăng/giảm 1–99 phần, xóa từng món, xác nhận xóa giỏ, trạng thái giỏ rỗng; cập nhật viền card và số lượng ngay.
- Tiền tính bằng số nguyên VNĐ, giảm 10%/20% làm tròn đến đồng. VAT 0% chỉ là dữ liệu trình diễn theo yêu cầu UI. VIP chưa có quy tắc nên option bị vô hiệu hóa.
- Ghi chú giới hạn 200 ký tự, đưa vào bản xem trước bằng textContent để không chèn HTML.
- Modal thêm món là biểu mẫu tĩnh, nút lưu vô hiệu hóa và ghi rõ chưa triển khai. Chọn ảnh chỉ hiển thị tên tệp, kiểm định dạng JPG/PNG tại trình duyệt, không upload.
- Máy tính nhẩm cộng/trừ/nhân/chia, chặn chia 0, không dùng eval. Đây là tiện ích nhẩm, không dùng kết quả để tính tiền giỏ.
- Thông báo có dropdown trạng thái rỗng. Quay lại chỉ dùng lịch sử cùng website; nếu không có thì thông báo đang ở màn hình đầu tiên.
- In hóa đơn/Thanh toán mở bản xem trước có nhãn BẢN MẪU · CHƯA THANH TOÁN. Nút In bản mẫu gọi hộp thoại in, không thông báo máy in đã in thành công.

### Dữ liệu và giới hạn

`pos.js` tập trung 8 mục danh mục (gồm Tất cả), 12 món mẫu và state của trang.
Giỏ ban đầu có 3 món mẫu, tổng 145.000 ₫ để thể hiện đầy đủ bố cục. Nút Làm mới trong
header giữ giỏ và cấu hình; tải lại trang bằng F5 hoặc đóng/mở lại sẽ khởi tạo mẫu lại.
Không dùng localStorage, không đọc hoặc xóa dữ liệu phiên sẵn có của người dùng.
Không có API thanh toán, ghi đơn vào SQL Server, xác thực, phân quyền hoặc tích hợp thẻ/ví.
Giai đoạn 3 phải thay dữ liệu trình diễn bằng luồng nghiệp vụ, kiểm quyền và tính tiền lại phía server.

Chưa có ảnh tham chiếu gốc trong chat để đối chiếu từng pixel; bản này bám thông số bằng chữ.
Tên "Đơn mẫu #003" là định danh minh họa, không phải mã đơn sinh từ database.

### Nguồn ảnh

Ảnh tải trực tiếp từ Unsplash, đã kiểm tra tải thành công và mở xem nội dung. Ảnh lưu
cục bộ 600 × 600, có alt, lazy loading và ảnh SVG dự phòng. Một số ảnh được tái sử dụng
cho các món gần chủ đề; đây là ảnh minh họa, không phải ảnh sản phẩm thật của quán.
Không dùng logo/tài sản OrderZ. Icon là SVG vẽ trong project, không cài thư viện icon.

| Tệp | URL nguồn |
| --- | --- |
| coffee.jpg | https://images.unsplash.com/photo-1509042239860-f550ce710b93 |
| latte.jpg | https://images.unsplash.com/photo-1461023058943-07fcbe16d735 |
| iced.jpg | https://images.unsplash.com/photo-1517701604599-bb29b565090c |
| espresso.jpg | https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd |
| croissant.jpg | https://images.unsplash.com/photo-1555507036-ab1f4038808a |
| sandwich.jpg | https://images.unsplash.com/photo-1528735602780-2552fd46c7af |
| tea.jpg | https://images.unsplash.com/photo-1544787219-7f47ccb76574 |
| fruit-tea.jpg | https://images.unsplash.com/photo-1556679343-c7306c1976bc |

Tham số tải: `?auto=format&fit=crop&w=600&h=600&q=85`.

## 3. Build, chạy và kiểm thử

**Dự án đã có sẵn: không chạy `dotnet new`, không dùng `--force`.**
Dừng phiên CafePOS cũ bằng Ctrl+C trước khi build để tránh lỗi MSB3027/MSB3021 khóa file.

Trên máy hiện tại, mở PowerShell trong thư mục chứa CafePOSManagement.sln:

```powershell
$env:DOTNET_CLI_HOME = Join-Path (Get-Location) '.tools\cli-home'
$env:NUGET_PACKAGES = Join-Path (Get-Location) '.tools\nuget'
.\.tools\dotnet\dotnet.exe build CafePOSManagement.sln --no-restore
.\.tools\dotnet\dotnet.exe run --project src/CafePOSManagement.Web --no-build --launch-profile http
```

Mở http://localhost:5088. Giữ terminal mở; Ctrl+C để dừng.
Trên máy đã cài .NET 8 SDK toàn hệ thống, thay `.\.tools\dotnet\dotnet.exe` bằng `dotnet`.
Nếu chưa restore trên máy mới, chạy `dotnet restore CafePOSManagement.sln` trước.

### Kết quả thực tế ngày 06/11/2026

- Build cuối bằng SDK 8.0.427: **0 warning, 0 error**.
- Chạy thành công trên ASP.NET Core Runtime 8.0.33, giữ target net8.0.
- Kiểm thử bằng Microsoft Edge và Playwright CLI, không dùng phiên trình duyệt cá nhân.
- 8 kích thước: 1600×1000, 1440×900, 1366×768, 1280×800, 1024×768, 768×1024, 390×844, 320×740. Không tràn ngang; desktop/tablet không bị cắt nút thanh toán.
- Đã mở xem ảnh chụp desktop, tablet, mobile, modal thêm món và chế độ CSS in.
- 145.000 ₫ ban đầu → thêm cà phê sữa 190.000 ₫ → giảm số lượng 145.000 ₫; giảm 10% = 130.500 ₫; giảm 20% = 116.000 ₫; xóa Croissant trong khi giảm 20% còn 80.000 ₫.
- Đạt: tìm không dấu, từ khóa rỗng/không khớp, chọn danh mục, loại đơn, phương thức; min/max số lượng; click nhanh; xóa/hủy xóa; giỏ rỗng chặn thanh toán.
- Đạt: ghi chú HTML không được thực thi; modal mở/đóng/Escape; máy tính cộng và chặn chia 0; thông báo; làm mới giữ state.
- Đạt: giả lập HTTP 404 của ảnh trong phiên test, SVG dự phòng tải đúng. Lỗi 404 này được tạo có chủ đích và không phải lỗi tài nguyên thật.
- Đạt: chọn danh mục bằng bàn phím, liên kết giỏ trên mobile, chỉnh số lượng và mở modal mobile.
- Đạt: tên tệp ảnh hợp lệ được hiển thị, tệp TXT bị từ chối, nhãn chọn ảnh tiếng Việt.
- Luồng bình thường không lỗi JavaScript hoặc request tài nguyên thất bại. Trang lỗi dùng chung vẫn trả 500 và hiển thị thông báo tiếng Việt; đây là mã phản hồi chủ đích.
- Đã kiểm tra nút in gọi window.print bằng cách thay hộp thoại thật bằng hàm theo dõi trong test; đã kiểm tra CSS in qua emulation. **Chưa in ra máy in vật lý.**

### Kịch bản tự kiểm tra nhanh

1. Mở POS, xác nhận 12 món, 3 món đã chọn, tổng 145.000 ₫ và nhãn Dữ liệu mẫu.
2. Chọn Cà phê truyền thống: còn 2 món. Tìm `ca phe sua`: còn 1 món. Tìm `khong-co`: hiện trạng thái không kết quả. Bấm Xem tất cả món.
3. Bấm một thẻ, tăng/giảm/xóa trong giỏ; kiểm tra viền cam và tổng tiền. Không giảm số lượng xuống 0; muốn bỏ món dùng thùng rác.
4. Chuyển Mang đi/Giao hàng/Tại bàn và Tiền mặt/Thẻ/Ví. Mỗi nhóm chỉ có một lựa chọn.
5. Chọn Giảm 10%, Giảm 20% và đối chiếu tiền. VIP chưa cấu hình nên không chọn được.
6. Nhập ghi chú, bấm Thanh toán ngay: chỉ mở bản xem trước, không trừ tiền/lưu đơn. Thử In hóa đơn để xem cùng nội dung.
7. Mở Thêm món mới; nhập thông tin, chọn ảnh JPG/PNG; nút lưu vẫn disabled. Escape đóng modal.
8. Xóa giỏ: thử Giữ lại rồi xác nhận xóa; tổng về 0 ₫, nút thanh toán/in vô hiệu hóa.
9. Kiểm tra viewport ở desktop/tablet/mobile, cuộn danh mục ngang trên tablet/mobile và bấm Xem giỏ hàng trên mobile.
10. Mở Console/Network, reload trang để kiểm tra tài nguyên. Ảnh đã lưu trong project không yêu cầu mạng ngoài.

Chưa kiểm tra máy in thật, giao dịch/database, trình duyệt Safari/Firefox hoặc thiết bị
cảm ứng thật. Các bài thử tự động và ảnh nằm trong `artifacts/stage2` (không đưa vào Git).

## 4. Mã nguồn đầy đủ từng tệp UI

Nội dung dưới đây được lấy trực tiếp từ các tệp của bản giao. Ảnh nhị phân nằm ở thư mục
images/menu theo danh sách nguồn phía trên; không chép dữ liệu nhị phân vào tài liệu.


### src/CafePOSManagement.Web/Views/Shared/_Layout.cshtml

```cshtml
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="theme-color" content="#FF5722" />
    <title>@ViewData["Title"] · CafePOS Management</title>
    <link rel="icon" type="image/svg+xml" href="~/images/cafepos-mark.svg" />
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" asp-append-version="true" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body class="@ViewData["PageClass"]">
    <partial name="_PosIcons" />
    <a class="skip-link" href="#main-content">Đến nội dung chính</a>
    <header class="app-header">
        <a class="brand" asp-controller="Pos" asp-action="Index" aria-label="CafePOS — Trang bán hàng">
            <img src="~/images/cafepos-mark.svg" width="40" height="40" alt="" />
            <span>Cafe<span class="brand-accent">POS</span><small>QUẢN LÝ BÁN HÀNG</small></span>
        </a>
        @if (IsSectionDefined("PosToolbar"))
        {
            @await RenderSectionAsync("PosToolbar", required: false)
        }
    </header>
    <main id="main-content" class="app-main" tabindex="-1">@RenderBody()</main>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js" asp-append-version="true" defer></script>
    <script src="~/js/site.js" asp-append-version="true" defer></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>

```

### src/CafePOSManagement.Web/Views/Shared/_PosIcons.cshtml

```cshtml
<svg xmlns="http://www.w3.org/2000/svg" class="icon-sheet" aria-hidden="true">
    <symbol id="i-search" viewBox="0 0 24 24"><circle cx="10.5" cy="10.5" r="6.5"/><path d="m16 16 5 5"/></symbol>
    <symbol id="i-refresh" viewBox="0 0 24 24"><path d="M20 10a8 8 0 1 0-1 7M20 4v6h-6"/></symbol>
    <symbol id="i-calculator" viewBox="0 0 24 24"><rect x="5" y="2" width="14" height="20" rx="2"/><path d="M8 6h8M8 11h1m6 0h1m-8 4h1m6 0h1m-8 4h1m6 0h1"/></symbol>
    <symbol id="i-bell" viewBox="0 0 24 24"><path d="M5 9a7 7 0 0 1 14 0c0 8 3 8 3 8H2s3 0 3-8m4 11a3 3 0 0 0 6 0"/></symbol>
    <symbol id="i-print" viewBox="0 0 24 24"><path d="M6 8V2h12v6M6 17H3V8h18v9h-3M6 14h12v8H6zM17 11h1"/></symbol>
    <symbol id="i-back" viewBox="0 0 24 24"><path d="m10 5-7 7 7 7M3 12h18"/></symbol>
    <symbol id="i-arrow" viewBox="0 0 24 24"><path d="m9 5 7 7-7 7"/></symbol>
    <symbol id="i-grid" viewBox="0 0 24 24"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></symbol>
    <symbol id="i-cup" viewBox="0 0 24 24"><path d="M4 8h13v7a6.5 6.5 0 0 1-13 0V8Zm13 1h2a3 3 0 0 1 0 6h-2M3 22h16M8 2v3m5-3v3"/></symbol>
    <symbol id="i-bag" viewBox="0 0 24 24"><path d="M4 7h16l1 15H3L4 7Zm4 1V5a4 4 0 0 1 8 0v3"/></symbol>
    <symbol id="i-bike" viewBox="0 0 24 24"><circle cx="5" cy="17" r="4"/><circle cx="19" cy="17" r="4"/><path d="m5 17 5-10 5 10h4L14 4h4M8 7h5"/></symbol>
    <symbol id="i-table" viewBox="0 0 24 24"><path d="M3 7h18v5H3zM6 12v10m12-10v10M8 3h8"/></symbol>
    <symbol id="i-trash" viewBox="0 0 24 24"><path d="M3 6h18M9 6V3h6v3M5 6l1 15h12l1-15M10 10v7m4-7v7"/></symbol>
    <symbol id="i-cash" viewBox="0 0 24 24"><rect x="2" y="5" width="20" height="14" rx="2"/><circle cx="12" cy="12" r="3"/><path d="M5 9v6m14-6v6"/></symbol>
    <symbol id="i-card" viewBox="0 0 24 24"><rect x="2" y="4" width="20" height="16" rx="3"/><path d="M2 9h20M6 15h4"/></symbol>
    <symbol id="i-wallet" viewBox="0 0 24 24"><path d="M20 7V4H5a3 3 0 0 0 0 6h17v10H5a3 3 0 0 1-3-3V7m20 6h-6v4h6m-3-2h.01"/></symbol>
    <symbol id="i-check" viewBox="0 0 24 24"><path d="m5 12 4 4L20 5"/></symbol>
    <symbol id="i-note" viewBox="0 0 24 24"><path d="M14 3H4v18h16V11M9 15l2-5 9-9 3 3-9 9-5 2Z"/></symbol>
</svg>

```

### src/CafePOSManagement.Web/Views/Pos/Index.cshtml

```cshtml
@model PosIndexViewModel
@{
    ViewData["Title"] = "Bán hàng";
    ViewData["PageClass"] = "pos-page";
}
@section PosToolbar {
    <div class="header-search"><svg class="icon"><use href="#i-search" /></svg><label class="visually-hidden" for="product-search">Tìm kiếm đồ uống, món ăn</label><input id="product-search" type="search" placeholder="Tìm kiếm đồ uống, món ăn..." maxlength="100" autocomplete="off" /></div>
    <div class="header-actions">
        <button class="clear-link" id="clear-cart" type="button">Xóa giỏ hàng</button>
        <button class="btn btn-primary add-product" type="button" data-bs-toggle="modal" data-bs-target="#add-product-modal"><span aria-hidden="true">+</span> Thêm món mới</button>
        <div class="utility-actions">
            <button class="icon-button" id="refresh-pos" title="Làm mới giao diện, giữ giỏ hàng" aria-label="Làm mới" type="button"><svg class="icon"><use href="#i-refresh" /></svg></button>
            <button class="icon-button" title="Máy tính nhẩm" aria-label="Máy tính nhẩm" type="button" data-bs-toggle="modal" data-bs-target="#calculator-modal"><svg class="icon"><use href="#i-calculator" /></svg></button>
            <div class="dropdown"><button class="icon-button" title="Thông báo" aria-label="Thông báo" type="button" data-bs-toggle="dropdown" aria-expanded="false"><svg class="icon"><use href="#i-bell" /></svg></button><div class="dropdown-menu dropdown-menu-end notification-menu"><strong>Thông báo</strong><p>Chưa có thông báo mới.</p><small>Bạn đang sử dụng bản xem trước giao diện.</small></div></div>
        </div>
        <button class="btn btn-outline-secondary" id="print-receipt" type="button"><svg class="icon"><use href="#i-print" /></svg><span>In hóa đơn</span></button>
        <button class="btn btn-outline-secondary back-button" id="go-back" type="button"><svg class="icon"><use href="#i-back" /></svg><span>Quay lại</span></button>
    </div>
}
<div class="workspace-bar"><div><span class="live-dot" aria-hidden="true"></span><h1>Bán hàng tại quầy</h1><span class="workspace-divider">/</span><span class="workspace-caption">Một ngày mới, những ly cà phê ngon.</span></div><span class="demo-badge">Bản xem trước · Dữ liệu mẫu</span></div>
<div class="pos-workspace" id="pos-app" data-image-root="@Url.Content("~/images/menu/")" data-fallback-image="@Url.Content("~/images/product-fallback.svg")">
    <aside class="categories-panel surface" aria-labelledby="categories-title">
        <div class="sidebar-heading"><span class="eyebrow">THỰC ĐƠN</span><h2 id="categories-title">Danh mục món</h2></div>
        <nav id="category-list" class="category-list" aria-label="Danh mục thực đơn"></nav>
        <div class="sidebar-bottom"><svg class="icon"><use href="#i-cup" /></svg><div><strong>Được pha bằng cả tâm huyết</strong><p>Mỗi món, một chút niềm vui.</p></div></div>
    </aside>
    <section class="menu-panel" aria-labelledby="menu-title">
        <div class="order-controls surface">
            <div class="order-type-field"><span class="field-label" id="order-type-label">Loại đơn hàng</span><div class="segmented" role="group" aria-labelledby="order-type-label" id="order-types">
                <button type="button" data-order="pickup" class="active" aria-pressed="true"><svg class="icon"><use href="#i-bag" /></svg>Mang đi</button>
                <button type="button" data-order="delivery" aria-pressed="false"><svg class="icon"><use href="#i-bike" /></svg>Giao hàng</button>
                <button type="button" data-order="dinein" aria-pressed="false"><svg class="icon"><use href="#i-table" /></svg>Tại bàn</button>
            </div></div>
            <div class="discount-field"><label class="field-label" for="discount">Giảm giá</label><select id="discount" class="form-select"><option value="0">Chọn mã giảm</option><option value="10">Giảm 10%</option><option value="20">Giảm 20%</option><option value="vip" disabled>Khách VIP — chưa cấu hình</option></select></div>
            <div class="note-field"><label class="field-label" for="order-note">Ghi chú đơn hàng</label><input class="form-control" id="order-note" type="text" maxlength="200" placeholder="Nhập ghi chú (ít đá, giảm ngọt...)" /></div>
        </div>
        <div class="menu-heading"><div><h2 id="menu-title">Tất cả món</h2><span id="product-count">Đang tải thực đơn...</span></div><span class="menu-tip"><svg class="icon"><use href="#i-bag" /></svg>Chọn món để thêm vào giỏ</span></div>
        <div id="product-scroll" class="product-scroll" tabindex="0" aria-label="Danh sách món, có thể cuộn"><div id="product-grid" class="product-grid" aria-busy="true"><p class="loading-message">Đang tải món mẫu...</p></div><div id="no-results" class="empty-state" hidden><svg class="icon"><use href="#i-search" /></svg><h3>Không tìm thấy món phù hợp</h3><p>Thử từ khóa khác hoặc xem tất cả món.</p><button class="btn btn-outline-secondary" id="reset-search" type="button">Xem tất cả món</button></div></div>
        <div class="menu-footnote">Ảnh minh họa từ Unsplash <span>Giá và món chỉ phục vụ xem trước giao diện</span></div>
    </section>
    <aside class="cart-panel surface" id="cart-panel" aria-labelledby="cart-title">
        <div class="cart-heading"><div><h2 id="cart-title">Giỏ hàng <span id="cart-count">0</span></h2><p>Đơn hàng hiện tại</p></div><div class="order-meta"><strong>Đơn mẫu #003</strong><time data-local-clock datetime="@Model.CurrentTime.ToString("O")">@Model.CurrentTime.ToString("dd/MM/yyyy HH:mm")</time></div></div>
        <div class="cart-scroll" tabindex="0" aria-label="Các món trong giỏ"><div id="cart-items"></div><div id="cart-empty" class="empty-state" hidden><svg class="icon"><use href="#i-bag" /></svg><h3>Giỏ hàng trống</h3><p>Chọn một món từ thực đơn để bắt đầu.</p></div></div>
        <div class="cart-bottom">
            <dl class="order-summary"><div><dt>Tạm tính</dt><dd id="subtotal">0 ₫</dd></div><div id="discount-row" hidden><dt id="discount-label">Giảm giá</dt><dd id="discount-amount">0 ₫</dd></div><div><dt>Thuế VAT (0%)</dt><dd>0 ₫</dd></div><div class="total-row"><dt>Tổng cộng</dt><dd id="total" data-testid="cart-total">0 ₫</dd></div></dl>
            <fieldset class="payment-field"><legend>Phương thức thanh toán</legend><div class="payment-options" id="payment-options">
                <label class="payment-option"><input type="radio" name="payment" value="cash" checked /><span><svg class="icon"><use href="#i-cash" /></svg>Tiền mặt</span></label>
                <label class="payment-option"><input type="radio" name="payment" value="card" /><span><svg class="icon"><use href="#i-card" /></svg>Thẻ ngân hàng</span></label>
                <label class="payment-option"><input type="radio" name="payment" value="wallet" /><span><svg class="icon"><use href="#i-wallet" /></svg>Ví điện tử</span></label>
            </div></fieldset>
            <button class="btn btn-primary checkout-button" id="checkout" type="button">Thanh toán ngay <svg class="icon"><use href="#i-arrow" /></svg></button>
            <p class="preview-note">Chỉ xem trước · Chưa ghi nhận thanh toán</p>
        </div>
    </aside>
</div>
<a href="#cart-panel" class="mobile-cart-link"><svg class="icon"><use href="#i-bag" /></svg>Xem giỏ hàng <strong id="mobile-total">0 ₫</strong></a>
<div id="pos-status" class="pos-toast" role="status" aria-live="polite" aria-atomic="true"></div>
<noscript><p class="alert alert-warning">Vui lòng bật JavaScript để xem thực đơn và thử giao diện POS.</p></noscript>
<template id="product-template"><button class="product-card" type="button"><div class="product-image"><img width="600" height="600" loading="lazy" /><span class="selected-mark" aria-hidden="true"><svg class="icon"><use href="#i-check" /></svg></span></div><div class="product-info"><span class="product-name"></span><div class="product-prices"><del></del><strong></strong></div><span class="product-add" aria-hidden="true">+</span></div></button></template>
<template id="cart-template"><article class="cart-item"><img class="cart-thumbnail" width="44" height="44" alt="" /><div class="cart-item-detail"><h3></h3><span class="cart-unit-price"></span><div class="quantity-control"><button type="button" data-action="decrease">−</button><span class="quantity"></span><button type="button" data-action="increase">+</button></div></div><div class="cart-item-end"><button class="remove-item icon-button" type="button" data-action="remove"><svg class="icon"><use href="#i-trash" /></svg></button><strong class="line-total"></strong></div></article></template>
<div class="modal fade" id="add-product-modal" tabindex="-1" aria-labelledby="add-product-title" aria-hidden="true"><div class="modal-dialog modal-dialog-centered"><div class="modal-content"><div class="modal-header"><div><span class="eyebrow">THỰC ĐƠN CỦA QUÁN</span><h2 class="modal-title fs-5" id="add-product-title">Thêm món mới</h2></div><button class="btn-close" type="button" data-bs-dismiss="modal" aria-label="Đóng"></button></div><div class="modal-body"><p class="form-preview-note">Biểu mẫu xem trước. Chức năng lưu món sẽ được triển khai ở giai đoạn sau.</p><div class="mb-3"><label class="form-label" for="new-name">Tên món</label><input class="form-control" id="new-name" placeholder="Ví dụ: Cà phê sữa đá" maxlength="100" /></div><div class="row g-3 mb-3"><div class="col-sm-6"><label class="form-label" for="new-category">Danh mục</label><select id="new-category" class="form-select"></select></div><div class="col-sm-6"><label class="form-label" for="new-price">Giá bán (₫)</label><input class="form-control" id="new-price" type="number" min="1" step="1" placeholder="45.000" /></div></div><div class="mb-2"><label class="form-label" for="new-image">Hình ảnh minh họa</label><div class="file-picker"><input class="visually-hidden" type="file" id="new-image" accept="image/jpeg,image/png" aria-describedby="image-file-name" /><label class="btn btn-outline-secondary" for="new-image">Chọn ảnh</label><span id="image-file-name" aria-live="polite">Chưa chọn tệp</span></div><small class="text-secondary">JPG hoặc PNG. Tệp chưa được tải lên máy chủ.</small></div></div><div class="modal-footer"><button class="btn btn-outline-secondary" type="button" data-bs-dismiss="modal">Đóng</button><button class="btn btn-primary" type="button" disabled>Lưu món — chưa triển khai</button></div></div></div></div>
<div class="modal fade" id="clear-modal" tabindex="-1" aria-labelledby="clear-title" aria-hidden="true"><div class="modal-dialog modal-dialog-centered modal-sm"><div class="modal-content"><div class="modal-header"><h2 class="modal-title fs-5" id="clear-title">Xóa giỏ hàng?</h2><button class="btn-close" type="button" data-bs-dismiss="modal" aria-label="Đóng"></button></div><div class="modal-body">Các món đang chọn sẽ được bỏ khỏi giỏ hàng mẫu.</div><div class="modal-footer"><button class="btn btn-outline-secondary" type="button" data-bs-dismiss="modal">Giữ lại</button><button class="btn btn-primary" id="confirm-clear" type="button">Xóa tất cả món</button></div></div></div></div>
<div class="modal fade" id="calculator-modal" tabindex="-1" aria-labelledby="calculator-title" aria-hidden="true"><div class="modal-dialog modal-dialog-centered modal-sm"><div class="modal-content"><div class="modal-header"><h2 class="modal-title fs-5" id="calculator-title">Máy tính nhẩm</h2><button class="btn-close" type="button" data-bs-dismiss="modal" aria-label="Đóng"></button></div><div class="modal-body"><form id="calculator-form"><label for="calc-a" class="form-label">Số thứ nhất</label><input class="form-control mb-3" id="calc-a" type="number" step="any" required /><label for="calc-operation" class="form-label">Phép tính</label><select id="calc-operation" class="form-select mb-3"><option value="add">Cộng (+)</option><option value="subtract">Trừ (−)</option><option value="multiply">Nhân (×)</option><option value="divide">Chia (÷)</option></select><label for="calc-b" class="form-label">Số thứ hai</label><input class="form-control mb-3" id="calc-b" type="number" step="any" required /><button class="btn btn-primary w-100" type="submit">Tính kết quả</button><output class="calculator-result" id="calc-result" aria-live="polite">Nhập hai số để bắt đầu.</output></form></div></div></div></div>
<div class="modal fade" id="receipt-modal" tabindex="-1" aria-labelledby="receipt-title" aria-hidden="true"><div class="modal-dialog modal-dialog-centered modal-dialog-scrollable"><div class="modal-content"><div class="modal-header"><h2 class="modal-title fs-5" id="receipt-title">Xem trước đơn hàng</h2><button class="btn-close" type="button" data-bs-dismiss="modal" aria-label="Đóng"></button></div><div class="modal-body receipt-content"><div class="receipt-brand">Cafe<span>POS</span></div><p class="receipt-watermark">BẢN MẪU · CHƯA THANH TOÁN</p><p id="receipt-meta"></p><div id="receipt-lines"></div><dl id="receipt-summary" class="order-summary"></dl><p id="receipt-note"></p><p class="receipt-disclaimer">Bản xem trước giao diện, không phải hóa đơn bán hàng đã ghi nhận. Chưa kết nối ngân hàng hoặc ví điện tử.</p></div><div class="modal-footer"><button class="btn btn-outline-secondary" type="button" data-bs-dismiss="modal">Quay lại đơn</button><button class="btn btn-primary" id="print-preview" type="button"><svg class="icon"><use href="#i-print" /></svg>In bản mẫu</button></div></div></div></div>
@section Scripts { <script src="~/js/pos.js" asp-append-version="true" defer></script> }

```

### src/CafePOSManagement.Web/wwwroot/css/site.css

```css
:root {
    --color-background: #f8f9fa; --color-surface: #fff;
    --color-primary: #ff5722; --color-primary-hover: #e64a19;
    --color-primary-soft: #fff2ec; --color-secondary: #2b4c7e;
    --color-text: #24272d; --color-muted: #7a808a; --color-border: #e9ebef;
    --space-1: 4px; --space-2: 8px; --space-3: 12px; --space-4: 16px; --space-5: 24px; --space-6: 32px;
    --radius-sm: 8px; --radius-md: 12px; --radius-lg: 16px;
    --bs-body-font-family: "Segoe UI", Arial, sans-serif; --bs-body-bg: #f8f9fa;
    --bs-body-color: #24272d; --bs-primary: #ff5722; --bs-primary-rgb: 255, 87, 34;
}
* { box-sizing: border-box; }
body { margin: 0; min-width: 320px; font-family: var(--bs-body-font-family); font-size: 14px; }
button, input, select { font: inherit; }
button, a, input, select { -webkit-tap-highlight-color: transparent; }
button { touch-action: manipulation; }
button:disabled { cursor: not-allowed; }
[hidden] { display: none !important; }
:focus-visible { outline: 3px solid var(--color-secondary); outline-offset: 3px; }
.icon-sheet { position: absolute; width: 0; height: 0; overflow: hidden; }
.icon { width: 20px; height: 20px; flex-shrink: 0; fill: none; stroke: currentColor; stroke-width: 1.7; stroke-linecap: round; stroke-linejoin: round; }
.skip-link { position: absolute; top: -100px; left: 12px; z-index: 1100; padding: 12px; background: white; }
.skip-link:focus { top: 8px; }
.pos-page { height: 100vh; height: 100dvh; display: flex; flex-direction: column; overflow: hidden; }
.app-header { display: flex; flex-shrink: 0; align-items: center; gap: 20px; padding: 18px 24px; min-height: 84px; background: white; border-bottom: 1px solid var(--color-border); }
.brand { display: inline-flex; align-items: center; gap: 10px; text-decoration: none; color: var(--color-text); font-size: 25px; font-weight: 750; letter-spacing: -.8px; line-height: 1; white-space: nowrap; }
.brand:hover { color: var(--color-text); }
.brand-accent { color: var(--color-primary); }
.brand small { display: block; font-size: 8px; letter-spacing: 1.7px; font-weight: 600; color: var(--color-muted); margin-top: 7px; }
.header-search { display: flex; align-items: center; gap: 10px; flex: 1; min-width: 140px; max-width: 440px; background: var(--color-background); border: 1px solid var(--color-border); border-radius: 9px; padding: 11px 13px; color: #959aa2; margin-left: 10px; }
.header-search:focus-within { border-color: var(--color-primary); box-shadow: 0 0 0 2px #ff57221a; }
.header-search input { width: 100%; min-width: 0; background: transparent; border: 0; outline: 0; color: var(--color-text); font-size: 12px; }
.header-search input::placeholder { color: #8b9098; }
.header-actions, .utility-actions { display: flex; align-items: center; gap: 10px; }
.header-actions { margin-left: auto; }
.utility-actions { gap: 1px; border-right: 1px solid var(--color-border); padding-right: 8px; }
.clear-link { background: none; border: none; color: #656c77; text-decoration: underline; text-underline-offset: 3px; font-size: 12px; padding: 8px 4px; white-space: nowrap; }
.clear-link:hover { color: var(--color-primary-hover); }
.clear-link:disabled { opacity: .5; }
.icon-button { border: 0; background: transparent; color: #526276; width: 36px; height: 36px; display: inline-flex; justify-content: center; align-items: center; border-radius: 8px; }
.icon-button:hover { background: #f0f3f7; color: var(--color-secondary); }
.icon-button:active { background: #e4e9f0; }
.btn { display: inline-flex; align-items: center; justify-content: center; gap: 7px; font-size: 12px; font-weight: 600; min-height: 42px; border-radius: 8px; padding: 10px 14px; white-space: nowrap; }
.btn-primary { --bs-btn-color: #fff; --bs-btn-bg: #ff5722; --bs-btn-border-color: #ff5722; --bs-btn-hover-color: #fff; --bs-btn-hover-bg: #e64a19; --bs-btn-hover-border-color: #e64a19; --bs-btn-active-bg: #d84315; --bs-btn-active-border-color: #d84315; --bs-btn-disabled-bg: #ff5722; --bs-btn-disabled-border-color: #ff5722; --bs-btn-disabled-color: #fff; }
.btn-outline-secondary { --bs-btn-color: #2b4c7e; --bs-btn-border-color: #e3e6eb; --bs-btn-hover-bg: #f0f3f7; --bs-btn-hover-color: #2b4c7e; --bs-btn-hover-border-color: #c4cedc; --bs-btn-active-bg: #e6ecf3; --bs-btn-active-color: #2b4c7e; }
.add-product > span { font-size: 20px; line-height: 1; font-weight: 400; }
.notification-menu { padding: 20px; width: min(300px, calc(100vw - 32px)); box-shadow: 0 8px 30px #24334a14; border-color: var(--color-border); }
.notification-menu p { margin: 12px 0; }
.notification-menu small { color: var(--color-muted); }
.app-main { padding: 0 20px 20px; min-height: 0; flex: 1; display: flex; flex-direction: column; }
.workspace-bar { display: flex; justify-content: space-between; align-items: center; gap: 12px; padding: 17px 2px; flex-shrink: 0; }
.workspace-bar > div { display: flex; align-items: center; gap: 10px; }
h1 { margin: 0; font-size: 14px; font-weight: 650; }
.live-dot { width: 7px; height: 7px; background: #459b77; border-radius: 50%; }
.workspace-divider { color: #d5d8dd; margin: 0 4px; }
.workspace-caption { color: var(--color-muted); font-size: 12px; }
.demo-badge { color: #94633f; background: #fff0e3; border-radius: 5px; padding: 5px 9px; font-size: 10px; white-space: nowrap; }
.pos-workspace { display: grid; grid-template-columns: minmax(170px, 1.7fr) minmax(0, 6.5fr) minmax(310px, 3.3fr); gap: 18px; min-height: 0; flex: 1; }
.surface { background: white; border: 1px solid var(--color-border); border-radius: 12px; min-width: 0; }
h2 { font-size: 18px; font-weight: 700; margin: 0; letter-spacing: -.3px; }
.eyebrow { display: block; font-size: 9px; letter-spacing: 1.6px; color: var(--color-muted); font-weight: 650; margin-bottom: 8px; }
.categories-panel { display: flex; flex-direction: column; overflow: auto; }
.sidebar-heading { padding: 24px 18px 20px; }
.sidebar-heading h2 { font-size: 16px; }
.category-list { display: flex; flex-direction: column; gap: 8px; padding: 0 10px; }
.category-button { border: 0; background: transparent; border-radius: 8px; width: 100%; display: flex; align-items: center; gap: 10px; padding: 14px 12px; color: #626974; text-align: left; font-size: 12px; font-weight: 500; transition: background-color .15s, color .15s; }
.category-button .category-label { flex: 1; }
.category-button .category-count { color: #9c9fa6; font-size: 10px; }
.category-button .icon { width: 17px; height: 17px; }
.category-button:hover { background: var(--color-primary-soft); color: #d84315; }
.category-button.active { background: var(--color-primary); color: #fff; font-weight: 600; }
.category-button.active .category-count { color: white; }
.category-button:active { transform: translateY(1px); }
.sidebar-bottom { margin-top: auto; padding: 30px 18px 22px; color: #9a7b69; }
.sidebar-bottom .icon { margin-bottom: 12px; width: 25px; height: 25px; }
.sidebar-bottom strong { font-size: 11px; font-weight: 600; }
.sidebar-bottom p { font-size: 10px; color: var(--color-muted); margin: 5px 0 0; }
.menu-panel { min-height: 0; min-width: 0; display: flex; flex-direction: column; }
.order-controls { padding: 18px; display: grid; grid-template-columns: minmax(0, 1.6fr) minmax(140px, 1fr); gap: 15px 18px; }
.field-label { display: block; font-size: 11px; font-weight: 600; margin-bottom: 9px; color: #667080; }
.segmented { display: flex; border: 1px solid var(--color-border); border-radius: 8px; padding: 3px; gap: 3px; min-height: 42px; }
.segmented button { display: flex; justify-content: center; align-items: center; flex: 1; gap: 6px; border: 1px solid transparent; background: transparent; color: #7b8089; border-radius: 5px; padding: 7px 5px; font-size: 11px; white-space: nowrap; }
.segmented button .icon { width: 16px; height: 16px; }
.segmented button:hover { background: var(--color-primary-soft); }
.segmented button.active { color: #df4c20; border-color: var(--color-primary); background: #fff; font-weight: 600; }
.note-field { grid-column: 1 / -1; }
.form-control, .form-select { min-height: 42px; border-color: var(--color-border); border-radius: 7px; font-size: 12px; }
.form-control::placeholder { color: #9298a1; }
.form-control:focus, .form-select:focus { border-color: var(--color-primary); box-shadow: 0 0 0 3px #ff57221a; }
.menu-heading { padding: 22px 0 16px; display: flex; align-items: center; justify-content: space-between; gap: 12px; }
.menu-heading > div { display: flex; align-items: baseline; gap: 10px; }
.menu-heading h2 { font-size: 18px; }
#product-count { color: var(--color-muted); font-size: 11px; }
.menu-tip { display: flex; gap: 6px; align-items: center; color: var(--color-muted); font-size: 10px; }
.menu-tip .icon { width: 14px; height: 14px; }
.product-scroll { min-height: 0; flex: 1; overflow-y: auto; overscroll-behavior: contain; padding: 2px 5px 8px 2px; scrollbar-width: thin; scrollbar-color: #dddfe4 transparent; }
.product-grid { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px; align-content: start; }
.product-card { min-width: 0; text-align: left; display: flex; flex-direction: column; position: relative; background: white; padding: 7px; border: 2px solid transparent; border-radius: 11px; transition: border-color .15s, box-shadow .15s; box-shadow: 0 0 0 1px var(--color-border); }
.product-card:hover { border-color: #ffc3af; box-shadow: 0 3px 12px #2a1e1510; }
.product-card:active { transform: scale(.98); }
.product-card.selected { border-color: var(--color-primary); box-shadow: none; }
.product-image { position: relative; aspect-ratio: 1; width: 100%; overflow: hidden; border-radius: 7px; background: #f2ece8; }
.product-image img { width: 100%; height: 100%; object-fit: cover; transition: transform .25s; }
.product-card:hover img { transform: scale(1.035); }
.selected-mark { display: none; position: absolute; right: 7px; top: 7px; border: 2px solid white; background: var(--color-primary); color: white; border-radius: 50%; width: 23px; height: 23px; align-items: center; justify-content: center; }
.selected .selected-mark { display: flex; }
.selected-mark .icon { width: 13px; height: 13px; stroke-width: 2.5; }
.product-info { display: flex; flex-direction: column; flex: 1; padding: 11px 3px 6px; position: relative; }
.product-name { font-size: 12px; line-height: 1.5; font-weight: 650; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; min-height: 36px; margin-bottom: 8px; }
.product-prices { display: flex; flex-direction: column; margin-top: auto; gap: 2px; padding-right: 23px; }
.product-prices del { font-size: 10px; color: #969ca5; min-height: 15px; }
.product-prices strong { font-size: 13px; font-weight: 750; }
.product-add { position: absolute; right: 1px; bottom: 5px; width: 23px; height: 23px; background: #fff1eb; color: var(--color-primary); border-radius: 7px; display: grid; place-items: center; font-size: 18px; }
.selected .product-add { color: white; background: var(--color-primary); }
.menu-footnote { display: flex; justify-content: space-between; padding-top: 10px; color: #969ba4; font-size: 9px; gap: 10px; }
.cart-panel { display: flex; flex-direction: column; min-height: 0; overflow: hidden; }
.cart-heading { display: flex; justify-content: space-between; align-items: flex-start; gap: 10px; padding: 22px 18px 18px; border-bottom: 1px solid var(--color-border); }
.cart-heading h2 { display: flex; gap: 8px; align-items: center; font-size: 19px; }
#cart-count { min-width: 21px; height: 21px; display: grid; place-items: center; padding: 0 4px; font-size: 10px; border-radius: 6px; color: var(--color-primary); background: var(--color-primary-soft); }
.cart-heading p { font-size: 10px; color: var(--color-muted); margin: 7px 0 0; }
.order-meta { text-align: right; }
.order-meta strong { color: var(--color-secondary); font-size: 12px; }
.order-meta time { display: block; margin-top: 7px; color: #8c939c; font-size: 9px; }
.cart-scroll { flex: 1; min-height: 0; overflow-y: auto; overscroll-behavior: contain; padding: 16px 14px 4px; scrollbar-width: thin; scrollbar-color: #dddfe4 transparent; }
.cart-item { padding: 12px 10px; border-radius: 9px; background: var(--color-background); display: flex; gap: 10px; margin-bottom: 10px; }
.cart-thumbnail { width: 44px; height: 44px; object-fit: cover; border-radius: 7px; }
.cart-item-detail { flex: 1; min-width: 0; }
.cart-item h3 { margin: 0 0 5px; font-size: 12px; font-weight: 650; line-height: 1.5; overflow-wrap: anywhere; }
.cart-unit-price { color: #8b919a; font-size: 11px; }
.quantity-control { display: flex; align-items: center; gap: 10px; margin-top: 10px; }
.quantity-control button { border: none; background: var(--color-primary); color: white; border-radius: 50%; height: 26px; width: 26px; font-size: 18px; line-height: 1; }
.quantity-control button:hover { background: var(--color-primary-hover); }
.quantity-control button:disabled { opacity: .35; }
.quantity { font-size: 12px; min-width: 14px; text-align: center; font-variant-numeric: tabular-nums; }
.cart-item-end { display: flex; flex-direction: column; align-items: flex-end; justify-content: space-between; }
.remove-item { width: 28px; height: 28px; }
.remove-item .icon { width: 16px; height: 16px; }
.line-total { font-size: 11px; white-space: nowrap; margin-bottom: 4px; }
.cart-bottom { flex-shrink: 0; padding: 18px; border-top: 1px solid var(--color-border); }
.order-summary { margin: 0; }
.order-summary > div { display: flex; justify-content: space-between; align-items: baseline; gap: 16px; font-size: 12px; margin-bottom: 12px; }
.order-summary dt { color: #7b8390; font-weight: 400; }
.order-summary dd { margin: 0; font-weight: 600; text-align: right; }
#discount-amount { color: #31805c; }
.order-summary .total-row { border-top: 1px dashed #dce0e6; padding-top: 18px; margin-top: 18px; margin-bottom: 22px; }
.total-row dt { color: var(--color-text); font-size: 17px; font-weight: 700; }
.total-row dd { font-size: 23px; font-weight: 750; letter-spacing: -.6px; }
.payment-field legend { float: none; width: auto; font-size: 11px; margin-bottom: 10px; color: #7a8290; }
.payment-options { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 8px; }
.payment-option { position: relative; cursor: pointer; }
.payment-option input { position: absolute; opacity: 0; width: 1px; height: 1px; }
.payment-option > span { border: 1px solid var(--color-border); border-radius: 8px; display: flex; flex-direction: column; align-items: center; gap: 9px; padding: 13px 2px; font-size: 10px; color: #627088; transition: background-color .15s, color .15s; white-space: nowrap; }
.payment-option input:checked + span { background: var(--color-secondary); border-color: var(--color-secondary); color: white; }
.payment-option input:focus-visible + span { outline: 3px solid var(--color-primary); outline-offset: 2px; }
.payment-option:hover span { border-color: var(--color-secondary); }
.checkout-button { width: 100%; min-height: 48px; margin-top: 20px; font-size: 14px; background: linear-gradient(110deg, #ff642e, #f4511e); box-shadow: 0 4px 10px #ff57221f; }
.checkout-button .icon { margin-left: auto; width: 16px; }
.checkout-button:hover { background: #e64a19; }
.preview-note { color: #959ba4; font-size: 9px; text-align: center; margin: 10px 0 0; }
.empty-state { padding: 44px 18px; text-align: center; color: #8c939c; }
.empty-state > .icon { width: 36px; height: 36px; margin-bottom: 20px; color: #c4a18e; }
.empty-state h3 { color: var(--color-text); font-size: 16px; }
.empty-state p { font-size: 12px; line-height: 1.6; }
.modal-content { border: 0; border-radius: 15px; background: white; }
.file-picker { display: flex; align-items: center; gap: 12px; border: 1px solid var(--color-border); padding: 8px; border-radius: 8px; margin-bottom: 8px; }
.file-picker input:focus-visible + label { outline: 3px solid var(--color-secondary); outline-offset: 2px; }
.file-picker span { min-width: 0; overflow-wrap: anywhere; font-size: 12px; color: #7a808a; }
.modal-header, .modal-footer { padding: 20px 24px; border-color: var(--color-border); }
.modal-body { padding: 24px; }
.form-preview-note { padding: 12px; background: var(--color-primary-soft); color: #925b43; font-size: 12px; border-radius: 8px; line-height: 1.7; }
.form-label { font-size: 12px; font-weight: 600; }
.calculator-result { display: block; margin-top: 18px; color: var(--color-secondary); overflow-wrap: anywhere; }
.receipt-content { font-size: 12px; }
.receipt-brand { font-size: 28px; font-weight: 750; text-align: center; }
.receipt-brand span { color: var(--color-primary); }
.receipt-watermark { color: #b04b2d; text-align: center; font-size: 10px; font-weight: 700; letter-spacing: 1px; margin: 12px 0 20px; }
.receipt-line { display: flex; justify-content: space-between; gap: 20px; border-bottom: 1px dashed var(--color-border); padding: 10px 0; }
#receipt-summary { margin-top: 24px; }
#receipt-note { white-space: pre-wrap; overflow-wrap: anywhere; }
.receipt-disclaimer { font-size: 10px; color: #7a808a; text-align: center; }
.pos-toast { position: fixed; bottom: 24px; left: 50%; transform: translateX(-50%); background: var(--color-secondary); color: white; border-radius: 9px; max-width: calc(100vw - 32px); padding: 0; box-shadow: 0 6px 30px #17365720; z-index: 1090; font-size: 12px; pointer-events: none; }
.pos-toast:not(:empty) { padding: 12px 20px; }
.mobile-cart-link { display: none; }
.error-panel { margin: 40px auto; padding: 32px; width: min(600px, 100%); }
.error-panel h1 { font-size: 24px; margin-bottom: 20px; }
@media (min-width: 1650px) { .order-controls { grid-template-columns: 1.3fr .8fr 1.2fr; } .note-field { grid-column: auto; } .product-name { font-size: 14px; } .product-prices strong { font-size: 15px; } }
@media (max-width: 1350px) { .app-header { flex-wrap: wrap; gap: 12px 20px; padding: 14px 20px; } .header-search { max-width: none; } .header-actions { width: 100%; justify-content: flex-end; } .pos-workspace { grid-template-columns: 165px minmax(0, 1fr) 300px; gap: 12px; } .product-grid { grid-template-columns: repeat(3, minmax(0, 1fr)); } .order-controls { grid-template-columns: minmax(0, 1fr); padding: 12px; gap: 10px; } .note-field { grid-column: auto; } .menu-tip { display: none; } .cart-bottom { padding: 14px; } .category-button { padding: 12px 8px; } }
@media (max-width: 1050px) { .pos-workspace { grid-template-columns: minmax(0, 1fr) 310px; } .categories-panel { grid-column: 1 / -1; overflow: visible; } .sidebar-heading, .sidebar-bottom { display: none; } .category-list { flex-direction: row; overflow-x: auto; padding: 8px; gap: 6px; scrollbar-width: thin; } .category-button { flex: 0 0 auto; width: auto; white-space: nowrap; padding: 10px 14px; } .category-button .icon, .category-count { display: none; } .pos-workspace { grid-template-rows: auto minmax(0, 1fr); } .product-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } .workspace-caption, .workspace-divider { display: none; } .menu-footnote span { display: none; } }
@media (max-width: 767.98px) { .pos-page { height: auto; min-height: 100dvh; overflow: visible; } .app-header { padding: 14px; gap: 14px; } .brand { font-size: 24px; } .header-search { order: 3; flex-basis: 100%; margin: 0; } .header-actions { flex-wrap: wrap; justify-content: flex-start; gap: 8px; } .clear-link { margin-right: auto; } .utility-actions { order: 2; border: 0; } .back-button { margin-left: auto; } .app-main { padding: 0 12px 90px; } .workspace-bar { flex-wrap: wrap; padding: 16px 2px; gap: 8px; } .workspace-bar h1 { font-size: 13px; } .demo-badge { font-size: 9px; } .pos-workspace { display: flex; flex-direction: column; gap: 16px; } .order-controls { grid-template-columns: minmax(0, 1fr) minmax(0, .8fr); padding: 16px; } .order-type-field, .note-field { grid-column: 1 / -1; } .discount-field { grid-column: 1 / -1; } .segmented button { padding: 10px 6px; font-size: 12px; } .product-scroll { overflow: visible; padding: 2px; } .product-grid { gap: 12px; } .product-name { font-size: 13px; min-height: 39px; } .product-prices strong { font-size: 15px; } .product-info { padding: 12px 3px 7px; } .product-prices del { font-size: 11px; } .cart-panel { scroll-margin-top: 12px; } .cart-scroll { max-height: 380px; min-height: 140px; } .cart-heading { padding: 20px; } .cart-bottom { padding: 20px; } .mobile-cart-link { display: flex; position: fixed; z-index: 1020; bottom: 12px; left: 12px; right: 12px; background: var(--color-secondary); color: white; text-decoration: none; padding: 15px 18px; border-radius: 12px; align-items: center; gap: 10px; box-shadow: 0 4px 18px #17365730; } .mobile-cart-link strong { margin-left: auto; } .pos-toast { bottom: 80px; } }
@media (min-width: 768px) and (max-height: 850px) {
    .cart-heading { padding: 14px; flex-shrink: 0; }
    .cart-bottom { padding: 12px 14px; }
    .order-summary > div { margin-bottom: 8px; }
    .order-summary .total-row { padding-top: 12px; margin: 12px 0 14px; }
    .payment-option > span { padding: 9px 2px; gap: 6px; }
    .checkout-button { margin-top: 12px; min-height: 44px; }
    .preview-note { margin-top: 6px; }
}
@media (min-width: 768px) and (max-height: 650px) {
    .pos-page { height: auto; overflow: auto; }
    .pos-workspace { min-height: 650px; }
    .product-scroll { max-height: 500px; }
    .cart-scroll { max-height: 300px; }
}
@media (prefers-reduced-motion: reduce) { *, *::before, *::after { scroll-behavior: auto !important; transition: none !important; animation: none !important; } }
@media print { body { height: auto !important; overflow: visible !important; background: white; } body > :not(#main-content), #main-content > :not(#receipt-modal), .modal-backdrop { display: none !important; } #receipt-modal { display: block !important; position: static; opacity: 1 !important; overflow: visible; } #receipt-modal .modal-dialog { transform: none; width: 80mm; max-width: 100%; margin: 0 auto; height: auto; } #receipt-modal .modal-content, #receipt-modal .modal-body { max-height: none; overflow: visible; } #receipt-modal .modal-header, #receipt-modal .modal-footer { display: none; } .app-main { padding: 0; display: block; } .receipt-watermark { color: black; } }

```

### src/CafePOSManagement.Web/wwwroot/js/site.js

```javascript
"use strict";
{
    const clocks = document.querySelectorAll("[data-local-clock]");
    if (clocks.length) {
        const formatter = new Intl.DateTimeFormat("vi-VN", {
            timeZone: "Asia/Ho_Chi_Minh", day: "2-digit", month: "2-digit",
            year: "numeric", hour: "2-digit", minute: "2-digit", hour12: false
        });
        const updateClock = () => {
            const now = new Date();
            clocks.forEach(clock => { clock.dateTime = now.toISOString(); clock.textContent = formatter.format(now); });
        };
        updateClock();
        window.setInterval(updateClock, 30_000);
    }
}

```

### src/CafePOSManagement.Web/wwwroot/js/pos.js

```javascript
"use strict";
(() => {
    const app = document.querySelector("#pos-app");
    if (!app) return;

    // Dữ liệu minh họa Giai đoạn 2; không thay thế dữ liệu từ SQL Server.
    const categories = [
        { id: "all", name: "Tất cả món" },
        { id: "traditional", name: "Cà phê truyền thống" },
        { id: "snack", name: "Đồ ăn nhẹ" },
        { id: "sandwich", name: "Bánh mì & Sandwich" },
        { id: "coldbrew", name: "Cold Brew đặc biệt" },
        { id: "espresso", name: "Cà phê Ý (Espresso)" },
        { id: "seasonal", name: "Đồ uống theo mùa" },
        { id: "tea", name: "Trà trái cây" }
    ];
    const products = [
        { id: "p1", name: "Cà phê sữa đá", category: "traditional", price: 45000, originalPrice: 55000, image: "latte", alt: "Ly cà phê đá hòa cùng sữa" },
        { id: "p2", name: "Cappuccino", category: "espresso", price: 55000, originalPrice: 65000, image: "coffee", alt: "Tách cà phê với lớp bọt sữa tạo hình" },
        { id: "p3", name: "Bánh Croissant bơ", category: "snack", price: 45000, originalPrice: 50000, image: "croissant", alt: "Bánh sừng bò phủ lớp đường mịn" },
        { id: "p4", name: "Americano nóng", category: "espresso", price: 40000, originalPrice: 45000, image: "espresso", alt: "Tách cà phê đen nóng nhìn từ trên xuống" },
        { id: "p5", name: "Bạc xỉu đá", category: "traditional", price: 39000, originalPrice: null, image: "iced", alt: "Cà phê sữa trong ly thủy tinh có đá" },
        { id: "p6", name: "Sandwich nướng", category: "sandwich", price: 65000, originalPrice: 75000, image: "sandwich", alt: "Bánh sandwich nướng vàng cắt tam giác" },
        { id: "p7", name: "Cold Brew sữa tươi", category: "coldbrew", price: 55000, originalPrice: 60000, image: "latte", alt: "Cà phê lạnh và sữa trong ly cao" },
        { id: "p8", name: "Trà chanh thanh mát", category: "tea", price: 45000, originalPrice: null, image: "fruit-tea", alt: "Ly trà đá cùng lát chanh tươi" },
        { id: "p9", name: "Latte kem sữa", category: "espresso", price: 59000, originalPrice: 65000, image: "coffee", alt: "Cà phê latte với họa tiết trên lớp bọt sữa" },
        { id: "p10", name: "Cà phê sữa mùa đông", category: "seasonal", price: 49000, originalPrice: null, image: "tea", alt: "Tách đồ uống nóng bên cạnh bánh quy" },
        { id: "p11", name: "Cold Brew đá", category: "coldbrew", price: 49000, originalPrice: 55000, image: "iced", alt: "Ly cà phê lạnh có đá trên bàn gỗ" },
        { id: "p12", name: "Bánh quy bơ", category: "snack", price: 29000, originalPrice: 35000, image: "tea", alt: "Bánh quy bơ dùng kèm một tách đồ uống" }
    ];
    const productById = new Map(products.map(product => [product.id, product]));
    const state = {
        category: "all", search: "", discount: 0, orderType: "pickup", payment: "cash", note: "",
        // Ba món mẫu để quan sát đầy đủ bố cục; tải lại trang sẽ khởi tạo lại bản mẫu.
        cart: new Map([["p1", 1], ["p2", 1], ["p3", 1]])
    };
    const money = new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND", maximumFractionDigits: 0 });
    const normalize = value => value.trim().toLocaleLowerCase("vi-VN").normalize("NFD").replace(/[\u0300-\u036f]/g, "").replace(/đ/g, "d");
    const $ = selector => document.querySelector(selector);
    const grid = $("#product-grid");
    const cartItems = $("#cart-items");
    const status = $("#pos-status");
    let notificationTimer;

    function notify(message) {
        window.clearTimeout(notificationTimer);
        status.textContent = message;
        notificationTimer = window.setTimeout(() => { status.textContent = ""; }, 3500);
    }

    function totals() {
        let subtotal = 0;
        let count = 0;
        state.cart.forEach((quantity, id) => { subtotal += productById.get(id).price * quantity; count += quantity; });
        const discount = Math.round(subtotal * state.discount / 100);
        return { subtotal, discount, total: subtotal - discount, count };
    }

    function setImage(image, product) {
        image.alt = image.classList.contains("cart-thumbnail") ? "" : product.alt;
        image.src = `${app.dataset.imageRoot}${product.image}.jpg`;
    }

    // error không bubble; capture xử lý cả ảnh mới render, không lặp khi ảnh dự phòng lỗi.
    app.addEventListener("error", event => {
        const image = event.target;
        if (!(image instanceof HTMLImageElement) || image.dataset.fallback) return;
        image.dataset.fallback = "true";
        image.src = app.dataset.fallbackImage;
    }, true);

    function renderCategories() {
        const fragment = document.createDocumentFragment();
        categories.forEach(category => {
            const button = document.createElement("button");
            button.type = "button";
            button.className = "category-button";
            button.dataset.category = category.id;
            button.setAttribute("aria-pressed", String(category.id === state.category));
            button.classList.toggle("active", category.id === state.category);
            const label = document.createElement("span");
            label.className = "category-label";
            label.textContent = category.name;
            const count = document.createElement("span");
            count.className = "category-count";
            count.textContent = String(category.id === "all" ? products.length : products.filter(product => product.category === category.id).length);
            const arrow = document.createElementNS("http://www.w3.org/2000/svg", "svg");
            arrow.classList.add("icon");
            arrow.setAttribute("aria-hidden", "true");
            const use = document.createElementNS("http://www.w3.org/2000/svg", "use");
            use.setAttribute("href", "#i-arrow");
            arrow.append(use);
            button.append(label, count, arrow);
            fragment.append(button);
        });
        $("#category-list").replaceChildren(fragment);
        categories.filter(category => category.id !== "all").forEach(category => {
            const option = new Option(category.name, category.id);
            $("#new-category").add(option);
        });
    }

    function renderProducts() {
        const query = normalize(state.search);
        const visible = products.filter(product => (state.category === "all" || product.category === state.category) && normalize(product.name).includes(query));
        const fragment = document.createDocumentFragment();
        visible.forEach(product => {
            const card = $("#product-template").content.firstElementChild.cloneNode(true);
            card.dataset.product = product.id;
            card.setAttribute("aria-label", `Thêm ${product.name}, ${money.format(product.price)} vào giỏ`);
            setImage(card.querySelector("img"), product);
            card.querySelector(".product-name").textContent = product.name;
            card.querySelector(".product-name").title = product.name;
            card.querySelector(".product-prices del").textContent = product.originalPrice ? money.format(product.originalPrice) : "";
            card.querySelector(".product-prices strong").textContent = money.format(product.price);
            fragment.append(card);
        });
        grid.replaceChildren(fragment);
        grid.setAttribute("aria-busy", "false");
        $("#product-count").textContent = `${visible.length} món`;
        $("#menu-title").textContent = categories.find(category => category.id === state.category).name;
        $("#no-results").hidden = visible.length > 0;
        updateSelectedProducts();
    }

    function updateSelectedProducts() {
        grid.querySelectorAll("[data-product]").forEach(card => {
            const quantity = state.cart.get(card.dataset.product) || 0;
            card.classList.toggle("selected", quantity > 0);
            card.querySelector(".product-add").textContent = quantity > 0 ? String(quantity) : "+";
        });
    }

    function renderCart() {
        const focused = document.activeElement;
        const focusedProduct = focused?.closest(".cart-item")?.dataset.product;
        const focusedAction = focused?.dataset.action;
        const fragment = document.createDocumentFragment();
        state.cart.forEach((quantity, id) => {
            const product = productById.get(id);
            const row = $("#cart-template").content.firstElementChild.cloneNode(true);
            row.dataset.product = id;
            setImage(row.querySelector("img"), product);
            row.querySelector("h3").textContent = product.name;
            row.querySelector(".cart-unit-price").textContent = money.format(product.price);
            row.querySelector(".quantity").textContent = String(quantity);
            row.querySelector(".line-total").textContent = money.format(product.price * quantity);
            const decrease = row.querySelector('[data-action="decrease"]');
            const increase = row.querySelector('[data-action="increase"]');
            decrease.disabled = quantity === 1;
            increase.disabled = quantity >= 99;
            decrease.setAttribute("aria-label", `Giảm số lượng ${product.name}`);
            increase.setAttribute("aria-label", `Tăng số lượng ${product.name}`);
            row.querySelector('[data-action="remove"]').setAttribute("aria-label", `Xóa ${product.name}`);
            fragment.append(row);
        });
        cartItems.replaceChildren(fragment);
        const amount = totals();
        $("#cart-empty").hidden = amount.count > 0;
        $("#cart-count").textContent = String(amount.count);
        $("#subtotal").textContent = money.format(amount.subtotal);
        $("#total").textContent = money.format(amount.total);
        $("#mobile-total").textContent = money.format(amount.total);
        $("#discount-row").hidden = !state.discount;
        $("#discount-label").textContent = `Giảm giá (${state.discount}%)`;
        $("#discount-amount").textContent = `−${money.format(amount.discount)}`;
        ["#checkout", "#print-receipt", "#clear-cart"].forEach(selector => { $(selector).disabled = !amount.count; });
        updateSelectedProducts();
        if (focusedProduct && focusedAction) {
            const row = cartItems.querySelector(`[data-product="${focusedProduct}"]`);
            const target = row?.querySelector(`[data-action="${focusedAction}"]:not(:disabled)`) || row?.querySelector("button:not(:disabled)") || cartItems.querySelector("button:not(:disabled)");
            (target || $("#product-search")).focus({ preventScroll: true });
        }
    }

    $("#category-list").addEventListener("click", event => {
        const button = event.target.closest("[data-category]");
        if (!button) return;
        state.category = button.dataset.category;
        $("#category-list").querySelectorAll("button").forEach(item => {
            item.classList.toggle("active", item === button);
            item.setAttribute("aria-pressed", String(item === button));
        });
        renderProducts();
        $("#product-scroll").scrollTop = 0;
    });
    $("#product-search").addEventListener("input", event => { state.search = event.target.value; renderProducts(); });
    $("#reset-search").addEventListener("click", () => {
        state.search = "";
        $("#product-search").value = "";
        $("#category-list [data-category='all']").click();
        $("#product-search").focus();
    });
    grid.addEventListener("click", event => {
        const card = event.target.closest("[data-product]");
        if (!card || !productById.has(card.dataset.product)) return;
        const id = card.dataset.product;
        const quantity = state.cart.get(id) || 0;
        if (quantity >= 99) { notify("Mỗi món tối đa 99 phần trong bản xem trước."); return; }
        state.cart.set(id, quantity + 1);
        renderCart();
        notify(`Đã thêm ${productById.get(id).name} vào giỏ mẫu.`);
    });
    cartItems.addEventListener("click", event => {
        const button = event.target.closest("[data-action]");
        const id = button?.closest("[data-product]")?.dataset.product;
        if (!button || !state.cart.has(id)) return;
        const quantity = state.cart.get(id);
        if (button.dataset.action === "remove") state.cart.delete(id);
        else if (button.dataset.action === "increase" && quantity < 99) state.cart.set(id, quantity + 1);
        else if (button.dataset.action === "decrease" && quantity > 1) state.cart.set(id, quantity - 1);
        renderCart();
    });
    $("#order-types").addEventListener("click", event => {
        const button = event.target.closest("[data-order]");
        if (!button) return;
        state.orderType = button.dataset.order;
        $("#order-types").querySelectorAll("button").forEach(item => {
            item.classList.toggle("active", item === button);
            item.setAttribute("aria-pressed", String(item === button));
        });
    });
    $("#discount").addEventListener("change", event => {
        const value = Number(event.target.value);
        state.discount = [0, 10, 20].includes(value) ? value : 0;
        renderCart();
    });
    $("#order-note").addEventListener("input", event => { state.note = event.target.value.slice(0, 200); });
    $("#payment-options").addEventListener("change", event => {
        if (["cash", "card", "wallet"].includes(event.target.value)) state.payment = event.target.value;
    });
    $("#clear-cart").addEventListener("click", () => bootstrap.Modal.getOrCreateInstance($("#clear-modal")).show());
    $("#confirm-clear").addEventListener("click", () => {
        state.cart.clear();
        renderCart();
        bootstrap.Modal.getInstance($("#clear-modal")).hide();
        notify("Đã xóa các món khỏi giỏ mẫu.");
    });
    $("#refresh-pos").addEventListener("click", () => { renderProducts(); renderCart(); notify("Đã làm mới giao diện. Giỏ hàng và cấu hình đơn được giữ nguyên."); });
    $("#go-back").addEventListener("click", () => {
        // Không tự chuyển người dùng tới một website khác từ bản xem trước.
        if (document.referrer && new URL(document.referrer).origin === window.location.origin && window.history.length > 1) window.history.back();
        else notify("Bạn đang ở màn hình đầu tiên. Trang quản trị chưa triển khai.");
    });

    function showReceipt() {
        if (!state.cart.size) return;
        const orderNames = { pickup: "Mang đi", delivery: "Giao hàng", dinein: "Tại bàn" };
        const paymentNames = { cash: "Tiền mặt", card: "Thẻ ngân hàng", wallet: "Ví điện tử" };
        $("#receipt-meta").textContent = `Đơn mẫu #003 · ${orderNames[state.orderType]} · ${paymentNames[state.payment]}`;
        const fragment = document.createDocumentFragment();
        state.cart.forEach((quantity, id) => {
            const product = productById.get(id);
            const row = document.createElement("div");
            row.className = "receipt-line";
            const label = document.createElement("span");
            label.textContent = `${quantity} × ${product.name} (${money.format(product.price)})`;
            const value = document.createElement("strong");
            value.textContent = money.format(product.price * quantity);
            row.append(label, value);
            fragment.append(row);
        });
        $("#receipt-lines").replaceChildren(fragment);
        const amount = totals();
        const summary = document.createDocumentFragment();
        [["Tạm tính", amount.subtotal], [`Giảm giá (${state.discount}%)`, -amount.discount], ["Thuế VAT (0%)", 0], ["Tổng cộng", amount.total]].forEach(([label, value], index) => {
            const row = document.createElement("div");
            if (index === 3) row.className = "total-row";
            const term = document.createElement("dt");
            const description = document.createElement("dd");
            term.textContent = label;
            description.textContent = money.format(value);
            row.append(term, description);
            summary.append(row);
        });
        $("#receipt-summary").replaceChildren(summary);
        $("#receipt-note").textContent = state.note.trim() ? `Ghi chú: ${state.note.trim()}` : "";
        bootstrap.Modal.getOrCreateInstance($("#receipt-modal")).show();
    }
    $("#checkout").addEventListener("click", showReceipt);
    $("#print-receipt").addEventListener("click", showReceipt);
    $("#print-preview").addEventListener("click", () => window.print());
    $("#calculator-form").addEventListener("submit", event => {
        event.preventDefault();
        const first = Number($("#calc-a").value);
        const second = Number($("#calc-b").value);
        const operation = $("#calc-operation").value;
        const output = $("#calc-result");
        if (!Number.isFinite(first) || !Number.isFinite(second) || Math.abs(first) > 1e12 || Math.abs(second) > 1e12) { output.textContent = "Vui lòng nhập số hữu hạn có trị tuyệt đối không quá 1.000.000.000.000."; return; }
        if (operation === "divide" && second === 0) { output.textContent = "Không thể chia cho 0."; return; }
        const result = { add: () => first + second, subtract: () => first - second, multiply: () => first * second, divide: () => first / second }[operation]?.();
        output.textContent = Number.isFinite(result) ? `Kết quả: ${new Intl.NumberFormat("vi-VN", { maximumFractionDigits: 8 }).format(result)}` : "Không thể tính kết quả.";
    });
    $("#new-image").addEventListener("change", event => {
        const file = event.target.files?.[0];
        if (file && !["image/jpeg", "image/png"].includes(file.type)) {
            event.target.value = "";
            $("#image-file-name").textContent = "Vui lòng chọn ảnh JPG hoặc PNG.";
            return;
        }
        $("#image-file-name").textContent = file ? file.name : "Chưa chọn tệp";
    });
    renderCategories();
    renderProducts();
    renderCart();
})();

```

### src/CafePOSManagement.Web/wwwroot/images/product-fallback.svg

```xml
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 600 600"><rect width="600" height="600" fill="#f4eee9"/><g fill="none" stroke="#bc8971" stroke-width="9" stroke-linecap="round"><path d="M205 240h170v100a85 85 0 0 1-170 0V240Zm170 15h24a42 42 0 0 1 0 84h-24M180 445h230M260 140v45m60-45v45"/></g><text x="300" y="510" text-anchor="middle" font-family="Arial,sans-serif" font-size="23" fill="#79665e">CafePOS</text></svg>

```

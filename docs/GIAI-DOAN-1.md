# Giai đoạn 1 — Khởi tạo CafePOS Management

**Dự án trong thư mục này đã hoàn tất khởi tạo. Để chạy, làm theo mục "Chạy trên
máy hiện tại" trong README. Không chạy lại các lệnh `dotnet new` ở mục 2 và không
dùng `--force` để ghi đè mã nguồn hiện có. Dừng phiên chạy cũ bằng Ctrl+C trước khi build.**

## 1. Phạm vi và các tệp

Tạo project MVC net8.0, DI, Service Layer, EF Core SQL Server, văn hóa Việt Nam,
layout nền tảng, CSS Variables và trang POS trống. Không triển khai nghiệp vụ bán hàng.

```text
CafePOSManagement.sln
.gitignore
README.md
docs/
  GIAI-DOAN-1.md
  KIEM-THU-GIAI-DOAN-1.md
src/CafePOSManagement.Web/
  CafePOSManagement.Web.csproj
  Program.cs
  appsettings.json
  appsettings.Development.json
  Properties/launchSettings.json
  Controllers/PosController.cs
  Models/Entities/README.md
  Models/ViewModels/PosIndexViewModel.cs
  Data/CafePosDbContext.cs
  Services/Interfaces/IPosService.cs
  Services/Implementations/PosService.cs
  Views/_ViewImports.cshtml
  Views/_ViewStart.cshtml
  Views/Shared/_Layout.cshtml
  Views/Shared/Error.cshtml
  Views/Pos/Index.cshtml
  wwwroot/css/site.css
  wwwroot/js/site.js
  wwwroot/images/cafepos-mark.svg
  wwwroot/lib/bootstrap/LICENSE
  wwwroot/lib/bootstrap/dist/css/bootstrap.min.css
  wwwroot/lib/bootstrap/dist/css/bootstrap.min.css.map
  wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js
  wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js.map
```

`.tools/` chứa SDK và cache riêng; `artifacts/` chứa kết quả kiểm tra. Cả hai bị
loại khỏi Git, không phải mã nguồn ứng dụng.

## 2. Các lệnh CLI tương đương khi khởi tạo từ thư mục trống

Các lệnh dưới đây là hướng dẫn tái tạo nền tảng trên máy có .NET 8 SDK;
không chạy `dotnet new` chồng lên project đã được tạo.
Nếu lệnh `dotnet` trỏ tới SDK 10, template MVC có thể không nhận `net8.0`.
Trong dự án hiện tại đã có SDK 8 riêng ở `.tools/dotnet/dotnet.exe`; không cần tạo lại.

```powershell
dotnet new sln --name CafePOSManagement
dotnet new mvc --name CafePOSManagement.Web --framework net8.0 --output src/CafePOSManagement.Web --no-restore
dotnet sln CafePOSManagement.sln add src/CafePOSManagement.Web/CafePOSManagement.Web.csproj
dotnet add src/CafePOSManagement.Web/CafePOSManagement.Web.csproj package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.33
dotnet add src/CafePOSManagement.Web/CafePOSManagement.Web.csproj package Microsoft.EntityFrameworkCore.Tools --version 8.0.33
dotnet user-secrets init --project src/CafePOSManagement.Web
```

Sau scaffold, áp dụng các tệp nguồn trong mục 4. Các trang Home/Privacy, CSS isolation
và thư viện jQuery do template tạo không thuộc bản giao này; bản triển khai chỉ có
các tệp trong danh sách mục 1. Bootstrap 5.3.8 được tải về `wwwroot/lib/bootstrap`
theo đường dẫn chính thức trong [tài liệu Bootstrap](https://getbootstrap.com/docs/5.3/getting-started/introduction/).
Hai tệp CSS/JS đã được đối chiếu SHA384 với tài liệu chính thức. Giữ nguyên LICENSE.

Trong lần triển khai này, SDK .NET 8 cục bộ đã tạo solution và scaffold mẫu ở `.tools`;
mã nguồn ứng dụng được viết trực tiếp, PackageReference được khai báo trong csproj rồi
`dotnet restore` tải gói. Không cần chạy lại `dotnet add package` trên bản giao.

`Microsoft.EntityFrameworkCore.Tools` hỗ trợ Package Manager Console trong Visual Studio,
không tự cài lệnh `dotnet ef`. Nếu giai đoạn database sau này dùng EF CLI, sẽ cài
local tool `dotnet-ef` cùng phiên bản và thêm `Microsoft.EntityFrameworkCore.Design`
trực tiếp khi cần. Giai đoạn 1 chưa thực hiện những bước đó.

## 3. Cấu hình và kiểm thử

Xem README để đặt User Secrets hoặc biến môi trường `ConnectionStrings__CafePos`.
`appsettings.json` chỉ chứa giá trị rỗng, không có tên tài khoản hoặc mật khẩu thật.
Đăng ký DbContext không đồng nghĩa đã kết nối SQL Server thành công.

```powershell
dotnet build CafePOSManagement.sln
dotnet run --project src/CafePOSManagement.Web --no-build --launch-profile http
```

Trên máy hiện tại dùng `.\.tools\dotnet\dotnet.exe` theo README. Mở
`http://localhost:5088` hoặc `http://localhost:5088/Pos/Index`.

Tiêu chí: trang tiếng Việt, định dạng tiền `0 ₫`, thời gian Việt Nam, logo CafePOS,
màu nền trắng/xám, màu nhấn cam/navy, ba vùng ở desktop và xếp lại trên mobile;
Làm mới và liên kết logo hoạt động; chưa có dữ liệu giả, thanh toán bị vô hiệu hóa;
không lỗi console, không tài nguyên 404, không cuộn ngang ở 320px trở lên.

Giảm giá, VAT, giao hàng, thẻ ngân hàng và ví điện tử là yêu cầu bổ sung trong prompt
mới, vượt phần đặc tả chi tiết Lab 01. Sẽ chốt quy tắc trước khi lập trình chức năng;
không tự chọn mức VAT, quy tắc chiết khấu hay nhà cung cấp thanh toán ở giai đoạn này.

## 4. Mã nguồn chi tiết

Các phần bên dưới là nội dung tệp thực tế của bản giao giai đoạn 1. Thư viện Bootstrap
không chép lại vào tài liệu; mã thư viện và giấy phép nằm ở các đường dẫn đã liệt kê.
README và checklist kiểm thử được cung cấp riêng để tránh lặp nội dung.


### .gitignore

```text
**/bin/
**/obj/
.vs/
.idea/
.tools/
.playwright-cli/
artifacts/
*.user
*.suo
.env
.env.*
!.env.example
**/secrets.json
**/appsettings.Local.json
*.pfx
*.p12


```

### CafePOSManagement.sln

```text

Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "src", "src", "{15081AD4-ECCE-4C1D-B331-5AED3022829A}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "CafePOSManagement.Web", "src\CafePOSManagement.Web\CafePOSManagement.Web.csproj", "{3FD3F36F-5D62-4D7A-9B82-393CD822566F}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{3FD3F36F-5D62-4D7A-9B82-393CD822566F}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{3FD3F36F-5D62-4D7A-9B82-393CD822566F}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{3FD3F36F-5D62-4D7A-9B82-393CD822566F}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{3FD3F36F-5D62-4D7A-9B82-393CD822566F}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{3FD3F36F-5D62-4D7A-9B82-393CD822566F} = {15081AD4-ECCE-4C1D-B331-5AED3022829A}
	EndGlobalSection
EndGlobal

```

### src/CafePOSManagement.Web/appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  }
}

```

### src/CafePOSManagement.Web/appsettings.json

```json
{
  "ConnectionStrings": {
    "CafePos": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "localhost;127.0.0.1"
}

```

### src/CafePOSManagement.Web/CafePOSManagement.Web.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UserSecretsId>CafePOSManagement-Web-Development</UserSecretsId>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.33" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.33">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>

```

### src/CafePOSManagement.Web/Controllers/PosController.cs

```csharp
using CafePOSManagement.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CafePOSManagement.Web.Controllers;

public sealed class PosController(IPosService posService) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(posService.GetOverview());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return View("~/Views/Shared/Error.cshtml");
    }
}

```

### src/CafePOSManagement.Web/Data/CafePosDbContext.cs

```csharp
using Microsoft.EntityFrameworkCore;

namespace CafePOSManagement.Web.Data;

public sealed class CafePosDbContext(DbContextOptions<CafePosDbContext> options)
    : DbContext(options)
{
    // DbSet và ràng buộc được bổ sung khi chốt mô hình dữ liệu nghiệp vụ.
}

```

### src/CafePOSManagement.Web/Models/Entities/README.md

```markdown
# Entities

Thư mục dành cho các thực thể nghiệp vụ theo UC01–UC13 của Lab 01.
Giai đoạn 1 chưa tạo entity hoặc bảng dữ liệu. Mô hình và ràng buộc được chốt
trước khi lập trình chức năng; migration được thực hiện ở giai đoạn database.


```

### src/CafePOSManagement.Web/Models/ViewModels/PosIndexViewModel.cs

```csharp
namespace CafePOSManagement.Web.Models.ViewModels;

public sealed record PosIndexViewModel(DateTimeOffset CurrentTime);

```

### src/CafePOSManagement.Web/Program.cs

```csharp
using System.Globalization;
using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Services.Implementations;
using CafePOSManagement.Web.Services.Interfaces;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var vietnameseCulture = (CultureInfo)CultureInfo.GetCultureInfo("vi-VN").Clone();
vietnameseCulture.NumberFormat.CurrencySymbol = "₫";
vietnameseCulture.NumberFormat.CurrencyDecimalDigits = 0;
CultureInfo.DefaultThreadCurrentCulture = vietnameseCulture;
CultureInfo.DefaultThreadCurrentUICulture = vietnameseCulture;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(vietnameseCulture);
    options.SupportedCultures = [vietnameseCulture];
    options.SupportedUICultures = [vietnameseCulture];
    // Một ngôn ngữ ở giai đoạn này, không đổi theo ngôn ngữ của trình duyệt.
    options.RequestCultureProviders.Clear();
});

builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

builder.Services.AddDbContext<CafePosDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("CafePos");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Chưa cấu hình SQL Server. Đặt ConnectionStrings:CafePos bằng User Secrets " +
            "hoặc biến môi trường ConnectionStrings__CafePos trước khi sử dụng dữ liệu.");
    }

    options.UseSqlServer(connectionString);
});

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IPosService, PosService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Pos/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRequestLocalization();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pos}/{action=Index}/{id?}");

app.Run();

```

### src/CafePOSManagement.Web/Properties/launchSettings.json

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5088",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}

```

### src/CafePOSManagement.Web/Services/Implementations/PosService.cs

```csharp
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Interfaces;

namespace CafePOSManagement.Web.Services.Implementations;

public sealed class PosService(TimeProvider timeProvider) : IPosService
{
    private static readonly TimeZoneInfo VietnamTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");

    public PosIndexViewModel GetOverview()
    {
        var localTime = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), VietnamTimeZone);
        return new PosIndexViewModel(localTime);
    }
}

```

### src/CafePOSManagement.Web/Services/Interfaces/IPosService.cs

```csharp
using CafePOSManagement.Web.Models.ViewModels;

namespace CafePOSManagement.Web.Services.Interfaces;

public interface IPosService
{
    PosIndexViewModel GetOverview();
}

```

### src/CafePOSManagement.Web/Views/_ViewImports.cshtml

```cshtml
@using CafePOSManagement.Web.Models.ViewModels
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers

```

### src/CafePOSManagement.Web/Views/_ViewStart.cshtml

```cshtml
@{
    Layout = "_Layout";
}

```

### src/CafePOSManagement.Web/Views/Pos/Index.cshtml

```cshtml
@model PosIndexViewModel
@{
    ViewData["Title"] = "Bán hàng";
    var zeroAmount = 0m.ToString("C0");
}
<div class="page-heading">
    <div><p class="eyebrow">ĐIỂM BÁN HÀNG</p><h1>Bán hàng tại quầy</h1></div>
    <time datetime="@Model.CurrentTime.ToString("O")" data-local-clock>
        @Model.CurrentTime.ToString("dddd, dd/MM/yyyy · HH:mm")
    </time>
</div>
<div class="stage-notice" role="note">
    <span class="notice-indicator" aria-hidden="true"></span>
    <p><strong>Khung POS đã sẵn sàng.</strong> Chức năng thực đơn, giỏ hàng và thanh toán chưa triển khai.</p>
</div>
<div class="pos-workspace">
    <aside class="surface categories-panel" aria-labelledby="categories-title">
        <div class="panel-heading"><h2 id="categories-title">Danh mục</h2></div>
        <div class="categories-body">
            <div class="category-current"><span>Tất cả món</span><span>0</span></div>
            <p class="muted-hint">Danh mục sẽ xuất hiện khi có dữ liệu thực đơn.</p>
        </div>
        <div class="sidebar-caption">Thực đơn của quán<br /><strong>Gọn gàng. Dễ tìm.</strong></div>
    </aside>
    <section class="surface menu-panel" aria-labelledby="menu-title">
        <div class="panel-heading menu-heading">
            <h2 id="menu-title">Thực đơn</h2><span class="count-label">0 món</span>
        </div>
        <div class="menu-toolbar">
            <div class="search-field">
                <label for="product-search" class="visually-hidden">Tìm kiếm món — chưa triển khai</label>
                <input id="product-search" class="form-control" type="search" placeholder="Tìm kiếm món..." disabled aria-describedby="menu-unavailable" />
            </div>
            <button type="button" class="btn btn-primary" disabled aria-describedby="menu-unavailable">+ Thêm món mới</button>
        </div>
        <div class="menu-empty">
            <div class="empty-emblem" aria-hidden="true">
                <svg width="64" height="64" viewBox="0 0 64 64" fill="none">
                    <path d="M17 24h27v15a13.5 13.5 0 0 1-27 0V24Z" stroke="currentColor" stroke-width="2.5" />
                    <path d="M44 27h5a7 7 0 1 1 0 14h-5M13 55h38M25 8v8M35 6v10" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" />
                </svg>
            </div>
            <h3>Thực đơn đang chờ bạn</h3>
            <p id="menu-unavailable">Chưa triển khai quản lý món và tìm kiếm.<br />Các món của quán sẽ được hiển thị tại đây.</p>
            <span class="empty-label">Chưa có dữ liệu thực đơn</span>
        </div>
    </section>
    <aside class="surface cart-panel" aria-labelledby="cart-title">
        <div class="panel-heading"><h2 id="cart-title">Giỏ hàng</h2><span class="count-label">0 món</span></div>
        <div class="order-reference">Chưa tạo đơn hàng</div>
        <div class="cart-empty"><span class="cart-empty-line" aria-hidden="true"></span><h3>Giỏ hàng trống</h3><p>Món được chọn sẽ xuất hiện tại đây.</p></div>
        <div class="cart-summary">
            <dl><div><dt>Tạm tính</dt><dd>@zeroAmount</dd></div><div><dt>Thuế VAT</dt><dd>Chưa cấu hình</dd></div><div class="total-row"><dt>Tổng cộng</dt><dd data-testid="cart-total">@zeroAmount</dd></div></dl>
            <button type="button" class="btn btn-primary checkout-button" disabled aria-describedby="checkout-unavailable">Thanh toán</button>
            <p id="checkout-unavailable" class="muted-hint text-center">Chưa triển khai thanh toán</p>
        </div>
    </aside>
</div>

```

### src/CafePOSManagement.Web/Views/Shared/_Layout.cshtml

```cshtml
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="theme-color" content="#F4511E" />
    <title>@ViewData["Title"] · CafePOS Management</title>
    <link rel="icon" type="image/svg+xml" href="~/images/cafepos-mark.svg" />
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" asp-append-version="true" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <a class="skip-link" href="#main-content">Đến nội dung chính</a>
    <header class="app-header">
        <a class="brand" asp-controller="Pos" asp-action="Index" aria-label="CafePOS — Trang bán hàng">
            <img src="~/images/cafepos-mark.svg" width="40" height="40" alt="" />
            <span>Cafe<span class="brand-accent">POS</span><small>MANAGEMENT</small></span>
        </a>
        <div class="header-location"><span class="location-mark" aria-hidden="true"></span> Quầy bán hàng</div>
        <span class="phase-badge">Giai đoạn 1 · Khung ứng dụng</span>
        <button type="button" class="btn btn-outline-secondary refresh-button" data-action="reload">Làm mới</button>
    </header>
    <main id="main-content" class="app-main" tabindex="-1">
        @RenderBody()
    </main>
    <footer class="app-footer">CafePOS Management <span>Không gian bán hàng của bạn</span></footer>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js" asp-append-version="true" defer></script>
    <script src="~/js/site.js" asp-append-version="true" defer></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>

```

### src/CafePOSManagement.Web/Views/Shared/Error.cshtml

```cshtml
@{
    ViewData["Title"] = "Có lỗi xảy ra";
}
<section class="surface error-panel">
    <p class="eyebrow">CafePOS Management</p>
    <h1>Không thể hoàn tất yêu cầu</h1>
    <p>Vui lòng thử lại. Nếu lỗi tiếp tục, hãy liên hệ người quản trị.</p>
    <a class="btn btn-primary" asp-controller="Pos" asp-action="Index">Về trang bán hàng</a>
</section>

```

### src/CafePOSManagement.Web/wwwroot/css/site.css

```css
:root {
    --color-background: #f8f9fa;
    --color-surface: #ffffff;
    --color-primary: #f4511e;
    --color-primary-bright: #ff5722;
    --color-primary-hover: #d84315;
    --color-primary-soft: #fff2ec;
    --color-on-primary: #231209;
    --color-secondary: #2b4c7e;
    --color-text: #20252b;
    --color-muted: #626b76;
    --color-border: #e5e8ec;
    --space-1: 4px;
    --space-2: 8px;
    --space-3: 12px;
    --space-4: 16px;
    --space-5: 24px;
    --space-6: 32px;
    --radius-sm: 8px;
    --radius-md: 12px;
    --radius-lg: 16px;
    --bs-body-bg: var(--color-background);
    --bs-body-color: var(--color-text);
    --bs-primary: var(--color-primary);
    --bs-primary-rgb: 244, 81, 30;
    --bs-body-font-family: "Segoe UI", Arial, sans-serif;
}

* { box-sizing: border-box; }
body { margin: 0; min-width: 320px; font-family: var(--bs-body-font-family); }
a { color: var(--color-secondary); }
button, input { font: inherit; }
:focus-visible { outline: 3px solid var(--color-secondary); outline-offset: 4px; }
.skip-link { position: absolute; top: -100px; left: var(--space-4); z-index: 100; padding: var(--space-3); background: white; }
.skip-link:focus { top: var(--space-3); }
.app-header { min-height: 88px; display: flex; align-items: center; gap: var(--space-5); padding: var(--space-4) var(--space-6); background: var(--color-surface); border-bottom: 1px solid var(--color-border); }
.brand { display: inline-flex; align-items: center; gap: var(--space-3); color: var(--color-text); font-size: 25px; font-weight: 750; line-height: 1; text-decoration: none; white-space: nowrap; }
.brand:hover { color: var(--color-text); }
.brand-accent { color: var(--color-primary-hover); }
.brand small { display: block; margin-top: 6px; color: var(--color-muted); font-size: 9px; letter-spacing: 2.4px; font-weight: 600; }
.header-location { display: flex; align-items: center; gap: var(--space-2); padding-left: var(--space-5); border-left: 1px solid var(--color-border); font-size: 14px; }
.location-mark { width: 8px; height: 8px; border-radius: 50%; background: var(--color-primary); }
.phase-badge { margin-left: auto; background: var(--color-background); border: 1px solid var(--color-border); padding: 6px 10px; border-radius: var(--radius-sm); color: var(--color-muted); font-size: 12px; }
.btn { border-radius: var(--radius-sm); min-height: 42px; font-size: 14px; font-weight: 600; }
.btn-primary { --bs-btn-bg: var(--color-primary); --bs-btn-border-color: var(--color-primary); --bs-btn-color: var(--color-on-primary); --bs-btn-hover-bg: var(--color-primary-hover); --bs-btn-hover-border-color: var(--color-primary-hover); --bs-btn-hover-color: #fff; --bs-btn-active-bg: #bf360c; --bs-btn-active-border-color: #bf360c; --bs-btn-active-color: #fff; --bs-btn-disabled-bg: var(--color-primary); --bs-btn-disabled-border-color: var(--color-primary); --bs-btn-disabled-color: var(--color-on-primary); --bs-btn-disabled-opacity: .5; }
.btn-outline-secondary { --bs-btn-color: var(--color-muted); --bs-btn-border-color: var(--color-border); --bs-btn-hover-bg: var(--color-secondary); --bs-btn-hover-border-color: var(--color-secondary); --bs-btn-active-bg: var(--color-secondary); --bs-btn-active-border-color: var(--color-secondary); }
.app-main { max-width: 1800px; margin: 0 auto; padding: var(--space-6); }
.page-heading { display: flex; justify-content: space-between; align-items: center; gap: var(--space-4); margin-bottom: var(--space-5); }
.eyebrow { margin: 0 0 var(--space-2); color: var(--color-muted); font-size: 11px; font-weight: 700; letter-spacing: 1.5px; }
h1 { margin: 0; font-size: clamp(24px, 2vw, 30px); font-weight: 700; letter-spacing: -.7px; }
.page-heading time { color: var(--color-muted); font-size: 13px; text-align: right; }
.stage-notice { display: flex; align-items: center; gap: var(--space-3); padding: 12px 16px; margin-bottom: var(--space-5); background: var(--color-primary-soft); border: 1px solid #f5d9cc; border-radius: var(--radius-sm); }
.stage-notice p { margin: 0; font-size: 13px; line-height: 1.6; }
.notice-indicator { flex-shrink: 0; width: 4px; height: 24px; background: var(--color-primary); border-radius: 3px; }
.pos-workspace { display: grid; grid-template-columns: minmax(160px, 2fr) minmax(0, 6fr) minmax(290px, 3.5fr); gap: var(--space-4); align-items: stretch; min-height: 560px; }
.surface { min-width: 0; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-md); }
.panel-heading { display: flex; justify-content: space-between; align-items: center; gap: var(--space-2); padding: var(--space-5) var(--space-4); border-bottom: 1px solid var(--color-border); }
h2 { margin: 0; font-size: 16px; font-weight: 700; }
.count-label { color: var(--color-muted); font-size: 12px; white-space: nowrap; }
.categories-panel { display: flex; flex-direction: column; }
.categories-body { padding: var(--space-4) var(--space-3); }
.category-current { display: flex; align-items: center; justify-content: space-between; gap: var(--space-2); padding: var(--space-3); color: var(--color-on-primary); background: var(--color-primary); border-radius: var(--radius-sm); font-size: 13px; font-weight: 650; }
.muted-hint { margin: var(--space-3) 0 0; color: var(--color-muted); font-size: 12px; line-height: 1.7; }
.categories-body .muted-hint { padding: 0 var(--space-1); }
.sidebar-caption { margin-top: auto; padding: var(--space-5) var(--space-4); color: var(--color-muted); font-size: 12px; line-height: 1.9; }
.sidebar-caption strong { color: var(--color-text); font-weight: 600; }
.menu-panel { display: flex; flex-direction: column; }
.menu-toolbar { display: flex; flex-wrap: wrap; gap: var(--space-3); padding: var(--space-4); }
.search-field { flex: 1 1 150px; min-width: 0; }
.form-control { min-height: 42px; border-color: var(--color-border); border-radius: var(--radius-sm); font-size: 13px; }
.form-control:disabled { background: var(--color-background); }
.menu-empty { flex: 1; display: flex; flex-direction: column; justify-content: center; align-items: center; padding: 40px var(--space-5) 64px; text-align: center; }
.empty-emblem { display: grid; place-items: center; width: 112px; height: 112px; margin-bottom: var(--space-5); border: 1px solid #f5d9cc; border-radius: 28px; background: var(--color-primary-soft); color: var(--color-primary-hover); }
h3 { margin: 0 0 var(--space-3); font-size: 18px; font-weight: 650; }
.menu-empty p, .cart-empty p { color: var(--color-muted); font-size: 13px; line-height: 1.8; }
.empty-label { margin-top: var(--space-2); padding: 6px 12px; background: var(--color-background); border-radius: 20px; font-size: 11px; color: var(--color-muted); }
.cart-panel { display: flex; flex-direction: column; }
.order-reference { padding: var(--space-3) var(--space-4); color: var(--color-muted); font-size: 12px; }
.cart-empty { flex: 1; padding: 32px var(--space-4); text-align: center; }
.cart-empty-line { display: block; width: 32px; height: 4px; background: var(--color-border); border-radius: 4px; margin: 0 auto var(--space-4); }
.cart-empty h3 { font-size: 15px; }
.cart-summary { padding: var(--space-5) var(--space-4) var(--space-4); border-top: 1px dashed var(--color-border); }
.cart-summary dl { margin: 0 0 var(--space-5); }
.cart-summary dl > div { display: flex; align-items: baseline; justify-content: space-between; gap: var(--space-3); margin-bottom: var(--space-3); font-size: 13px; }
.cart-summary dt { font-weight: 400; color: var(--color-muted); }
.cart-summary dd { margin: 0; text-align: right; }
.cart-summary .total-row { margin: var(--space-5) 0 0; padding-top: var(--space-4); border-top: 1px dashed var(--color-border); font-size: 21px; }
.total-row dt, .total-row dd { font-weight: 700; color: var(--color-text); }
.checkout-button { width: 100%; min-height: 48px; }
.app-footer { display: flex; justify-content: space-between; gap: var(--space-4); max-width: 1800px; margin: 0 auto; padding: 0 var(--space-6) var(--space-5); font-size: 11px; color: var(--color-muted); }
.error-panel { padding: var(--space-6); }
.error-panel p { margin-top: var(--space-4); }
@media (max-width: 1199.98px) {
    .pos-workspace { grid-template-columns: minmax(0, 1fr) minmax(280px, 340px); }
    .categories-panel { grid-column: 1 / -1; }
    .categories-panel .panel-heading { padding: var(--space-4); }
    .categories-body { display: flex; align-items: center; flex-wrap: wrap; gap: var(--space-4); }
    .category-current { min-width: 150px; }
    .categories-body .muted-hint { margin: 0; }
    .sidebar-caption { display: none; }
    .header-location { display: none; }
}
@media (max-width: 767.98px) {
    .app-header { flex-wrap: wrap; gap: var(--space-3); padding: var(--space-4); }
    .brand { font-size: 23px; }
    .refresh-button { margin-left: auto; }
    .phase-badge { order: 3; margin-left: 0; width: 100%; text-align: center; }
    .app-main { padding: var(--space-5) var(--space-4); }
    .page-heading { align-items: flex-start; flex-direction: column; }
    .page-heading time { text-align: left; }
    .pos-workspace { grid-template-columns: minmax(0, 1fr); }
    .menu-empty { padding: 40px var(--space-4); }
    .cart-empty { min-height: 140px; }
    .app-footer { padding: 0 var(--space-4) var(--space-5); flex-direction: column; gap: var(--space-1); }
}

```

### src/CafePOSManagement.Web/wwwroot/images/cafepos-mark.svg

```xml
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 48 48" fill="none">
  <rect width="48" height="48" rx="13" fill="#F4511E"/>
  <path d="M12 19h20v10a10 10 0 0 1-20 0V19Z" stroke="#231209" stroke-width="2.5"/>
  <path d="M32 21h3a5 5 0 1 1 0 10h-3M11 39h24M19 9v5M26 8v6" stroke="#231209" stroke-width="2.5" stroke-linecap="round"/>
</svg>

```

### src/CafePOSManagement.Web/wwwroot/js/site.js

```javascript
"use strict";

document.querySelector('[data-action="reload"]')?.addEventListener("click", () => {
    window.location.reload();
});

const clock = document.querySelector("[data-local-clock]");
if (clock) {
    const formatter = new Intl.DateTimeFormat("vi-VN", {
        timeZone: "Asia/Ho_Chi_Minh",
        weekday: "long",
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
        hour12: false
    });

    const updateClock = () => {
        const now = new Date();
        clock.dateTime = now.toISOString();
        clock.textContent = formatter.format(now);
    };

    updateClock();
    window.setInterval(updateClock, 30_000);
}

```

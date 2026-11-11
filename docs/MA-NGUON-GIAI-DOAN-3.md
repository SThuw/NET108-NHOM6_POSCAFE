# Giai đoạn 3 — Mã nguồn đầy đủ theo tệp

Bản chụp ngày 07/11/2026. Hướng dẫn và kết quả kiểm thử: [GIAI-DOAN-3.md](GIAI-DOAN-3.md).

## src/CafePOSManagement.Web/Program.cs

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
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

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

## src/CafePOSManagement.Web/Models/Entities/Category.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Models.Entities;

public sealed record Category(
    [property: Required, StringLength(40), Display(Name = "Mã danh mục")] string Id,
    [property: Required, StringLength(100), Display(Name = "Tên danh mục")] string Name);

```

## src/CafePOSManagement.Web/Models/Entities/Product.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Models.Entities;

public sealed record Product(
    string Id,
    [property: Required, StringLength(100), Display(Name = "Tên món")] string Name,
    [property: Required, Display(Name = "Danh mục")] string CategoryId,
    [property: Range(1, 10000000), Display(Name = "Giá gốc")] decimal? OriginalPrice,
    [property: Range(1, 10000000), Display(Name = "Giá bán")] decimal Price,
    [property: StringLength(2048), Display(Name = "Hình ảnh")] string ImageUrl,
    [property: Display(Name = "Đang bán")] bool IsAvailable = true);

```

## src/CafePOSManagement.Web/Models/Entities/DiscountCode.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Models.Entities;

public sealed record DiscountCode(
    [property: Required, StringLength(30)] string Code,
    [property: Required, StringLength(100)] string Name,
    [property: Range(0, 100), Display(Name = "Giảm giá (%)")] decimal Percent);

```

## src/CafePOSManagement.Web/Models/Entities/Order.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Models.Entities;

public sealed record Order
{
    public Guid RequestId { get; init; }
    public string OrderNumber { get; init; } = "";
    [Required, Display(Name = "Loại đơn hàng")]
    public string OrderType { get; init; } = "pickup";
    [Range(0, 100), Display(Name = "Giảm giá (%)")]
    public decimal DiscountPercent { get; init; }
    [Range(0, 100), Display(Name = "Thuế VAT (%)")]
    public decimal TaxPercent { get; init; }
    [StringLength(200), Display(Name = "Ghi chú")]
    public string Note { get; init; } = "";
    [Required, Display(Name = "Phương thức thanh toán")]
    public string PaymentMethod { get; init; } = "cash";
    public DateTimeOffset CreatedAt { get; init; }
    public IReadOnlyList<OrderItem> Items { get; init; } = Array.Empty<OrderItem>();
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal Total { get; init; }
}

```

## src/CafePOSManagement.Web/Models/Entities/OrderItem.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Models.Entities;

// Giữ tên và giá tại thời điểm chốt đơn, không phụ thuộc giá thực đơn về sau.
public sealed record OrderItem(
    string ProductId,
    string ProductName,
    decimal UnitPrice,
    [property: Range(1, 99, ErrorMessage = "Số lượng mỗi món phải từ 1 đến 99.")] int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}

```

## src/CafePOSManagement.Web/Models/ViewModels/CreateProductDto.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Models.ViewModels;

public sealed class CreateProductDto : IValidatableObject
{
    [Required(ErrorMessage = "Vui lòng nhập tên món."), StringLength(100, ErrorMessage = "Tên món tối đa 100 ký tự."), Display(Name = "Tên món")]
    public string Name { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng chọn danh mục."), StringLength(40, ErrorMessage = "Mã danh mục tối đa 40 ký tự."), Display(Name = "Danh mục")]
    public string CategoryId { get; set; } = "";
    [Range(1, 10000000, ErrorMessage = "Giá bán phải từ 1 đến 10.000.000 đồng."), Display(Name = "Giá bán")]
    public decimal Price { get; set; }
    [Range(1, 10000000, ErrorMessage = "Giá gốc phải từ 1 đến 10.000.000 đồng."), Display(Name = "Giá gốc")]
    public decimal? OriginalPrice { get; set; }
    [StringLength(2048, ErrorMessage = "Đường dẫn ảnh tối đa 2048 ký tự."), Display(Name = "Đường dẫn ảnh")]
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Price != decimal.Truncate(Price) || (OriginalPrice.HasValue && OriginalPrice != decimal.Truncate(OriginalPrice.Value)))
            yield return new ValidationResult("Giá tiền phải là số nguyên đồng.", [nameof(Price), nameof(OriginalPrice)]);
        if (OriginalPrice.HasValue && OriginalPrice < Price)
            yield return new ValidationResult("Giá gốc không được nhỏ hơn giá bán.", [nameof(OriginalPrice)]);
        if (!string.IsNullOrWhiteSpace(ImageUrl) &&
            (!Uri.TryCreate(ImageUrl.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo)))
            yield return new ValidationResult("Ảnh phải dùng đường dẫn HTTPS hợp lệ hoặc để trống.", [nameof(ImageUrl)]);
    }
}

```

## src/CafePOSManagement.Web/Models/ViewModels/CheckoutOrderDto.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Models.ViewModels;

public sealed class CheckoutOrderDto : IValidatableObject
{
    public Guid RequestId { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn loại đơn."), RegularExpression("^(pickup|delivery|dinein)$", ErrorMessage = "Loại đơn không hợp lệ."), Display(Name = "Loại đơn")]
    public string OrderType { get; set; } = "pickup";
    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán."), RegularExpression("^(cash|card|wallet)$", ErrorMessage = "Phương thức thanh toán không hợp lệ."), Display(Name = "Thanh toán")]
    public string PaymentMethod { get; set; } = "cash";
    [StringLength(30, ErrorMessage = "Mã giảm giá tối đa 30 ký tự.")]
    public string? DiscountCode { get; set; }
    [Range(0, 100, ErrorMessage = "VAT phải từ 0 đến 100%."), Display(Name = "VAT (%)")]
    public decimal TaxPercent { get; set; }
    [StringLength(200, ErrorMessage = "Ghi chú tối đa 200 ký tự."), Display(Name = "Ghi chú")]
    public string? Note { get; set; }
    [Required(ErrorMessage = "Giỏ hàng không được để trống."), MinLength(1, ErrorMessage = "Giỏ hàng không được để trống."), MaxLength(100, ErrorMessage = "Mỗi đơn tối đa 100 món khác nhau.")]
    public List<CheckoutOrderItemDto> Items { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RequestId == Guid.Empty)
            yield return new ValidationResult("Thiếu mã yêu cầu thanh toán.", [nameof(RequestId)]);
        if (Items is not null && Items.Any(item => item is null))
            yield return new ValidationResult("Chi tiết món không hợp lệ.", [nameof(Items)]);
    }
}

public sealed class CheckoutOrderItemDto
{
    [Required(ErrorMessage = "Thiếu mã món."), StringLength(40, ErrorMessage = "Mã món tối đa 40 ký tự.")]
    public string ProductId { get; set; } = "";
    [Range(1, 99, ErrorMessage = "Số lượng mỗi món phải từ 1 đến 99.")]
    public int Quantity { get; set; }
}

```

## src/CafePOSManagement.Web/Models/ViewModels/PosIndexViewModel.cs

```csharp
using CafePOSManagement.Web.Models.Entities;

namespace CafePOSManagement.Web.Models.ViewModels;

public sealed record PosIndexViewModel(DateTimeOffset CurrentTime, IReadOnlyList<Category> Categories,
    IReadOnlyList<Product> Products, IReadOnlyList<DiscountCode> Discounts,
    IReadOnlyList<decimal> TaxRates, string NextOrderNumber);

```

## src/CafePOSManagement.Web/Services/Interfaces/IPosService.cs

```csharp
using CafePOSManagement.Web.Models.ViewModels;

namespace CafePOSManagement.Web.Services.Interfaces;

public interface IPosService
{
    PosIndexViewModel GetOverview();
}

```

## src/CafePOSManagement.Web/Services/Interfaces/IProductService.cs

```csharp
using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;

namespace CafePOSManagement.Web.Services.Interfaces;

public interface IProductService
{
    IReadOnlyList<Category> GetCategories();
    IReadOnlyList<Product> GetProducts(string? categoryId = null, string? search = null);
    Product Create(CreateProductDto input);
}

```

## src/CafePOSManagement.Web/Services/Interfaces/IOrderService.cs

```csharp
using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;

namespace CafePOSManagement.Web.Services.Interfaces;

public interface IOrderService
{
    IReadOnlyList<DiscountCode> GetDiscountCodes();
    IReadOnlyList<decimal> GetTaxRates();
    string GetNextOrderNumber();
    Order Preview(CheckoutOrderDto input);
    Order Checkout(CheckoutOrderDto input);
}

```

## src/CafePOSManagement.Web/Services/Implementations/PosService.cs

```csharp
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Interfaces;

namespace CafePOSManagement.Web.Services.Implementations;

public sealed class PosService(TimeProvider timeProvider, IProductService products, IOrderService orders) : IPosService
{
    private static readonly TimeZoneInfo VietnamTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");

    public PosIndexViewModel GetOverview()
    {
        var localTime = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), VietnamTimeZone);
        return new PosIndexViewModel(localTime, products.GetCategories(), products.GetProducts(),
            orders.GetDiscountCodes(), orders.GetTaxRates(), orders.GetNextOrderNumber());
    }
}

```

## src/CafePOSManagement.Web/Services/Implementations/ProductService.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Interfaces;

namespace CafePOSManagement.Web.Services.Implementations;

public sealed class ProductService : IProductService
{
    private static readonly object Gate = new();
    private static readonly Category[] Categories =
    [
        new("traditional", "Cà phê truyền thống"), new("snack", "Đồ ăn nhẹ"),
        new("sandwich", "Bánh mì & Sandwich"), new("coldbrew", "Cold Brew đặc biệt"),
        new("espresso", "Cà phê Ý (Espresso)"), new("seasonal", "Đồ uống theo mùa"), new("tea", "Trà trái cây")
    ];
    // shortcut: Dữ liệu chỉ sống trong tiến trình; thay bằng DbContext ở giai đoạn 5.
    private static readonly List<Product> Products =
    [
        new("p1", "Cà phê sữa đá", "traditional", 55000, 45000, "/images/menu/latte.jpg"),
        new("p2", "Cappuccino", "espresso", 65000, 55000, "/images/menu/coffee.jpg"),
        new("p3", "Bánh Croissant bơ", "snack", 50000, 45000, "/images/menu/croissant.jpg"),
        new("p4", "Americano nóng", "espresso", 45000, 40000, "/images/menu/espresso.jpg"),
        new("p5", "Bạc xỉu đá", "traditional", null, 39000, "/images/menu/iced.jpg"),
        new("p6", "Sandwich nướng", "sandwich", 75000, 65000, "/images/menu/sandwich.jpg"),
        new("p7", "Cold Brew sữa tươi", "coldbrew", 60000, 55000, "/images/menu/latte.jpg"),
        new("p8", "Trà chanh thanh mát", "tea", null, 45000, "/images/menu/fruit-tea.jpg"),
        new("p9", "Latte kem sữa", "espresso", 65000, 59000, "/images/menu/coffee.jpg"),
        new("p10", "Cà phê sữa mùa đông", "seasonal", null, 49000, "/images/menu/tea.jpg"),
        new("p11", "Cold Brew đá", "coldbrew", 55000, 49000, "/images/menu/iced.jpg"),
        new("p12", "Bánh quy bơ", "snack", 35000, 29000, "/images/menu/tea.jpg")
    ];

    public IReadOnlyList<Category> GetCategories() => Array.AsReadOnly(Categories);

    public IReadOnlyList<Product> GetProducts(string? categoryId = null, string? search = null)
    {
        var query = Normalize(search ?? "");
        lock (Gate)
            return Products.Where(p => (string.IsNullOrEmpty(categoryId) || p.CategoryId == categoryId) && Normalize(p.Name).Contains(query)).ToArray();
    }

    public Product Create(CreateProductDto input)
    {
        Validator.ValidateObject(input, new ValidationContext(input), true);
        if (!Categories.Any(category => category.Id == input.CategoryId))
            throw new ValidationException("Danh mục không tồn tại.");
        lock (Gate)
        {
            if (Products.Any(product => product.CategoryId == input.CategoryId && Normalize(product.Name) == Normalize(input.Name)))
                throw new ValidationException("Món này đã tồn tại trong danh mục.");
            var product = new Product($"p{Products.Count + 1}", input.Name.Trim(), input.CategoryId,
                input.OriginalPrice, input.Price, string.IsNullOrWhiteSpace(input.ImageUrl) ? "/images/product-fallback.svg" : input.ImageUrl.Trim(), input.IsAvailable);
            Products.Add(product);
            return product;
        }
    }

    private static string Normalize(string text)
    {
        var decomposed = text.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
        return string.Concat(decomposed.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
    }
}

```

## src/CafePOSManagement.Web/Services/Implementations/OrderService.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Interfaces;

namespace CafePOSManagement.Web.Services.Implementations;

public sealed class OrderService(IProductService products, TimeProvider clock) : IOrderService
{
    private static readonly object Gate = new();
    private static readonly DiscountCode[] Discounts = [new("SALE10", "Giảm 10%", 10), new("SALE20", "Giảm 20%", 20)];
    private static readonly decimal[] TaxRates = [0, 5, 8, 10];
    // shortcut: Đơn và khóa chống gửi trùng mất khi restart; chuyển sang transaction + unique key khi dùng SQL Server.
    private static readonly List<(Order Order, string Fingerprint)> Orders = [];
    private static int sequence = 3;

    public IReadOnlyList<DiscountCode> GetDiscountCodes() => Array.AsReadOnly(Discounts);
    public IReadOnlyList<decimal> GetTaxRates() => Array.AsReadOnly(TaxRates);
    public string GetNextOrderNumber() { lock (Gate) return $"#{sequence + 1:000}"; }

    public Order Preview(CheckoutOrderDto input)
    {
        Validate(input);
        return Calculate(input) with { OrderNumber = GetNextOrderNumber() };
    }

    public Order Checkout(CheckoutOrderDto input)
    {
        Validate(input);
        var fingerprint = JsonSerializer.Serialize(input);
        lock (Gate)
        {
            var previous = Orders.FirstOrDefault(entry => entry.Order.RequestId == input.RequestId);
            if (previous.Order is not null)
            {
                if (previous.Fingerprint != fingerprint)
                    throw new ValidationException("Mã yêu cầu đã được dùng cho một nội dung đơn khác. Vui lòng tạo đơn mới.");
                return previous.Order;
            }
            var order = Calculate(input) with { OrderNumber = $"#{++sequence:000}" };
            Orders.Add((order, fingerprint));
            return order;
        }
    }

    private static void Validate(CheckoutOrderDto input)
    {
        Validator.ValidateObject(input, new ValidationContext(input), true);
        foreach (var item in input.Items)
            Validator.ValidateObject(item, new ValidationContext(item), true);
        if (input.Items.Select(item => item.ProductId).Distinct().Count() != input.Items.Count)
            throw new ValidationException("Mỗi món chỉ được xuất hiện một dòng trong đơn.");
        if (!TaxRates.Contains(input.TaxPercent))
            throw new ValidationException("Mức VAT chưa được cấu hình.");
    }

    private Order Calculate(CheckoutOrderDto input)
    {
        var discount = string.IsNullOrEmpty(input.DiscountCode) ? 0 : Discounts.FirstOrDefault(d => d.Code == input.DiscountCode)?.Percent
            ?? throw new ValidationException("Mã giảm giá không hợp lệ.");
        var catalog = products.GetProducts().ToDictionary(p => p.Id);
        var items = input.Items.Select(item =>
        {
            if (!catalog.TryGetValue(item.ProductId, out var product) || !product.IsAvailable)
                throw new ValidationException("Một món không tồn tại hoặc đã ngừng bán. Vui lòng làm mới thực đơn.");
            return new OrderItem(product.Id, product.Name, product.Price, item.Quantity);
        }).ToArray();
        var subtotal = items.Sum(item => item.LineTotal);
        var discountAmount = decimal.Round(subtotal * discount / 100, 0, MidpointRounding.AwayFromZero);
        var tax = decimal.Round((subtotal - discountAmount) * input.TaxPercent / 100, 0, MidpointRounding.AwayFromZero);
        return new Order
        {
            RequestId = input.RequestId, OrderType = input.OrderType, PaymentMethod = input.PaymentMethod,
            DiscountPercent = discount, TaxPercent = input.TaxPercent, Note = input.Note?.Trim() ?? "",
            CreatedAt = clock.GetUtcNow(), Items = Array.AsReadOnly(items), Subtotal = subtotal,
            DiscountAmount = discountAmount, TaxAmount = tax, Total = subtotal - discountAmount + tax
        };
    }
}

```

## src/CafePOSManagement.Web/Controllers/PosController.cs

```csharp
using System.ComponentModel.DataAnnotations;
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CafePOSManagement.Web.Controllers;

public sealed class PosController(IPosService posService, IProductService products, IOrderService orders) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(posService.GetOverview());
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Products(string? categoryId, string? search)
    {
        if (search?.Length > 100 || categoryId?.Length > 40)
            return BadRequest(new { message = "Từ khóa hoặc danh mục quá dài." });
        return Json(products.GetProducts(categoryId, search));
    }

    [HttpPost, RequestSizeLimit(32768)]
    public IActionResult CreateProduct([FromBody] CreateProductDto input) => Execute(() => products.Create(input));

    [HttpPost, RequestSizeLimit(32768)]
    public IActionResult Preview([FromBody] CheckoutOrderDto input) => Execute(() => orders.Preview(input));

    [HttpPost, RequestSizeLimit(32768)]
    public IActionResult Checkout([FromBody] CheckoutOrderDto input) => Execute(() =>
        new { order = orders.Checkout(input), nextOrderNumber = orders.GetNextOrderNumber() });

    private IActionResult Execute<T>(Func<T> action)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Dữ liệu chưa hợp lệ. Kiểm tra các trường nhập.", errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrEmpty(e.ErrorMessage) || e.Exception is not null ? "Sai định dạng dữ liệu đầu vào." : e.ErrorMessage) });
        try { return Json(action()); }
        catch (ValidationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return View("~/Views/Shared/Error.cshtml");
    }
}

```

## src/CafePOSManagement.Web/Views/Pos/Index.cshtml

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
            <div class="dropdown"><button class="icon-button" title="Thông báo" aria-label="Thông báo" type="button" data-bs-toggle="dropdown" aria-expanded="false"><svg class="icon"><use href="#i-bell" /></svg></button><div class="dropdown-menu dropdown-menu-end notification-menu"><strong>Thông báo</strong><p>Chưa có thông báo mới.</p><small>Dữ liệu được lưu tạm đến khi máy chủ khởi động lại.</small></div></div>
        </div>
        <button class="btn btn-outline-secondary" id="print-receipt" type="button"><svg class="icon"><use href="#i-print" /></svg><span>In hóa đơn</span></button>
        <button class="btn btn-outline-secondary back-button" id="go-back" type="button"><svg class="icon"><use href="#i-back" /></svg><span>Quay lại</span></button>
    </div>
}
<div class="workspace-bar"><div><span class="live-dot" aria-hidden="true"></span><h1>Bán hàng tại quầy</h1><span class="workspace-divider">/</span><span class="workspace-caption">Một ngày mới, những ly cà phê ngon.</span></div><span class="demo-badge">Giai đoạn 3 · Lưu tạm trong bộ nhớ</span></div>
@Html.AntiForgeryToken()
<script id="pos-data" type="application/json">@Json.Serialize(Model)</script>
<div class="pos-workspace" id="pos-app" data-products-url="@Url.Action("Products", "Pos")" data-create-url="@Url.Action("CreateProduct", "Pos")" data-checkout-url="@Url.Action("Checkout", "Pos")" data-preview-url="@Url.Action("Preview", "Pos")" data-image-root="@Url.Content("~/images/menu/")" data-fallback-image="@Url.Content("~/images/product-fallback.svg")">
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
            <div class="discount-field"><label class="field-label" for="discount">Giảm giá</label><select id="discount" class="form-select"><option value="">Không giảm giá</option>@foreach (var discount in Model.Discounts) { <option value="@discount.Code">@discount.Name</option> }</select></div>
            <div class="note-field"><label class="field-label" for="order-note">Ghi chú đơn hàng</label><input class="form-control" id="order-note" type="text" maxlength="200" placeholder="Nhập ghi chú (ít đá, giảm ngọt...)" /></div>
        </div>
        <div class="menu-heading"><div><h2 id="menu-title">Tất cả món</h2><span id="product-count">Đang tải thực đơn...</span></div><span class="menu-tip"><svg class="icon"><use href="#i-bag" /></svg>Chọn món để thêm vào giỏ</span></div>
        <div id="product-scroll" class="product-scroll" tabindex="0" aria-label="Danh sách món, có thể cuộn"><div id="product-grid" class="product-grid" aria-busy="true"><p class="loading-message">Đang tải thực đơn...</p></div><div id="no-results" class="empty-state" hidden><svg class="icon"><use href="#i-search" /></svg><h3>Không tìm thấy món phù hợp</h3><p>Thử từ khóa khác hoặc xem tất cả món.</p><button class="btn btn-outline-secondary" id="reset-search" type="button">Xem tất cả món</button></div></div>
        <div class="menu-footnote">Ảnh minh họa từ Unsplash <span>Dữ liệu tạm · Chưa kết nối SQL Server</span></div>
    </section>
    <aside class="cart-panel surface" id="cart-panel" aria-labelledby="cart-title">
        <div class="cart-heading"><div><h2 id="cart-title">Giỏ hàng <span id="cart-count">0</span></h2><p>Đơn hàng hiện tại</p></div><div class="order-meta"><strong id="order-number">Đơn @Model.NextOrderNumber</strong><time data-local-clock datetime="@Model.CurrentTime.ToString("O")">@Model.CurrentTime.ToString("dd/MM/yyyy HH:mm")</time></div></div>
        <div class="cart-scroll" tabindex="0" aria-label="Các món trong giỏ"><div id="cart-items"></div><div id="cart-empty" class="empty-state" hidden><svg class="icon"><use href="#i-bag" /></svg><h3>Giỏ hàng trống</h3><p>Chọn một món từ thực đơn để bắt đầu.</p></div></div>
        <div class="cart-bottom">
            <dl class="order-summary"><div><dt>Tạm tính</dt><dd id="subtotal">0 ₫</dd></div><div id="discount-row" hidden><dt id="discount-label">Giảm giá</dt><dd id="discount-amount">0 ₫</dd></div><div><dt><label for="tax-percent">Thuế VAT (%)</label> <select id="tax-percent" aria-label="Thuế VAT phần trăm">@foreach (var rate in Model.TaxRates) { <option value="@rate">@rate%</option> }</select></dt><dd id="tax-amount">0 ₫</dd></div><div class="total-row"><dt>Tổng cộng</dt><dd id="total" data-testid="cart-total">0 ₫</dd></div></dl>
            <fieldset class="payment-field"><legend>Phương thức thanh toán</legend><div class="payment-options" id="payment-options">
                <label class="payment-option"><input type="radio" name="payment" value="cash" checked /><span><svg class="icon"><use href="#i-cash" /></svg>Tiền mặt</span></label>
                <label class="payment-option"><input type="radio" name="payment" value="card" /><span><svg class="icon"><use href="#i-card" /></svg>Thẻ ngân hàng</span></label>
                <label class="payment-option"><input type="radio" name="payment" value="wallet" /><span><svg class="icon"><use href="#i-wallet" /></svg>Ví điện tử</span></label>
            </div></fieldset>
            <button class="btn btn-primary checkout-button" id="checkout" type="button">Thanh toán ngay <svg class="icon"><use href="#i-arrow" /></svg></button>
            <p class="preview-note">Ghi nhận đơn nội bộ · Chưa kết nối ngân hàng/ví</p>
        </div>
    </aside>
</div>
<a href="#cart-panel" class="mobile-cart-link"><svg class="icon"><use href="#i-bag" /></svg>Xem giỏ hàng <strong id="mobile-total">0 ₫</strong></a>
<div id="pos-status" class="pos-toast" role="status" aria-live="polite" aria-atomic="true"></div>
<noscript><p class="alert alert-warning">Vui lòng bật JavaScript để xem thực đơn và sử dụng POS.</p></noscript>
<template id="product-template"><button class="product-card" type="button"><div class="product-image"><img width="600" height="600" loading="lazy" /><span class="selected-mark" aria-hidden="true"><svg class="icon"><use href="#i-check" /></svg></span></div><div class="product-info"><span class="product-name"></span><div class="product-prices"><del></del><strong></strong></div><span class="product-add" aria-hidden="true">+</span></div></button></template>
<template id="cart-template"><article class="cart-item"><img class="cart-thumbnail" width="44" height="44" alt="" /><div class="cart-item-detail"><h3></h3><span class="cart-unit-price"></span><div class="quantity-control"><button type="button" data-action="decrease">−</button><span class="quantity"></span><button type="button" data-action="increase">+</button></div></div><div class="cart-item-end"><button class="remove-item icon-button" type="button" data-action="remove"><svg class="icon"><use href="#i-trash" /></svg></button><strong class="line-total"></strong></div></article></template>
<div class="modal fade" id="add-product-modal" tabindex="-1" aria-labelledby="add-product-title" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered"><form id="add-product-form" class="modal-content">
        <div class="modal-header"><div><span class="eyebrow">THỰC ĐƠN CỦA QUÁN</span><h2 class="modal-title fs-5" id="add-product-title">Thêm món mới</h2></div><button class="btn-close" type="button" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
        <div class="modal-body">
            <p class="form-preview-note">Món mới được lưu trong bộ nhớ máy chủ ở giai đoạn này.</p>
            <p id="product-error" class="text-danger" role="alert" hidden></p>
            <div class="mb-3"><label class="form-label" for="new-name">Tên món</label><input class="form-control" id="new-name" required maxlength="100" placeholder="Ví dụ: Cà phê sữa đá" /></div>
            <div class="mb-3"><label class="form-label" for="new-category">Danh mục</label><select id="new-category" class="form-select" required></select></div>
            <div class="row g-3 mb-3">
                <div class="col-sm-6"><label class="form-label" for="new-price">Giá bán (₫)</label><input class="form-control" id="new-price" type="number" required min="1" max="10000000" step="1" /></div>
                <div class="col-sm-6"><label class="form-label" for="new-original-price">Giá gốc (tùy chọn)</label><input class="form-control" id="new-original-price" type="number" min="1" max="10000000" step="1" /></div>
            </div>
            <div class="mb-3"><label class="form-label" for="new-image">Đường dẫn ảnh HTTPS (tùy chọn)</label><input class="form-control" id="new-image" type="url" maxlength="2048" placeholder="https://..." /><small class="text-secondary">Để trống để dùng ảnh mặc định. Ảnh không tải được sẽ có ảnh thay thế.</small></div>
            <label class="form-check"><input id="new-available" class="form-check-input" type="checkbox" checked /><span class="form-check-label">Đang bán</span></label>
        </div>
        <div class="modal-footer"><button class="btn btn-outline-secondary" type="button" data-bs-dismiss="modal">Đóng</button><button class="btn btn-primary" id="save-product" type="submit">Lưu món</button></div>
    </form></div>
</div>
<div class="modal fade" id="clear-modal" tabindex="-1" aria-labelledby="clear-title" aria-hidden="true"><div class="modal-dialog modal-dialog-centered modal-sm"><div class="modal-content"><div class="modal-header"><h2 class="modal-title fs-5" id="clear-title">Xóa giỏ hàng?</h2><button class="btn-close" type="button" data-bs-dismiss="modal" aria-label="Đóng"></button></div><div class="modal-body">Các món đang chọn sẽ được bỏ khỏi giỏ hàng.</div><div class="modal-footer"><button class="btn btn-outline-secondary" type="button" data-bs-dismiss="modal">Giữ lại</button><button class="btn btn-primary" id="confirm-clear" type="button">Xóa tất cả món</button></div></div></div></div>
<div class="modal fade" id="calculator-modal" tabindex="-1" aria-labelledby="calculator-title" aria-hidden="true"><div class="modal-dialog modal-dialog-centered modal-sm"><div class="modal-content"><div class="modal-header"><h2 class="modal-title fs-5" id="calculator-title">Máy tính nhẩm</h2><button class="btn-close" type="button" data-bs-dismiss="modal" aria-label="Đóng"></button></div><div class="modal-body"><form id="calculator-form"><label for="calc-a" class="form-label">Số thứ nhất</label><input class="form-control mb-3" id="calc-a" type="number" step="any" required /><label for="calc-operation" class="form-label">Phép tính</label><select id="calc-operation" class="form-select mb-3"><option value="add">Cộng (+)</option><option value="subtract">Trừ (−)</option><option value="multiply">Nhân (×)</option><option value="divide">Chia (÷)</option></select><label for="calc-b" class="form-label">Số thứ hai</label><input class="form-control mb-3" id="calc-b" type="number" step="any" required /><button class="btn btn-primary w-100" type="submit">Tính kết quả</button><output class="calculator-result" id="calc-result" aria-live="polite">Nhập hai số để bắt đầu.</output></form></div></div></div></div>
<div class="modal fade" id="receipt-modal" tabindex="-1" aria-labelledby="receipt-title" aria-hidden="true"><div class="modal-dialog modal-dialog-centered modal-dialog-scrollable"><div class="modal-content"><div class="modal-header"><h2 class="modal-title fs-5" id="receipt-title">Xem trước đơn hàng</h2><button class="btn-close" type="button" data-bs-dismiss="modal" aria-label="Đóng"></button></div><div class="modal-body receipt-content"><div class="receipt-brand">Cafe<span>POS</span></div><p class="receipt-watermark">TẠM TÍNH · CHƯA CHỐT ĐƠN</p><p id="receipt-meta"></p><div id="receipt-lines"></div><dl id="receipt-summary" class="order-summary"></dl><p id="receipt-note"></p><p class="receipt-disclaimer">Phiếu nội bộ, không phải hóa đơn điện tử. Chưa kết nối ngân hàng hoặc ví điện tử.</p></div><div class="modal-footer"><button class="btn btn-outline-secondary" type="button" data-bs-dismiss="modal">Quay lại đơn</button><button class="btn btn-primary" id="print-preview" type="button"><svg class="icon"><use href="#i-print" /></svg>In hóa đơn</button></div></div></div></div>
@section Scripts { <script src="~/js/pos.js" asp-append-version="true" defer></script> }

```

## src/CafePOSManagement.Web/wwwroot/js/pos.js

```javascript
"use strict";
(() => {
    const app = document.querySelector("#pos-app");
    if (!app) return;

    const initial = JSON.parse(document.querySelector("#pos-data").textContent);
    const categories = [{ id: "all", name: "Tất cả món" }, ...initial.categories];
    let products = initial.products;
    let visibleProducts = products;
    const productById = new Map(products.map(product => [product.id, product]));
    const state = { category: "all", search: "", discount: 0, discountCode: "", tax: 0,
        orderType: "pickup", payment: "cash", note: "", cart: new Map() };
    let orderNumber = initial.nextOrderNumber;
    let pendingCheckout = null;
    let checkingOut = false;
    let previewing = false;
    let productRequest = 0;
    let productAbort;
    let searchTimer;
    let lastReceipt = null;
    const money = new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND", maximumFractionDigits: 0 });
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
        const tax = Math.round((subtotal - discount) * state.tax / 100);
        return { subtotal, discount, tax, total: subtotal - discount + tax, count };
    }

    function setImage(image, product) {
        image.alt = image.classList.contains("cart-thumbnail") ? "" : product.name;
        image.src = product.imageUrl;
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
            count.textContent = String(category.id === "all" ? products.length : products.filter(product => product.categoryId === category.id).length);
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
        $("#new-category").replaceChildren();
        categories.filter(category => category.id !== "all").forEach(category => {
            const option = new Option(category.name, category.id);
            $("#new-category").add(option);
        });
    }

    function renderProducts() {
        const visible = visibleProducts;
        const fragment = document.createDocumentFragment();
        visible.forEach(product => {
            const card = $("#product-template").content.firstElementChild.cloneNode(true);
            card.dataset.product = product.id;
            card.disabled = !product.isAvailable || Boolean(pendingCheckout) || previewing;
            if (!product.isAvailable) card.title = "Món đang ngừng bán";
            card.setAttribute("aria-label", `Thêm ${product.name}, ${money.format(product.price)} vào giỏ`);
            setImage(card.querySelector("img"), product);
            card.querySelector(".product-name").textContent = product.name + (product.isAvailable ? "" : " · Ngừng bán");
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
            card.disabled = !productById.get(card.dataset.product).isAvailable || Boolean(pendingCheckout) || previewing;
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
            decrease.disabled = Boolean(pendingCheckout) || previewing;
            increase.disabled = quantity >= 99 || Boolean(pendingCheckout) || previewing;
            row.querySelector('[data-action="remove"]').disabled = Boolean(pendingCheckout) || previewing;
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
        $("#tax-amount").textContent = money.format(amount.tax);
        $("#checkout").disabled = checkingOut || previewing;
        $("#checkout").textContent = checkingOut ? "Đang ghi nhận đơn..." : pendingCheckout ? "Thử xác nhận lại đơn" : "Thanh toán ngay";
        $("#print-receipt").disabled = Boolean(pendingCheckout) || previewing || (!amount.count && !lastReceipt);
        $("#clear-cart").disabled = Boolean(pendingCheckout) || previewing;
        $("#pos-app").querySelectorAll("#order-types button, #discount, #tax-percent, #order-note, input[name=payment]").forEach(el => { el.disabled = Boolean(pendingCheckout) || previewing; });
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
        loadProducts();
        $("#product-scroll").scrollTop = 0;
    });
    $("#product-search").addEventListener("input", event => { state.search = event.target.value; window.clearTimeout(searchTimer); searchTimer = window.setTimeout(loadProducts, 180); });
    $("#reset-search").addEventListener("click", () => {
        state.search = "";
        $("#product-search").value = "";
        $("#category-list [data-category='all']").click();
        $("#product-search").focus();
    });
    grid.addEventListener("click", event => {
        const card = event.target.closest("[data-product]");
        if (pendingCheckout || previewing || !card || !productById.get(card.dataset.product)?.isAvailable) return;
        const id = card.dataset.product;
        const quantity = state.cart.get(id) || 0;
        if (!quantity && state.cart.size >= 100) { notify("Mỗi đơn tối đa 100 món khác nhau."); return; }
        if (quantity >= 99) { notify("Mỗi món tối đa 99 phần trong một đơn."); return; }
        state.cart.set(id, quantity + 1);
        renderCart();
        notify(`Đã thêm ${productById.get(id).name} vào giỏ.`);
    });
    cartItems.addEventListener("click", event => {
        const button = event.target.closest("[data-action]");
        const id = button?.closest("[data-product]")?.dataset.product;
        if (pendingCheckout || previewing || !button || !state.cart.has(id)) return;
        const quantity = state.cart.get(id);
        if (button.dataset.action === "remove") state.cart.delete(id);
        else if (button.dataset.action === "increase" && quantity < 99) state.cart.set(id, quantity + 1);
        else if (button.dataset.action === "decrease") { if (quantity === 1) state.cart.delete(id); else state.cart.set(id, quantity - 1); }
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
        state.discountCode = event.target.value;
        state.discount = initial.discounts.find(d => d.code === state.discountCode)?.percent || 0;
        renderCart();
    });
    $("#tax-percent").addEventListener("change", event => { state.tax = Number(event.target.value); renderCart(); });
    $("#order-note").addEventListener("input", event => { state.note = event.target.value.slice(0, 200); });
    $("#payment-options").addEventListener("change", event => {
        if (["cash", "card", "wallet"].includes(event.target.value)) state.payment = event.target.value;
    });
    $("#clear-cart").addEventListener("click", () => bootstrap.Modal.getOrCreateInstance($("#clear-modal")).show());
    $("#confirm-clear").addEventListener("click", () => {
        if (pendingCheckout || previewing) return;
        resetOrder();
        bootstrap.Modal.getInstance($("#clear-modal")).hide();
        notify("Đã xóa giỏ và đặt lại cấu hình đơn.");
    });
    $("#refresh-pos").addEventListener("click", () => loadProducts(true));
    $("#go-back").addEventListener("click", () => {
        // Không tự chuyển người dùng tới một website khác từ bản xem trước.
        if (document.referrer && new URL(document.referrer).origin === window.location.origin && window.history.length > 1) window.history.back();
        else notify("Bạn đang ở màn hình đầu tiên. Trang quản trị chưa triển khai.");
    });

    async function request(url, data, signal) {
        const controller = new AbortController();
        const abort = () => controller.abort();
        signal?.addEventListener("abort", abort, { once: true });
        if (signal?.aborted) abort();
        const timeoutId = window.setTimeout(abort, 20000);
        try {
            const response = await fetch(url, { method: data ? "POST" : "GET", signal: controller.signal,
                headers: data ? { "Content-Type": "application/json", "X-CSRF-TOKEN": $("input[name=__RequestVerificationToken]").value } : {},
                body: data ? JSON.stringify(data) : undefined });
            const result = await response.json().catch(() => null);
            if (!response.ok) {
                const error = new Error(result?.errors?.join(" ") || result?.message || "Yêu cầu không thành công. Vui lòng tải lại trang nếu phiên đã hết hạn.");
                // Chỉ lỗi từ chối đầu vào chắc chắn chưa ghi nhận đơn. Lỗi 5xx cần thử lại cùng RequestId.
                error.rejected = response.status >= 400 && response.status < 500;
                throw error;
            }
            if (!result) throw new Error("Không đọc được phản hồi máy chủ. Vui lòng thử lại.");
            return result;
        } catch (error) {
            if (signal?.aborted) throw error;
            if (error.name === "AbortError" || error instanceof TypeError)
                throw new Error("Không kết nối được máy chủ hoặc đã quá thời gian chờ. Vui lòng thử lại.");
            throw error;
        } finally {
            window.clearTimeout(timeoutId);
            signal?.removeEventListener("abort", abort);
        }
    }

    async function loadProducts(refreshAll = false) {
        const requestId = ++productRequest;
        productAbort?.abort();
        productAbort = new AbortController();
        grid.setAttribute("aria-busy", "true");
        $("#product-count").textContent = "Đang tải...";
        try {
            if (refreshAll) {
                const all = await request(app.dataset.productsUrl, null, productAbort.signal);
                if (requestId !== productRequest) return;
                products = all;
                all.forEach(p => productById.set(p.id, p));
                renderCategories();
            }
            const url = new URL(app.dataset.productsUrl, location.href);
            if (state.category !== "all") url.searchParams.set("categoryId", state.category);
            url.searchParams.set("search", state.search);
            const found = await request(url, null, productAbort.signal);
            if (requestId !== productRequest) return;
            visibleProducts = found;
            found.forEach(p => productById.set(p.id, p));
            renderProducts();
            renderCart();
        } catch (error) {
            if (error.name === "AbortError" || requestId !== productRequest) return;
            visibleProducts = [];
            renderProducts();
            $("#product-count").textContent = "Không tải được · Bấm Làm mới để thử lại";
            notify(error.message);
        } finally { if (requestId === productRequest) grid.setAttribute("aria-busy", "false"); }
    }

    function orderPayload() {
        return { requestId: crypto.randomUUID(), orderType: state.orderType, paymentMethod: state.payment,
            discountCode: state.discountCode || null, taxPercent: state.tax, note: state.note,
            items: [...state.cart].map(([productId, quantity]) => ({ productId, quantity })) };
    }

    function resetOrder() {
        state.cart.clear();
        state.discount = 0; state.discountCode = ""; state.tax = 0; state.note = "";
        state.orderType = "pickup"; state.payment = "cash";
        $("#discount").value = ""; $("#tax-percent").value = "0"; $("#order-note").value = "";
        $("input[name=payment][value=cash]").checked = true;
        $("#order-types").querySelectorAll("button").forEach(button => {
            button.classList.toggle("active", button.dataset.order === "pickup");
            button.setAttribute("aria-pressed", String(button.dataset.order === "pickup"));
        });
        renderCart();
    }

    function showReceipt(order, committed) {
        const orderNames = { pickup: "Mang đi", delivery: "Giao hàng", dinein: "Tại bàn" };
        const paymentNames = { cash: "Tiền mặt", card: "Thẻ ngân hàng", wallet: "Ví điện tử" };
        const date = new Intl.DateTimeFormat("vi-VN", { dateStyle: "short", timeStyle: "short", timeZone: "Asia/Ho_Chi_Minh" }).format(new Date(order.createdAt));
        $("#receipt-title").textContent = committed ? "Đơn đã ghi nhận" : "Hóa đơn tạm tính";
        $(".receipt-watermark").textContent = committed ? "ĐÃ GHI NHẬN ĐƠN NỘI BỘ" : "TẠM TÍNH · CHƯA CHỐT ĐƠN";
        $("#receipt-meta").textContent = `Đơn ${order.orderNumber} · ${date} · ${orderNames[order.orderType]} · ${paymentNames[order.paymentMethod]}`;
        const fragment = document.createDocumentFragment();
        order.items.forEach(item => {
            const row = document.createElement("div"); row.className = "receipt-line";
            const label = document.createElement("span");
            label.textContent = `${item.quantity} × ${item.productName} (${money.format(item.unitPrice)})`;
            const value = document.createElement("strong"); value.textContent = money.format(item.lineTotal);
            row.append(label, value); fragment.append(row);
        });
        $("#receipt-lines").replaceChildren(fragment);
        const summary = document.createDocumentFragment();
        [["Tạm tính", order.subtotal], [`Giảm giá (${order.discountPercent}%)`, -order.discountAmount],
            [`Thuế VAT (${order.taxPercent}%)`, order.taxAmount], ["Tổng cộng", order.total]].forEach(([label, value], index) => {
            const row = document.createElement("div"); if (index === 3) row.className = "total-row";
            const term = document.createElement("dt"); term.textContent = label;
            const description = document.createElement("dd"); description.textContent = money.format(value);
            row.append(term, description); summary.append(row);
        });
        $("#receipt-summary").replaceChildren(summary);
        $("#receipt-note").textContent = order.note ? `Ghi chú: ${order.note}` : "";
        bootstrap.Modal.getOrCreateInstance($("#receipt-modal")).show();
    }

    $("#checkout").addEventListener("click", async () => {
        if (checkingOut || previewing) return;
        if (!state.cart.size) { notify("Giỏ hàng trống. Vui lòng chọn món trước khi thanh toán."); return; }
        pendingCheckout ||= orderPayload();
        checkingOut = true; renderCart();
        try {
            const result = await request(app.dataset.checkoutUrl, pendingCheckout);
            lastReceipt = result.order;
            orderNumber = result.nextOrderNumber;
            $("#order-number").textContent = `Đơn ${orderNumber}`;
            pendingCheckout = null;
            resetOrder();
            showReceipt(lastReceipt, true);
            notify(`Đã ghi nhận đơn ${lastReceipt.orderNumber}.`);
        } catch (error) {
            if (error.rejected) pendingCheckout = null;
            notify(pendingCheckout ? "Chưa xác nhận được kết quả. Giữ nguyên trang và bấm Thử xác nhận lại đơn để tránh tạo trùng." : error.message);
        } finally { checkingOut = false; renderCart(); }
    });
    $("#print-receipt").addEventListener("click", async () => {
        if (pendingCheckout || previewing) return;
        if (!state.cart.size) { if (lastReceipt) showReceipt(lastReceipt, true); return; }
        previewing = true; renderCart();
        try { showReceipt(await request(app.dataset.previewUrl, orderPayload()), false); }
        catch (error) { notify(error.message); }
        finally { previewing = false; renderCart(); }
    });
    $("#print-preview").addEventListener("click", () => window.print());
    window.addEventListener("beforeunload", event => {
        if (pendingCheckout || state.cart.size) { event.preventDefault(); event.returnValue = ""; }
    });

    $("#add-product-form").addEventListener("submit", async event => {
        event.preventDefault();
        const button = $("#save-product");
        if (button.disabled) return;
        const errorBox = $("#product-error"); errorBox.hidden = true;
        const payload = { name: $("#new-name").value.trim(), categoryId: $("#new-category").value,
            price: Number($("#new-price").value), originalPrice: $("#new-original-price").value === "" ? null : Number($("#new-original-price").value),
            imageUrl: $("#new-image").value.trim() || null, isAvailable: $("#new-available").checked };
        if (!payload.name) { errorBox.textContent = "Vui lòng nhập tên món."; errorBox.hidden = false; return; }
        button.disabled = true; button.textContent = "Đang lưu...";
        try {
            const product = await request(app.dataset.createUrl, payload);
            products.push(product); productById.set(product.id, product);
            state.category = product.categoryId; state.search = ""; $("#product-search").value = "";
            renderCategories();
            event.target.reset();
            bootstrap.Modal.getOrCreateInstance($("#add-product-modal")).hide();
            await loadProducts(); notify("Đã thêm món mới vào thực đơn.");
        } catch (error) { errorBox.textContent = error.message; errorBox.hidden = false; }
        finally { button.disabled = false; button.textContent = "Lưu món"; }
    });
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
    renderCategories();
    renderProducts();
    renderCart();
})();

```

## src/CafePOSManagement.Web/wwwroot/css/site.css

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
.product-card:disabled { opacity: .55; cursor: not-allowed; }
#tax-percent { border: 1px solid var(--color-border); border-radius: 4px; background: white; color: var(--color-text); font-size: 11px; padding: 2px; }
#tax-percent:focus-visible { outline: 2px solid var(--color-primary); outline-offset: 2px; }
#receipt-modal .receipt-content { width: 80mm; max-width: 100%; margin: 0 auto; }
.receipt-line span { min-width: 0; overflow-wrap: anywhere; }
.receipt-line strong { flex-shrink: 0; }
#receipt-meta { overflow-wrap: anywhere; }
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
@media print {
    @page { size: 80mm 297mm; margin: 3mm; }
    #receipt-modal { padding: 0 !important; }
    #receipt-modal .modal-dialog { width: 74mm; }
    #receipt-modal .modal-content { border-radius: 0; }
    #receipt-modal .receipt-content { padding: 3mm; color: black; }
    .receipt-line, #receipt-summary > div { break-inside: avoid; }
    #receipt-summary .total-row dd { font-size: 18px; }
}

```

## tests/CafePOSManagement.Checks/CafePOSManagement.Checks.csproj

```
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/CafePOSManagement.Web/CafePOSManagement.Web.csproj" />
  </ItemGroup>
</Project>

```

## tests/CafePOSManagement.Checks/Program.cs

```
using System.ComponentModel.DataAnnotations;
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Implementations;

var products = new ProductService();
var orders = new OrderService(products, TimeProvider.System);
var passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    Console.WriteLine("PASS: " + name); passed++;
}
void Reject(Action action, string name)
{
    try { action(); }
    catch (ValidationException) { Check(true, name); return; }
    throw new Exception("FAIL: " + name + " did not reject");
}
CheckoutOrderDto Basket() => new()
{
    RequestId = Guid.NewGuid(), DiscountCode = "SALE10", TaxPercent = 8,
    Items = [new() { ProductId = "p1", Quantity = 2 }, new() { ProductId = "p2", Quantity = 1 }]
};
Check(products.GetProducts(search: "  CA PHE SUA DA ").Single().Id == "p1", "Accent-insensitive search including d-stroke");
Check(products.GetProducts("espresso").Count == 3, "Category filter");
Check(products.GetProducts("tea", "ca phe").Count == 0, "Combined category and search");
Check(products.GetProducts(search: "   ").Count == 12, "Whitespace search");
var preview = orders.Preview(Basket());
Check(preview.Subtotal == 145000 && preview.DiscountAmount == 14500 && preview.TaxAmount == 10440 && preview.Total == 140940, "Server money: discount before tax");
Check(orders.GetNextOrderNumber() == "#004", "Preview does not consume order number");
var input = Basket();
var submissions = Enumerable.Range(0, 20).Select(_ => Task.Run(() => orders.Checkout(input))).ToArray();
Task.WaitAll(submissions);
Check(submissions.Select(t => t.Result.OrderNumber).Distinct().Single() == "#004" && orders.GetNextOrderNumber() == "#005", "20 concurrent retries create one order");
input.Note = "Changed";
Reject(() => orders.Checkout(input), "Idempotency key cannot be reused with different payload");
var separate = Enumerable.Range(0, 20).Select(_ => Task.Run(() => orders.Checkout(Basket()))).ToArray();
Task.WaitAll(separate);
Check(separate.Select(t => t.Result.OrderNumber).Distinct().Count() == 20, "Concurrent independent checkouts have unique numbers");
var invalid = Basket(); invalid.Items.Clear(); Reject(() => orders.Checkout(invalid), "Empty cart");
invalid = Basket(); invalid.Items[0].Quantity = 0; Reject(() => orders.Checkout(invalid), "Zero quantity");
invalid = Basket(); invalid.Items[0].Quantity = -1; Reject(() => orders.Checkout(invalid), "Negative quantity");
invalid = Basket(); invalid.Items[0].Quantity = 100; Reject(() => orders.Checkout(invalid), "Quantity upper limit");
invalid = Basket(); invalid.Items[0].ProductId = "missing"; Reject(() => orders.Checkout(invalid), "Missing product");
invalid = Basket(); invalid.Items.Add(invalid.Items[0]); Reject(() => orders.Checkout(invalid), "Duplicate product lines");
invalid = Basket(); invalid.Items = null!; Reject(() => orders.Checkout(invalid), "Null cart");
invalid = Basket(); invalid.Items.Add(null!); Reject(() => orders.Checkout(invalid), "Null line");
invalid = Basket(); invalid.DiscountCode = "FREE"; Reject(() => orders.Checkout(invalid), "Unknown discount");
invalid = Basket(); invalid.TaxPercent = 99; Reject(() => orders.Checkout(invalid), "Unconfigured tax");
invalid = Basket(); invalid.OrderType = "unknown"; Reject(() => orders.Checkout(invalid), "Invalid order type");
invalid = Basket(); invalid.PaymentMethod = "bitcoin"; Reject(() => orders.Checkout(invalid), "Invalid payment method");
invalid = Basket(); invalid.Note = new string('x', 201); Reject(() => orders.Checkout(invalid), "Note length");
invalid = Basket(); invalid.RequestId = Guid.Empty; Reject(() => orders.Checkout(invalid), "Missing request ID");
CreateProductDto NewProduct() => new() { Name = "  Món kiểm thử  ", CategoryId = "tea", Price = 5 };
var newProduct = products.Create(NewProduct());
Check(newProduct.Name == "Món kiểm thử" && newProduct.ImageUrl == "/images/product-fallback.svg", "New product trimmed with fallback");
Reject(() => products.Create(NewProduct()), "Duplicate product blocked");
var badProduct = NewProduct(); badProduct.Name = " "; Reject(() => products.Create(badProduct), "Blank name");
badProduct = NewProduct(); badProduct.Price = 1.5m; Reject(() => products.Create(badProduct), "Fractional VND rejected");
badProduct = NewProduct(); badProduct.Price = -5; Reject(() => products.Create(badProduct), "Negative price rejected");
badProduct = NewProduct(); badProduct.OriginalPrice = 4; Reject(() => products.Create(badProduct), "Original price below selling price");
badProduct = NewProduct(); badProduct.CategoryId = "missing"; Reject(() => products.Create(badProduct), "Invalid category");
badProduct = NewProduct(); badProduct.ImageUrl = "javascript:alert(1)"; Reject(() => products.Create(badProduct), "Unsafe image scheme");
var rounded = orders.Preview(new() { RequestId = Guid.NewGuid(), Items = [new() { ProductId = newProduct.Id, Quantity = 1 }], DiscountCode = "SALE10", TaxPercent = 10 });
Check(rounded.DiscountAmount == 1 && rounded.TaxAmount == 0 && rounded.Total == 4, "Half-dong rounding away from zero");
var unavailable = products.Create(new() { Name = "Ngừng bán", CategoryId = "tea", Price = 10000, IsAvailable = false });
invalid = Basket(); invalid.Items[0].ProductId = unavailable.Id; Reject(() => orders.Checkout(invalid), "Unavailable product rejected");
Console.WriteLine($"ALL {passed} CHECKS PASSED");

```

## tests/pos-stage3-ui.cjs

```
async page => {
    const results = [];
    const errors = [];
    const onError = error => errors.push(error.message);
    page.on('pageerror', onError);
    const check = (condition, name) => { if (!condition) throw new Error('FAIL: ' + name); results.push(name); };
    const total = async () => (await page.locator('#total').textContent()).replace(/\D/g, '');
    const filterDone = () => page.waitForFunction(() => document.querySelector('#product-grid').getAttribute('aria-busy') === 'false');
    await page.setViewportSize({ width: 1440, height: 900 });
    check(await page.locator('.product-card').count() === 12, 'Initial 12 products from MVC model');
    check(await page.locator('#cart-count').textContent() === '0', 'Initial cart empty');
    await page.locator('#checkout').click();
    check((await page.locator('#pos-status').textContent()).includes('Giỏ hàng trống'), 'Empty checkout gives error');
    await page.locator('[data-category=traditional]').click(); await filterDone();
    check(await page.locator('.product-card').count() === 2, 'AJAX category filter');
    await page.locator('#product-search').fill('CA PHE SUA DA');
    await page.waitForFunction(() => document.querySelectorAll('.product-card').length === 1);
    check(await page.locator('.product-name').textContent() === 'Cà phê sữa đá', 'Accent and case insensitive AJAX search');
    await page.locator('#product-search').fill('khong-co-mon-nay');
    await page.locator('#no-results').waitFor({ state: 'visible' });
    check(await page.locator('.product-card').count() === 0, 'Empty search state');
    await page.locator('#reset-search').click(); await filterDone();
    await page.locator('.product-card[data-product=p1]').click();
    await page.locator('.product-card[data-product=p1]').click();
    await page.locator('.product-card[data-product=p2]').click();
    check(await total() === '145000', 'Add repeat product increments and sums');
    check(await page.locator('.product-card.selected').count() === 2, 'Orange selection markers');
    await page.locator('#discount').selectOption('SALE10');
    await page.locator('#tax-percent').selectOption('8');
    check(await total() === '140940', 'UI discount and VAT matches server formula');
    await page.locator('[data-order=delivery]').click();
    await page.locator('label.payment-option').filter({ has: page.locator('input[value=wallet]') }).click();
    await page.locator('#order-note').fill('Ít đá <script>alert(1)</script>');
    await page.screenshot({ path: 'artifacts/stage3-desktop.png', fullPage: true });
    await page.locator('#print-receipt').click();
    await page.locator('#receipt-modal.show').waitFor();
    check((await page.locator('#receipt-summary').textContent()).includes('140.940'), 'Server receipt money');
    check((await page.locator('#receipt-meta').textContent()).includes('Giao hàng'), 'Receipt order type and method');
    check(await page.locator('#receipt-note script').count() === 0, 'Note rendered as text, no script');
    await page.evaluate(() => { window.print = () => { window.__printCalled = true; }; });
    await page.locator('#print-preview').click();
    check(await page.evaluate(() => window.__printCalled === true), 'Print button calls browser print');
    await page.emulateMedia({ media: 'print' });
    await page.locator('#receipt-modal .modal-dialog').screenshot({ path: 'artifacts/stage3-receipt.png' });
    await page.pdf({ path: 'artifacts/stage3-receipt.pdf', preferCSSPageSize: true, printBackground: true });
    check(await page.locator('.app-header').isHidden(), 'Print hides POS controls');
    await page.emulateMedia({ media: 'screen' });
    await page.locator('#receipt-modal .btn-close').click();
    await page.locator('#receipt-modal').waitFor({ state: 'hidden' });
    const originalNumber = await page.locator('#order-number').textContent();
    await page.locator('#checkout').dblclick();
    await page.locator('#receipt-modal.show').waitFor();
    check(await page.locator('#cart-count').textContent() === '0', 'Successful checkout clears cart');
    check(await page.locator('#order-number').textContent() !== originalNumber, 'Next order number returned');
    check(await page.locator('#discount').inputValue() === '' && await page.locator('#tax-percent').inputValue() === '0', 'Successful checkout resets configuration');
    check((await page.locator('.receipt-watermark').textContent()).includes('ĐÃ GHI NHẬN'), 'Committed receipt shown');
    await page.locator('#receipt-modal .btn-close').click();
    await page.locator('#receipt-modal').waitFor({ state: 'hidden' });
    await page.locator('.product-card[data-product=p1]').click();
    await page.locator('.cart-item [data-action=decrease]').click();
    check(await page.locator('.cart-item').count() === 0 && await page.locator('.product-card.selected').count() === 0, 'Minus one removes line and selection');
    await page.locator('.product-card[data-product=p1]').click();
    await page.locator('.cart-item [data-action=increase]').click();
    check(await page.locator('.quantity').textContent() === '2', 'Cart plus increments');
    await page.locator('.cart-item [data-action=remove]').click();
    check(await page.locator('.cart-item').count() === 0, 'Trash removes item');
    await page.locator('.product-card[data-product=p1]').click();
    await page.locator('#discount').selectOption('SALE20');
    await page.locator('[data-order=dinein]').click();
    await page.locator('#clear-cart').click();
    await page.locator('#clear-modal .btn-outline-secondary').click();
    await page.locator('#clear-modal').waitFor({ state: 'hidden' });
    check(await page.locator('.cart-item').count() === 1, 'Cancel clear preserves cart');
    await page.locator('#clear-cart').click();
    await page.locator('#confirm-clear').click();
    await page.locator('#clear-modal').waitFor({ state: 'hidden' });
    check(await page.locator('.cart-item').count() === 0 && await page.locator('#discount').inputValue() === '', 'Confirmed clear resets cart and discount');
    check(await page.locator('[data-order=pickup]').getAttribute('aria-pressed') === 'true', 'Clear resets order type');
    await page.locator('.add-product').click();
    await page.locator('#new-name').fill('Kiểm thử <b>Trà</b> ' + Date.now());
    await page.locator('#new-category').selectOption('tea');
    await page.locator('#new-price').fill('31001');
    await page.locator('#new-original-price').fill('30000');
    await page.locator('#save-product').click();
    await page.locator('#product-error').waitFor({ state: 'visible' });
    check((await page.locator('#product-error').textContent()).includes('Giá gốc'), 'Server validation errors shown in form');
    await page.locator('#new-original-price').fill('35000');
    await page.locator('#save-product').click();
    await page.locator('#add-product-modal').waitFor({ state: 'hidden' }); await filterDone();
    check(await page.locator('.product-card').count() === 2, 'New product visible immediately in its category');
    check(await page.locator('.product-name b').count() === 0, 'Product name safe text');
    const createdCard = page.locator('.product-card').filter({ hasText: 'Kiểm thử' });
    check((await createdCard.locator('img').getAttribute('src')).includes('product-fallback.svg'), 'New product fallback image');
    await createdCard.click();
    check(await total() === '31001', 'New product can be ordered');
    for (const width of [1440, 1024, 768, 390]) {
        await page.setViewportSize({ width, height: 900 });
        check(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth), 'No horizontal overflow at ' + width);
    }
    await page.screenshot({ path: 'artifacts/stage3-mobile.png', fullPage: true });
    await page.setViewportSize({ width: 1440, height: 900 });
    await page.locator('#clear-cart').click(); await page.locator('#confirm-clear').click();
    await page.locator('#clear-modal').waitFor({ state: 'hidden' });
    await page.reload();
    check(await page.locator('.product-card').count() === 13, 'Created product survives browser reload');
    check(errors.length === 0, 'No JavaScript runtime errors');
    page.off('pageerror', onError);
    return { passed: results.length, results, errors };
}

```

## tests/pos-stage3-api.cjs

```
async page => {
    const token = await page.locator('input[name=__RequestVerificationToken]').inputValue();
    const headers = { 'X-CSRF-TOKEN': token };
    const results = [];
    const check = (condition, name) => { if (!condition) throw new Error('FAIL: ' + name); results.push(name); };
    const basket = () => ({ requestId: crypto.randomUUID(), orderType: 'pickup', paymentMethod: 'cash', discountCode: 'SALE10', taxPercent: 8,
        items: [{ productId: 'p1', quantity: 2 }, { productId: 'p2', quantity: 1 }] });
    // page.request shares browser cookies and uses absolute URLs when baseURL is not configured.
    const base = new URL(page.url()).origin;
    const send = (action, data, csrf = true) => page.request.post(base + '/Pos/' + action, { data, headers: csrf ? headers : {} });
    check((await send('Checkout', basket(), false)).status() === 400, 'POST without antiforgery rejected');
    const input = basket(); input.total = 1; input.items[0].price = 1; input.discountPercent = 100;
    const first = await send('Checkout', input); check(first.ok(), 'Valid checkout accepted');
    const saved = await first.json();
    check(saved.order.total === 140940, 'Client price, total and discount tampering ignored');
    const retries = await Promise.all(Array.from({ length: 12 }, () => send('Checkout', input)));
    const bodies = await Promise.all(retries.map(r => r.json()));
    check(bodies.every(r => r.order.orderNumber === saved.order.orderNumber && r.nextOrderNumber === saved.nextOrderNumber), '12 HTTP retries keep one order');
    input.note = 'different';
    check((await send('Checkout', input)).status() === 400, 'Changed payload with same key rejected');
    for (const [label, mutate] of [
        ['empty cart', p => p.items = []], ['null cart', p => p.items = null], ['null line', p => p.items = [null]],
        ['negative quantity', p => p.items[0].quantity = -1], ['excess quantity', p => p.items[0].quantity = 100],
        ['fractional quantity', p => p.items[0].quantity = 1.5], ['unknown product', p => p.items[0].productId = 'missing'],
        ['unknown discount', p => p.discountCode = 'FREE'], ['unsupported VAT', p => p.taxPercent = 12],
        ['missing request ID', p => delete p.requestId], ['invalid order type', p => p.orderType = 'x'],
        ['invalid payment', p => p.paymentMethod = 'x'], ['null order type', p => p.orderType = null],
        ['long note', p => p.note = 'a'.repeat(201)]
    ]) {
        const payload = basket(); mutate(payload);
        check((await send('Checkout', payload)).status() === 400, 'Reject ' + label);
    }
    check((await send('Checkout', null)).status() === 400, 'Reject null JSON body');
    const malformed = await page.request.post(base + '/Pos/Checkout', { headers: { ...headers, 'Content-Type': 'application/json' }, data: '{bad' });
    check(malformed.status() === 400, 'Reject malformed JSON');
    for (const payload of [
        { name: ' ', categoryId: 'tea', price: 5 }, { name: 'Bad', categoryId: 'missing', price: 5 },
        { name: 'Bad', categoryId: 'tea', price: -1 }, { name: 'Bad', categoryId: 'tea', price: 1.5 },
        { name: 'Bad', categoryId: 'tea', price: 5, imageUrl: 'javascript:alert(1)' }
    ]) check((await send('CreateProduct', payload)).status() === 400, 'Reject invalid product ' + JSON.stringify(payload));
    const inactive = await send('CreateProduct', { name: 'Kiểm thử ngừng bán ' + Date.now(), categoryId: 'tea', price: 20000, isAvailable: false });
    const inactiveProduct = await inactive.json(); const bad = basket(); bad.items[0].productId = inactiveProduct.id;
    check((await send('Checkout', bad)).status() === 400, 'Cannot checkout unavailable product');
    return { passed: results.length, results };
}

```

## tests/pos-stage3-recovery.cjs

```
async page => {
    const results = [];
    const check = (condition, name) => { if (!condition) throw new Error('FAIL: ' + name); results.push(name); };
    await page.reload();
    await page.locator('.product-card[data-product=p1]').click();
    let committed;
    const lostResponse = async route => {
        const response = await route.fetch();
        committed = await response.json();
        await route.abort('connectionreset');
    };
    await page.route('**/Pos/Checkout', lostResponse);
    await page.locator('#checkout').click();
    await page.waitForFunction(() => document.querySelector('#checkout').textContent === 'Thử xác nhận lại đơn');
    check(committed.order.total === 45000, 'Server committed before simulated response loss');
    check(await page.locator('#cart-count').textContent() === '1', 'Network error preserves cart');
    check(await page.locator('#clear-cart').isDisabled() && await page.locator('#discount').isDisabled(), 'Uncertain checkout locks mutations');
    check(await page.locator('#checkout').isEnabled(), 'Retry remains available');
    await page.unroute('**/Pos/Checkout', lostResponse);
    await page.locator('#checkout').click();
    await page.locator('#receipt-modal.show').waitFor();
    check((await page.locator('#receipt-meta').textContent()).includes(committed.order.orderNumber), 'Retry retrieves same committed order');
    check((await page.locator('#order-number').textContent()).includes(committed.nextOrderNumber), 'Retry did not consume another number');
    check(await page.locator('#cart-count').textContent() === '0', 'Confirmed retry clears cart');
    await page.locator('#receipt-modal .btn-close').click();
    await page.locator('#receipt-modal').waitFor({ state: 'hidden' });
    const failedSearch = route => route.abort('connectionreset');
    await page.route('**/Pos/Products?*', failedSearch);
    await page.locator('[data-category=traditional]').click();
    await page.waitForFunction(() => document.querySelector('#product-count').textContent.includes('Không tải được'));
    check(await page.locator('#product-grid').getAttribute('aria-busy') === 'false', 'Filter failure exits loading state');
    await page.unroute('**/Pos/Products?*', failedSearch);
    await page.locator('#refresh-pos').click();
    await page.waitForFunction(() => document.querySelector('#product-count').textContent === '2 món');
    check(await page.locator('.product-card').count() === 2, 'Refresh recovers failed filter');
    await page.locator('[data-category=all]').click();
    await page.waitForFunction(() => document.querySelectorAll('.product-card').length >= 12);
    await page.locator('#product-search').fill('tra');
    await page.locator('#product-search').fill('CA PHE SUA DA');
    await page.waitForFunction(() => document.querySelectorAll('.product-card').length === 1);
    check(await page.locator('.product-name').textContent() === 'Cà phê sữa đá', 'Latest rapid search wins');
    const brokenImage = route => route.fulfill({ status: 404, body: '' });
    await page.route('**/images/menu/latte.jpg', brokenImage);
    await page.reload();
    await page.waitForFunction(() => document.querySelector('.product-card[data-product=p1] img').getAttribute('src').includes('product-fallback.svg'));
    check(await page.locator('.product-card[data-product=p1] img').evaluate(img => img.complete && img.naturalWidth > 0), 'Broken image replaced by loaded fallback');
    await page.unroute('**/images/menu/latte.jpg', brokenImage);
    await page.reload();
    return { passed: results.length, results };
}

```

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

    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
});

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IPosService, PosService>();
// Giai đoạn 4: Dùng DB services (EF Core / SQL Server).
// Khi chưa cấu hình connection string, Program.cs ném exception ngay khi AddDbContext truy cập.
// Nếu muốn fallback in-memory, chú thích 2 dòng Db* bên dưới và bỏ chú thích 2 dòng ProductService/OrderService.
builder.Services.AddScoped<IProductService, DbProductService>();
builder.Services.AddScoped<IOrderService, DbOrderService>();
// builder.Services.AddScoped<IProductService, ProductService>();   // in-memory fallback
// builder.Services.AddScoped<IOrderService, OrderService>();       // in-memory fallback
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

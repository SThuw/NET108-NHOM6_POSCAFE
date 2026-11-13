using System.Globalization;
using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Services.Implementations;
using CafePOSManagement.Web.Services.Interfaces;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using CafePOSManagement.Web.DAL;
using Microsoft.AspNetCore.Authentication;

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
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((_, field) => $"Giá trị nhập cho {field} không hợp lệ.");
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Login/Login";
    options.AccessDeniedPath = "/Login/Denied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Events.OnValidatePrincipal = async context =>
    {
        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var account = string.IsNullOrWhiteSpace(id) ? null : UserDAL.GetById(id);
        var expectedRole = account?.Role == true ? "Manager" : "Employee";
        if (account?.Active == true && context.Principal!.IsInRole(expectedRole)) return;
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmployeeOrManager", policy => policy.RequireRole("Employee", "Manager"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
});

builder.Services.AddDbContext<CafePosDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("CafePos");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=CafePOS;Integrated Security=True;TrustServerCertificate=True;";
    }

    CafePOSManagement.Web.Utils.DBUtil.connString = connectionString;
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
app.UseAuthentication();
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
    }
    await next();
});
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pos}/{action=Index}/{id?}");

// Tự động khởi tạo / cập nhật CSDL trên máy mới (giúp máy thầy hoặc máy chấm thi chạy ngay không cần gõ lệnh)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<CafePosDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Lưu ý CSDL] Chưa thể tự động migrate: {ex.Message}");
    }
}

app.Run();

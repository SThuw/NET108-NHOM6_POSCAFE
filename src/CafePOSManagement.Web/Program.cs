using System.Globalization;
using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Services.Implementations;
using CafePOSManagement.Web.Services.Interfaces;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using CafePOSManagement.Web.DAL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

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
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("PasswordReset", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
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
app.UseRateLimiter();
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
        try { db.Database.Migrate(); } catch { db.Database.EnsureCreated(); }

        // Đảm bảo bảng Orders luôn có cột CreatedByUserId kể cả trên DB cũ
        try
        {
            db.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('dbo.Orders', 'CreatedByUserId') IS NULL
BEGIN
    ALTER TABLE [Orders] ADD [CreatedByUserId] varchar(40) NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_CreatedByUserId' AND object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    CREATE INDEX [IX_Orders_CreatedByUserId] ON [Orders] ([CreatedByUserId]);
END");
        }
        catch { }

        // Tự động đồng bộ các Category mới (của Kha: matcha, milktea, combo) nếu DB chưa có
        var existingCatIds = db.Categories.Select(c => c.Id).ToHashSet();
        var newCategories = new[]
        {
            new Category("matcha", "Matcha & Đá xay") { DisplayOrder = 8, IsActive = true, CreatedAt = DateTimeOffset.Now },
            new Category("milktea", "Trà sữa") { DisplayOrder = 9, IsActive = true, CreatedAt = DateTimeOffset.Now },
            new Category("combo", "Combo Tiết Kiệm (Deal Hời)") { DisplayOrder = 10, IsActive = true, CreatedAt = DateTimeOffset.Now }
        };
        foreach (var cat in newCategories)
        {
            if (!existingCatIds.Contains(cat.Id))
                db.Categories.Add(cat);
        }
        db.SaveChanges();

        // Tự động đồng bộ các Product mới (của Kha: p17 - p24) nếu DB chưa có
        var existingProdIds = db.Products.Select(p => p.Id).ToHashSet();
        var newProducts = new[]
        {
            new Product("p17", "Matcha Latte", "matcha", 65000, 59000, "https://images.unsplash.com/photo-1576092762791-dd9e2220e561?auto=format&fit=crop&w=800&q=85") { IsAvailable = true, Description = "Matcha Latte — món đặc sắc của CafePOS." },
            new Product("p18", "Matcha Freeze", "matcha", 75000, 69000, "https://images.unsplash.com/photo-1563805042-7684c019e1cb?auto=format&fit=crop&w=800&q=85") { IsAvailable = true, Description = "Matcha Freeze — món đặc sắc của CafePOS." },
            new Product("p19", "Cacao sữa", "matcha", 60000, 55000, "https://images.unsplash.com/photo-1542990253-a781e04c0082?auto=format&fit=crop&w=800&q=85") { IsAvailable = true, Description = "Cacao sữa — món đặc sắc của CafePOS." },
            new Product("p20", "Trà sữa Oolong nướng", "milktea", 65000, 59000, "https://images.unsplash.com/photo-1558857563-b371033fe7cf?auto=format&fit=crop&w=800&q=85") { IsAvailable = true, Description = "Trà sữa Oolong nướng — món đặc sắc của CafePOS." },
            new Product("p21", "Hồng trà Macchiato", "milktea", 70000, 65000, "https://images.unsplash.com/photo-1576092762791-dd9e2220e561?auto=format&fit=crop&w=800&q=85&sat=-10") { IsAvailable = true, Description = "Hồng trà Macchiato — món đặc sắc của CafePOS." },
            new Product("p22", "Trà sữa lài", "milktea", 60000, 55000, "https://images.unsplash.com/photo-1556679343-c7306c1976bc?auto=format&fit=crop&w=800&q=85&sat=-15") { IsAvailable = true, Description = "Trà sữa lài — món đặc sắc của CafePOS." },
            new Product("p23", "Combo Sáng: Cà phê + Bánh croissant", "combo", 95000, 79000, "https://images.unsplash.com/photo-1498804103079-a6351b050096?auto=format&fit=crop&w=800&q=85") { IsAvailable = true, Description = "Combo Sáng: Cà phê + Bánh croissant — combo Deal Hời." },
            new Product("p24", "Combo Trà chiều: Trà đào + Bánh", "combo", 95000, 79000, "https://images.unsplash.com/photo-1507133750040-4a8f57021571?auto=format&fit=crop&w=800&q=85") { IsAvailable = true, Description = "Combo Trà chiều: Trà đào + Bánh — combo Deal Hời." }
        };
        foreach (var prod in newProducts)
        {
            if (!existingProdIds.Contains(prod.Id))
                db.Products.Add(prod);
        }
        db.SaveChanges();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Lưu ý CSDL] {ex.Message}");
    }
}

app.Run();

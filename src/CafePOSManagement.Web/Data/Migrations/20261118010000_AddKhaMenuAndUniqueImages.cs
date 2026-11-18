using CafePOSManagement.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CafePOSManagement.Web.Data.Migrations;

[DbContext(typeof(CafePosDbContext))]
[Migration("20261118010000_AddKhaMenuAndUniqueImages")]
public sealed partial class AddKhaMenuAndUniqueImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var created = new DateTimeOffset(2026, 11, 7, 8, 0, 0, TimeSpan.FromHours(7));

        migrationBuilder.InsertData("Categories", new[] { "Id", "Code", "CreatedAt", "DisplayOrder", "IsActive", "Name" },
            new object[,]
            {
                { "matcha", "matcha", created, 8, true, "Matcha & Đá xay" },
                { "milktea", "milktea", created, 9, true, "Trà sữa" },
                { "combo", "combo", created, 10, true, "Combo Tiết Kiệm (Deal Hời)" }
            });

        migrationBuilder.InsertData("Products", new[] { "Id", "CategoryId", "CreatedAt", "Description", "ImageUrl", "IsAvailable", "Name", "OriginalPrice", "Price" },
            new object[,]
            {
                { "p17", "matcha", created, "Matcha Latte — món đặc sắc của CafePOS.", "https://images.unsplash.com/photo-1576092762791-dd9e2220e561?auto=format&fit=crop&w=800&q=85", true, "Matcha Latte", 65000m, 59000m },
                { "p18", "matcha", created, "Matcha Freeze — món đặc sắc của CafePOS.", "https://images.unsplash.com/photo-1563805042-7684c019e1cb?auto=format&fit=crop&w=800&q=85", true, "Matcha Freeze", 75000m, 69000m },
                { "p19", "matcha", created, "Cacao sữa — món đặc sắc của CafePOS.", "https://images.unsplash.com/photo-1542990253-a781e04c0082?auto=format&fit=crop&w=800&q=85", true, "Cacao sữa", 60000m, 55000m },
                { "p20", "milktea", created, "Trà sữa Oolong nướng — món đặc sắc của CafePOS.", "https://images.unsplash.com/photo-1558857563-b371033fe7cf?auto=format&fit=crop&w=800&q=85", true, "Trà sữa Oolong nướng", 65000m, 59000m },
                { "p21", "milktea", created, "Hồng trà Macchiato — món đặc sắc của CafePOS.", "https://images.unsplash.com/photo-1576092762791-dd9e2220e561?auto=format&fit=crop&w=800&q=85&sat=-10", true, "Hồng trà Macchiato", 70000m, 65000m },
                { "p22", "milktea", created, "Trà sữa lài — món đặc sắc của CafePOS.", "https://images.unsplash.com/photo-1556679343-c7306c1976bc?auto=format&fit=crop&w=800&q=85&sat=-15", true, "Trà sữa lài", 60000m, 55000m },
                { "p23", "combo", created, "Combo Sáng: Cà phê + Bánh croissant — combo Deal Hời.", "https://images.unsplash.com/photo-1498804103079-a6351b050096?auto=format&fit=crop&w=800&q=85", true, "Combo Sáng: Cà phê + Bánh croissant", 95000m, 79000m },
                { "p24", "combo", created, "Combo Trà chiều: Trà đào + Bánh — combo Deal Hời.", "https://images.unsplash.com/photo-1507133750040-4a8f57021571?auto=format&fit=crop&w=800&q=85", true, "Combo Trà chiều: Trà đào + Bánh", 95000m, 79000m }
            });

        UpdateProductImage(migrationBuilder, "p1", "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p2", "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p3", "https://images.unsplash.com/photo-1555507036-ab1f4038808a?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p4", "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p5", "https://images.unsplash.com/photo-1517701604599-bb29b565090c?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p6", "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p7", "https://images.unsplash.com/photo-1485808191679-5f86510681a2?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p8", "https://images.unsplash.com/photo-1556679343-c7306c1976bc?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p9", "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p10", "https://images.unsplash.com/photo-1497636577773-f1231844b336?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p11", "https://images.unsplash.com/photo-1511081692775-05d0f180a065?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p12", "https://images.unsplash.com/photo-1499636136210-6f4ee915583e?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p13", "https://images.unsplash.com/photo-1534778101976-62847782c213?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p14", "https://images.unsplash.com/photo-1522992319-7b3a8d2a8ca1?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p15", "https://images.unsplash.com/photo-1533777324565-a040eb52facd?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p16", "https://images.unsplash.com/photo-1497935586351-b67a49e012bf?auto=format&fit=crop&w=800&q=85");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData("Products", "Id", new object[] { "p24" });
        migrationBuilder.DeleteData("Products", "Id", new object[] { "p23" });
        migrationBuilder.DeleteData("Products", "Id", new object[] { "p22" });
        migrationBuilder.DeleteData("Products", "Id", new object[] { "p21" });
        migrationBuilder.DeleteData("Products", "Id", new object[] { "p20" });
        migrationBuilder.DeleteData("Products", "Id", new object[] { "p19" });
        migrationBuilder.DeleteData("Products", "Id", new object[] { "p18" });
        migrationBuilder.DeleteData("Products", "Id", new object[] { "p17" });

        migrationBuilder.DeleteData("Categories", "Id", new object[] { "combo" });
        migrationBuilder.DeleteData("Categories", "Id", new object[] { "milktea" });
        migrationBuilder.DeleteData("Categories", "Id", new object[] { "matcha" });

        UpdateProductImage(migrationBuilder, "p1", "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p2", "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p3", "https://images.unsplash.com/photo-1555507036-ab1f4038808a?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p4", "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p5", "https://images.unsplash.com/photo-1517701604599-bb29b565090c?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p6", "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p7", "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p8", "https://images.unsplash.com/photo-1556679343-c7306c1976bc?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p9", "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p10", "https://images.unsplash.com/photo-1544787219-7f47ccb76574?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p11", "https://images.unsplash.com/photo-1517701604599-bb29b565090c?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p12", "https://images.unsplash.com/photo-1544787219-7f47ccb76574?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p13", "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p14", "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p15", "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85");
        UpdateProductImage(migrationBuilder, "p16", "https://images.unsplash.com/photo-1556679343-c7306c1976bc?auto=format&fit=crop&w=800&q=85");
    }

    private static void UpdateProductImage(MigrationBuilder migrationBuilder, string id, string imageUrl)
        => migrationBuilder.UpdateData("Products", "Id", new object[] { id }, "ImageUrl", new object[] { imageUrl });
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace CafePOSManagement.Web.Data.Migrations;

// Model đích (BuildTargetModel) và các attribute [DbContext]/[Migration] nằm trong file .Designer.cs.
public sealed partial class AddKhaMenuAndUniqueImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Ba danh mục Non-Coffee (chèn an toàn IF NOT EXISTS để không lỗi trên DB đã có sẵn).
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE [Id] = 'matcha')
    INSERT INTO [Categories] ([Id], [Code], [CreatedAt], [DisplayOrder], [IsActive], [Name]) VALUES ('matcha', 'matcha', '2026-11-07T08:00:00.0000000+07:00', 8, 1, N'Matcha & Đá xay');
IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE [Id] = 'milktea')
    INSERT INTO [Categories] ([Id], [Code], [CreatedAt], [DisplayOrder], [IsActive], [Name]) VALUES ('milktea', 'milktea', '2026-11-07T08:00:00.0000000+07:00', 9, 1, N'Trà sữa');
IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE [Id] = 'combo')
    INSERT INTO [Categories] ([Id], [Code], [CreatedAt], [DisplayOrder], [IsActive], [Name]) VALUES ('combo', 'combo', '2026-11-07T08:00:00.0000000+07:00', 10, 1, N'Combo Tiết Kiệm (Deal Hời)');
");

        // 2. Tám món Non-Coffee (chèn an toàn IF NOT EXISTS).
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = 'p17')
    INSERT INTO [Products] ([Id], [CategoryId], [CreatedAt], [Description], [ImageUrl], [IsAvailable], [Name], [OriginalPrice], [Price]) VALUES ('p17', 'matcha', '2026-11-07T08:00:00.0000000+07:00', N'Matcha Latte — món đặc sắc của CafePOS.', N'https://images.unsplash.com/photo-1536256263959-770b48d82b0a?auto=format&fit=crop&w=800&q=85', 1, N'Matcha Latte', 65000.0, 55000.0);
IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = 'p18')
    INSERT INTO [Products] ([Id], [CategoryId], [CreatedAt], [Description], [ImageUrl], [IsAvailable], [Name], [OriginalPrice], [Price]) VALUES ('p18', 'matcha', '2026-11-07T08:00:00.0000000+07:00', N'Matcha Freeze — món đặc sắc của CafePOS.', N'https://images.unsplash.com/photo-1515823064-d6e0c04616a7?auto=format&fit=crop&w=800&q=85', 1, N'Matcha Freeze', 69000.0, 59000.0);
IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = 'p19')
    INSERT INTO [Products] ([Id], [CategoryId], [CreatedAt], [Description], [ImageUrl], [IsAvailable], [Name], [OriginalPrice], [Price]) VALUES ('p19', 'matcha', '2026-11-07T08:00:00.0000000+07:00', N'Cacao sữa — món đặc sắc của CafePOS.', N'https://images.unsplash.com/photo-1542990253-0d0f5be5f0ed?auto=format&fit=crop&w=800&q=85', 1, N'Cacao sữa', NULL, 49000.0);
IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = 'p20')
    INSERT INTO [Products] ([Id], [CategoryId], [CreatedAt], [Description], [ImageUrl], [IsAvailable], [Name], [OriginalPrice], [Price]) VALUES ('p20', 'milktea', '2026-11-07T08:00:00.0000000+07:00', N'Trà sữa Oolong nướng — món đặc sắc của CafePOS.', N'https://images.unsplash.com/photo-1525385133512-2f3bdd039054?auto=format&fit=crop&w=800&q=85', 1, N'Trà sữa Oolong nướng', 55000.0, 48000.0);
IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = 'p21')
    INSERT INTO [Products] ([Id], [CategoryId], [CreatedAt], [Description], [ImageUrl], [IsAvailable], [Name], [OriginalPrice], [Price]) VALUES ('p21', 'milktea', '2026-11-07T08:00:00.0000000+07:00', N'Hồng trà Macchiato — món đặc sắc của CafePOS.', N'https://images.unsplash.com/photo-1576092768241-dec231879fc3?auto=format&fit=crop&w=800&q=85', 1, N'Hồng trà Macchiato', 52000.0, 45000.0);
IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = 'p22')
    INSERT INTO [Products] ([Id], [CategoryId], [CreatedAt], [Description], [ImageUrl], [IsAvailable], [Name], [OriginalPrice], [Price]) VALUES ('p22', 'milktea', '2026-11-07T08:00:00.0000000+07:00', N'Trà sữa lài — món đặc sắc của CafePOS.', N'https://images.unsplash.com/photo-1579954115545-a95591f28bfc?auto=format&fit=crop&w=800&q=85', 1, N'Trà sữa lài', NULL, 42000.0);
IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = 'p23')
    INSERT INTO [Products] ([Id], [CategoryId], [CreatedAt], [Description], [ImageUrl], [IsAvailable], [Name], [OriginalPrice], [Price]) VALUES ('p23', 'combo', '2026-11-07T08:00:00.0000000+07:00', N'Combo Sáng: Cà phê + Bánh croissant — món đặc sắc của CafePOS.', N'https://images.unsplash.com/photo-1525610553991-2bede1a236e2?auto=format&fit=crop&w=800&q=85', 1, N'Combo Sáng: Cà phê + Bánh croissant', 95000.0, 75000.0);
IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = 'p24')
    INSERT INTO [Products] ([Id], [CategoryId], [CreatedAt], [Description], [ImageUrl], [IsAvailable], [Name], [OriginalPrice], [Price]) VALUES ('p24', 'combo', '2026-11-07T08:00:00.0000000+07:00', N'Combo Trà chiều: Trà đào + Bánh — món đặc sắc của CafePOS.', N'https://images.unsplash.com/photo-1509440159596-0249088772ff?auto=format&fit=crop&w=800&q=85', 1, N'Combo Trà chiều: Trà đào + Bánh', 85000.0, 69000.0);
");

        // 3. Gán lại ảnh cho 16 món cũ: trước đây nhiều món dùng chung một ảnh,
        //    nay mỗi món một photo-id riêng để cả 24 món không trùng ảnh.
        SetProduct(migrationBuilder, "p1", "Cà phê sữa đá", "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p2", "Cappuccino", "https://images.unsplash.com/photo-1572442388796-11668a67e53d?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p3", "Bánh Croissant bơ", "https://images.unsplash.com/photo-1555507036-ab1f4038808a?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p4", "Americano nóng", "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p5", "Bạc xỉu đá", "https://images.unsplash.com/photo-1517701604599-bb29b565090c?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p6", "Sandwich nướng", "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p7", "Cold Brew sữa tươi", "https://images.unsplash.com/photo-1517256064527-09c73fc73e38?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p8", "Trà chanh thanh mát", "https://images.unsplash.com/photo-1556679343-c7306c1976bc?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p9", "Latte kem sữa", "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p10", "Cà phê sữa mùa đông", "https://images.unsplash.com/photo-1512568400610-62da28bc8a13?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p11", "Cold Brew đá", "https://images.unsplash.com/photo-1517701550927-30cf4ba1dba5?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p12", "Bánh quy bơ", "https://images.unsplash.com/photo-1499636136210-6f4ee915583e?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p13", "Cà phê bọt biển Whipped Coffee", "https://images.unsplash.com/photo-1589396575653-c09c794ff6a6?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p14", "Cà phê phin Filter Coffee", "https://images.unsplash.com/photo-1511920170033-f8396924c348?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p15", "Latte nghệ thuật", "https://images.unsplash.com/photo-1534778101976-62847782c213?auto=format&fit=crop&w=800&q=85");
        SetProduct(migrationBuilder, "p16", "Cold Brew cam sả", "https://images.unsplash.com/photo-1544787219-7f47ccb76574?auto=format&fit=crop&w=800&q=85");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData("Products", "Id", "p24");
        migrationBuilder.DeleteData("Products", "Id", "p23");
        migrationBuilder.DeleteData("Products", "Id", "p22");
        migrationBuilder.DeleteData("Products", "Id", "p21");
        migrationBuilder.DeleteData("Products", "Id", "p20");
        migrationBuilder.DeleteData("Products", "Id", "p19");
        migrationBuilder.DeleteData("Products", "Id", "p18");
        migrationBuilder.DeleteData("Products", "Id", "p17");

        migrationBuilder.DeleteData("Categories", "Id", "combo");
        migrationBuilder.DeleteData("Categories", "Id", "milktea");
        migrationBuilder.DeleteData("Categories", "Id", "matcha");

        RevertProduct(migrationBuilder, "p1", "Cà phê sữa đá", "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p2", "Cappuccino", "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p3", "Bánh Croissant bơ", "https://images.unsplash.com/photo-1555507036-ab1f4038808a?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p4", "Americano nóng", "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p5", "Bạc xỉu đá", "https://images.unsplash.com/photo-1517701604599-bb29b565090c?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p6", "Sandwich nướng", "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p7", "Cold Brew sữa tươi", "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p8", "Trà chanh thanh mát", "https://images.unsplash.com/photo-1556679343-c7306c1976bc?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p9", "Latte kem sữa", "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p10", "Cà phê sữa mùa đông", "https://images.unsplash.com/photo-1544787219-7f47ccb76574?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p11", "Cold Brew đá", "https://images.unsplash.com/photo-1517701604599-bb29b565090c?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p12", "Bánh quy bơ", "https://images.unsplash.com/photo-1544787219-7f47ccb76574?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p13", "Cà phê bọt biển Whipped Coffee", "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p14", "Cà phê phin Filter Coffee", "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p15", "Latte nghệ thuật", "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85");
        RevertProduct(migrationBuilder, "p16", "Cold Brew cam sả", "https://images.unsplash.com/photo-1556679343-c7306c1976bc?auto=format&fit=crop&w=800&q=85");
    }

    private static void SetProduct(MigrationBuilder migrationBuilder, string id, string name, string imageUrl)
    {
        migrationBuilder.UpdateData("Products", "Id", id, "ImageUrl", imageUrl);
        migrationBuilder.UpdateData("Products", "Id", id, "Description", $"{name} — món đặc sắc của CafePOS.");
    }

    private static void RevertProduct(MigrationBuilder migrationBuilder, string id, string name, string imageUrl)
    {
        migrationBuilder.UpdateData("Products", "Id", id, "ImageUrl", imageUrl);
        migrationBuilder.UpdateData("Products", "Id", id, "Description", $"{name} — món mẫu của CafePOS.");
    }
}

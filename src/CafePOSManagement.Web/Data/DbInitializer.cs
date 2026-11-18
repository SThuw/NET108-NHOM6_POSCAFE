using CafePOSManagement.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafePOSManagement.Web.Data;

public static class DbInitializer
{
    // Dữ liệu cố định để migration không thay đổi mỗi lần build. Chỉ nạp qua migration.
    public static void ConfigureSeedData(ModelBuilder modelBuilder)
    {
        var created = new DateTimeOffset(2026, 11, 7, 8, 0, 0, TimeSpan.FromHours(7));
        var categories = new[]
        {
            new Category("traditional", "Cà phê truyền thống"), new Category("snack", "Đồ ăn nhẹ"),
            new Category("sandwich", "Bánh mì & Sandwich"), new Category("coldbrew", "Cold Brew đặc biệt"),
            new Category("espresso", "Cà phê Ý (Espresso)"), new Category("seasonal", "Đồ uống theo mùa"),
            new Category("tea", "Trà trái cây")
        };
        modelBuilder.Entity<Category>().HasData(categories.Select((c, i) => new
        { c.Id, c.Name, c.Code, DisplayOrder = i + 1, IsActive = true, CreatedAt = created }));

        const string coffee = "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85";
        const string latte = "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85";
        const string iced = "https://images.unsplash.com/photo-1517701604599-bb29b565090c?auto=format&fit=crop&w=800&q=85";
        const string espresso = "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=800&q=85";
        const string croissant = "https://images.unsplash.com/photo-1555507036-ab1f4038808a?auto=format&fit=crop&w=800&q=85";
        const string sandwich = "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?auto=format&fit=crop&w=800&q=85";
        const string tea = "https://images.unsplash.com/photo-1544787219-7f47ccb76574?auto=format&fit=crop&w=800&q=85";
        const string fruitTea = "https://images.unsplash.com/photo-1556679343-c7306c1976bc?auto=format&fit=crop&w=800&q=85";
        Product[] products =
        [
            new("p1", "Cà phê sữa đá", "traditional", 55000, 45000, latte),
            new("p2", "Cappuccino", "espresso", 65000, 55000, coffee),
            new("p3", "Bánh Croissant bơ", "snack", 50000, 45000, croissant),
            new("p4", "Americano nóng", "espresso", 45000, 40000, espresso),
            new("p5", "Bạc xỉu đá", "traditional", null, 39000, iced),
            new("p6", "Sandwich nướng", "sandwich", 75000, 65000, sandwich),
            new("p7", "Cold Brew sữa tươi", "coldbrew", 60000, 55000, latte),
            new("p8", "Trà chanh thanh mát", "tea", null, 45000, fruitTea),
            new("p9", "Latte kem sữa", "espresso", 65000, 59000, coffee),
            new("p10", "Cà phê sữa mùa đông", "seasonal", null, 49000, tea),
            new("p11", "Cold Brew đá", "coldbrew", 55000, 49000, iced),
            new("p12", "Bánh quy bơ", "snack", 35000, 29000, tea),
            new("p13", "Cà phê bọt biển Whipped Coffee", "seasonal", 65000, 45000, latte),
            new("p14", "Cà phê phin Filter Coffee", "traditional", null, 22000, espresso),
            new("p15", "Latte nghệ thuật", "espresso", null, 60000, coffee),
            new("p16", "Cold Brew cam sả", "coldbrew", null, 45000, fruitTea)
        ];
        modelBuilder.Entity<Product>().HasData(products.Select(p => new
        {
            p.Id, p.Name, p.CategoryId, p.OriginalPrice, p.Price, p.ImageUrl, IsAvailable = true,
            Description = $"{p.Name} — món mẫu của CafePOS.", CreatedAt = created
        }));
        modelBuilder.Entity<Discount>().HasData(
            new Discount { Id = 1, Code = "NONE", Name = "Không giảm", DiscountPercent = 0 },
            new Discount { Id = 2, Code = "SALE10", Name = "Giảm 10%", DiscountPercent = 10 },
            new Discount { Id = 3, Code = "THANK15", Name = "Giảm 15% tri ân", DiscountPercent = 15 },
            new Discount { Id = 4, Code = "SALE20", Name = "Giảm 20% Khách VIP", DiscountPercent = 20 });

        modelBuilder.Entity<Order>().HasData(
            new { Id = 1L, RequestId = Guid.Parse("00000000-0000-4000-8000-000000000001"), OrderNumber = "#001", OrderType = "pickup", Subtotal = 45000m, DiscountId = (int?)null, DiscountPercent = 0m, DiscountAmount = 0m, TaxPercent = 0m, TaxAmount = 0m, Total = 45000m, Note = "Đơn mẫu — ít đá", PaymentMethod = "cash", Status = "Hoàn thành", CreatedAt = created.AddMinutes(10) },
            new { Id = 2L, RequestId = Guid.Parse("00000000-0000-4000-8000-000000000002"), OrderNumber = "#002", OrderType = "dinein", Subtotal = 100000m, DiscountId = (int?)2, DiscountPercent = 10m, DiscountAmount = 10000m, TaxPercent = 8m, TaxAmount = 7200m, Total = 97200m, Note = "Đơn mẫu — dùng tại bàn", PaymentMethod = "card", Status = "Hoàn thành", CreatedAt = created.AddMinutes(20) },
            new { Id = 3L, RequestId = Guid.Parse("00000000-0000-4000-8000-000000000003"), OrderNumber = "#003", OrderType = "delivery", Subtotal = 135000m, DiscountId = (int?)3, DiscountPercent = 15m, DiscountAmount = 20250m, TaxPercent = 10m, TaxAmount = 11475m, Total = 126225m, Note = "Đơn mẫu — giao hàng", PaymentMethod = "wallet", Status = "Hoàn thành", CreatedAt = created.AddMinutes(30) });

        // Không seed LineTotal vì SQL Server tự tính và lưu computed column.
        modelBuilder.Entity<OrderItem>().HasData(
            new { Id = 1L, OrderId = 1L, ProductId = "p1", ProductName = "Cà phê sữa đá", UnitPrice = 45000m, Quantity = 1 },
            new { Id = 2L, OrderId = 2L, ProductId = "p2", ProductName = "Cappuccino", UnitPrice = 55000m, Quantity = 1 },
            new { Id = 3L, OrderId = 2L, ProductId = "p3", ProductName = "Bánh Croissant bơ", UnitPrice = 45000m, Quantity = 1 },
            new { Id = 4L, OrderId = 3L, ProductId = "p13", ProductName = "Cà phê bọt biển Whipped Coffee", UnitPrice = 45000m, Quantity = 2 },
            new { Id = 5L, OrderId = 3L, ProductId = "p16", ProductName = "Cold Brew cam sả", UnitPrice = 45000m, Quantity = 1 });

        // Dữ liệu mẫu bảng Users (Nhóm 6 PolyCafe) - Phân hệ của Tâm
        modelBuilder.Entity<User>().HasData(
            new User("u1", "thu@polycafe.vn", "123456", "Nguyễn Kim Song Thư", "0901234567", true, true) { CreatedAt = created },
            new User("u2", "kha@polycafe.vn", "123456", "Trần Thị Tuyết Kha", "0902345678", false, true) { CreatedAt = created },
            new User("u3", "nguyen@polycafe.vn", "123456", "Nguyễn Trí Nguyễn", "0903456789", false, true) { CreatedAt = created },
            new User("u4", "duc@polycafe.vn", "123456", "Ninh Văn Đức", "0904567890", false, true) { CreatedAt = created },
            new User("u5", "tam@polycafe.vn", "123456", "Nguyễn Lâm Chí Tâm", "0905678901", true, true) { CreatedAt = created });

        // Dữ liệu mẫu bảng Cards (Thẻ bàn / Thẻ rung) - Phân hệ của Kha
        modelBuilder.Entity<Card>().HasData(
            new Card("c1", "TB01", "Thẻ Bàn 01", false, true) { CreatedAt = created },
            new Card("c2", "TB02", "Thẻ Bàn 02", true, true) { CreatedAt = created },
            new Card("c3", "TB03", "Thẻ Bàn 03", false, true) { CreatedAt = created },
            new Card("c4", "TR01", "Thẻ Rung 01", false, true) { CreatedAt = created },
            new Card("c5", "TR02", "Thẻ Rung 02", true, true) { CreatedAt = created });

        // Dữ liệu mẫu Trợ lý AI và Bộ nhớ trạng thái (AiStateStore)
        modelBuilder.Entity<AiSession>().HasData(
            new { Id = "sess_01", UserId = "u1", Title = "Gợi ý combo cà phê sáng cho khách", Context = "PolyCafe AI Assistant - Trợ lý quầy POS", IsActive = true, CreatedAt = created },
            new { Id = "sess_02", UserId = "u4", Title = "Tư vấn món trà thanh nhiệt buổi chiều", Context = "PolyCafe AI Assistant - Tư vấn thực đơn", IsActive = true, CreatedAt = created });

        modelBuilder.Entity<AiMessage>().HasData(
            new { Id = 1L, SessionId = "sess_01", Role = "user", Content = "Khách muốn đồ uống đậm vị cà phê và bánh ăn kèm bữa sáng", CreatedAt = created },
            new { Id = 2L, SessionId = "sess_01", Role = "assistant", Content = "Gợi ý combo: Cà phê sữa đá (p1) + Bánh Croissant bơ (p3), áp dụng mã SALE10 giảm 10%.", CreatedAt = created },
            new { Id = 3L, SessionId = "sess_02", Role = "user", Content = "Khách nữ thích uống ngọt dịu, thanh mát ít đá", CreatedAt = created },
            new { Id = 4L, SessionId = "sess_02", Role = "assistant", Content = "Gợi ý: Trà chanh thanh mát (p8) hoặc Cold Brew cam sả (p16) 50% đường.", CreatedAt = created });

        modelBuilder.Entity<AiStateStore>().HasData(
            new { Id = 1L, SessionId = "sess_01", StateKey = "intent", StateValue = "recommend_combo", UpdatedAt = created },
            new { Id = 2L, SessionId = "sess_01", StateKey = "suggested_items", StateValue = "[\"p1\", \"p3\"]", UpdatedAt = created },
            new { Id = 3L, SessionId = "sess_02", StateKey = "intent", StateValue = "recommend_drink", UpdatedAt = created },
            new { Id = 4L, SessionId = "sess_02", StateKey = "taste_preference", StateValue = "less_sweet,fruity", UpdatedAt = created });
    }
}

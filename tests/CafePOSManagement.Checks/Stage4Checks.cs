using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

internal static class Stage4Checks
{
    public static async Task RunAsync()
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__CafePos");
        if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("Thiếu ConnectionStrings__CafePos cho kiểm tra SQL Server.");
        using var db = new CafePosDbContext(new DbContextOptionsBuilder<CafePosDbContext>().UseSqlServer(connection).Options);
        var count = 0;
        void Check(bool condition, string name)
        {
            if (!condition) throw new Exception("FAIL SQL: " + name);
            Console.WriteLine("PASS SQL: " + name); count++;
        }
        var model = db.GetService<IDesignTimeModel>().Model;
        Check(model.GetEntityTypes().Count() == 5, "Exactly five business tables");
        foreach (var (type, property) in new[] {
            (typeof(Product), "Price"), (typeof(Product), "OriginalPrice"), (typeof(Order), "Subtotal"),
            (typeof(Order), "DiscountAmount"), (typeof(Order), "TaxAmount"), (typeof(Order), "Total"),
            (typeof(OrderItem), "UnitPrice"), (typeof(OrderItem), "LineTotal") })
        {
            var field = model.FindEntityType(type)!.FindProperty(property)!;
            Check(field.GetPrecision() == 18 && field.GetScale() == 0, type.Name + "." + property + " is decimal(18,0)");
        }
        Check((await db.Database.GetPendingMigrationsAsync()).Count() == 0, "Database has no pending migrations");
        Check(await db.Categories.CountAsync() == 7, "Seven seeded categories");
        Check(await db.Products.CountAsync() == 16, "Sixteen seeded products");
        Check(await db.Discounts.CountAsync() == 4, "Four seeded discounts");
        Check(await db.Orders.CountAsync() == 3 && await db.OrderItems.CountAsync() == 5, "Three seeded orders and five lines");
        var categories = await db.Categories.Include(c => c.Products).OrderBy(c => c.DisplayOrder).ToListAsync();
        Check(categories[0].Name == "Cà phê truyền thống" && categories.All(c => c.Products.Count > 0), "Vietnamese labels and category navigation materialize");
        var orders = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Product).Include(o => o.Discount).OrderBy(o => o.Id).ToListAsync();
        Check(orders.Select(o => o.OrderNumber).SequenceEqual(new[] { "#001", "#002", "#003" }), "Seeded order numbers");
        Check(orders[1].OrderType == "dinein" && orders[1].PaymentMethod == "card", "Vietnamese storage converted to existing API codes");
        Check(orders.All(o => o.Subtotal == o.Items.Sum(i => i.LineTotal)), "Order subtotals match snapshots");
        Check(orders.All(o => o.Total == o.Subtotal - o.DiscountAmount + o.TaxAmount), "Order totals are consistent");
        Check(orders[2].Total == 126225 && orders[2].Discount!.DiscountPercent == 15, "15 percent seeded promotion with VAT");
        Check(await db.Products.CountAsync(p => p.Name.Contains("CA PHE")) > 0, "SQL collation supports case and accent insensitive search");
        Check(await db.Products.CountAsync(p => p.ImageUrl.StartsWith("https://images.unsplash.com/")) == 16, "All seed images use HTTPS Unsplash");

        // Tất cả thử nghiệm ghi nằm trong một transaction và được rollback, không sửa dữ liệu mẫu.
        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("SET XACT_ABORT OFF;");
        async Task Reject(string sql, string name)
        {
            try { await db.Database.ExecuteSqlRawAsync(sql); }
            catch (SqlException ex) when (ex.Number is 547 or 2601 or 2627)
            { Check(true, name); return; }
            throw new Exception("FAIL SQL: accepted invalid write: " + name);
        }
        try
        {
            await Reject("DELETE FROM Categories WHERE Id = 'traditional'", "Category deletion restricted by products");
            await Reject("DELETE FROM Products WHERE Id = 'p1'", "Sold product deletion restricted by history");
            await Reject("DELETE FROM Discounts WHERE Id = 2", "Used discount deletion restricted");
            await Reject("UPDATE Products SET CategoryId = 'missing' WHERE Id = 'p1'", "Orphan product rejected");
            await Reject("UPDATE Products SET Price = -1 WHERE Id = 'p1'", "Negative price rejected");
            await Reject("UPDATE Products SET OriginalPrice = 1 WHERE Id = 'p1'", "Original price below selling price rejected");
            await Reject("UPDATE Products SET Name = N'ca phe sua da' WHERE Id = 'p5'", "Duplicate normalized product name rejected");
            await Reject("UPDATE Discounts SET DiscountPercent = 101 WHERE Id = 1", "Discount outside range rejected");
            await Reject("UPDATE Orders SET OrderNumber = '#001' WHERE Id = 2", "Duplicate order number rejected");
            await Reject("UPDATE Orders SET RequestId = '00000000-0000-4000-8000-000000000001' WHERE Id = 2", "Duplicate request ID rejected");
            await Reject("UPDATE Orders SET TotalAmount = 1 WHERE Id = 1", "Inconsistent total rejected");
            await Reject("UPDATE Orders SET Status = N'Không hợp lệ' WHERE Id = 1", "Unknown status rejected");
            await Reject("UPDATE Orders SET OrderType = N'Không hợp lệ' WHERE Id = 1", "Unknown order type rejected");
            await Reject("UPDATE Orders SET PaymentMethod = N'Không hợp lệ' WHERE Id = 1", "Unknown payment method rejected");
            await Reject("UPDATE OrderItems SET Quantity = 0 WHERE Id = 1", "Zero quantity rejected");
            await Reject("UPDATE OrderItems SET Quantity = 100 WHERE Id = 1", "Excess quantity rejected");
            await db.Database.ExecuteSqlRawAsync("UPDATE Products SET Name = N'Tên thay đổi thử nghiệm', Price = 40000 WHERE Id = 'p1'");
            var snapshot = await db.OrderItems.AsNoTracking().SingleAsync(i => i.Id == 1);
            Check(snapshot.ProductName == "Cà phê sữa đá" && snapshot.UnitPrice == 45000, "Historical name and price unchanged by menu updates");
            await db.Database.ExecuteSqlRawAsync("UPDATE OrderItems SET Quantity = 2 WHERE Id = 1");
            Check(await db.OrderItems.Where(i => i.Id == 1).Select(i => i.LineTotal).SingleAsync() == 90000, "Computed LineTotal maintained by SQL");
            await db.Database.ExecuteSqlRawAsync("DELETE FROM Orders WHERE Id = 2");
            Check(await db.OrderItems.CountAsync(i => i.OrderId == 2) == 0, "Order deletion cascades to its lines");
        }
        finally { await transaction.RollbackAsync(); }
        Check(await db.Orders.CountAsync() == 3 && await db.OrderItems.CountAsync() == 5 && await db.Products.Where(p => p.Id == "p1").Select(p => p.Price).SingleAsync() == 45000, "All write checks rolled back");
        Console.WriteLine($"ALL {count} SQL CHECKS PASSED");
    }
}

using CafePOSManagement.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafePOSManagement.Web.Data;

public sealed class CafePosDbContext(DbContextOptions<CafePosDbContext> options)
    : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mặc định sắp xếp tiếng Việt; cột tên dùng Latin1 CI_AI để coi ê/e và đ/d là tương đương.
        modelBuilder.UseCollation("Vietnamese_100_CI_AI");
        modelBuilder.Ignore<DiscountCode>(); // Hợp đồng dữ liệu in-memory của giai đoạn 3.

        var category = modelBuilder.Entity<Category>();
        category.ToTable("Categories", table => table.HasCheckConstraint("CK_Categories_DisplayOrder", "[DisplayOrder] >= 0"));
        category.HasKey(x => x.Id);
        category.Property(x => x.Id).HasMaxLength(40).IsUnicode(false).ValueGeneratedNever();
        category.Property(x => x.Name).HasMaxLength(100).UseCollation("Latin1_General_100_CI_AI").IsRequired();
        category.Property(x => x.Code).HasMaxLength(40).IsUnicode(false).IsRequired();
        category.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");
        category.HasIndex(x => x.Code).IsUnique();
        category.HasIndex(x => x.Name).IsUnique();

        var product = modelBuilder.Entity<Product>();
        product.ToTable("Products", table =>
        {
            table.HasCheckConstraint("CK_Products_Price", "[Price] > 0 AND [Price] <= 10000000");
            table.HasCheckConstraint("CK_Products_OriginalPrice", "[OriginalPrice] IS NULL OR ([OriginalPrice] >= [Price] AND [OriginalPrice] <= 10000000)");
        });
        product.HasKey(x => x.Id);
        product.Property(x => x.Id).HasMaxLength(40).IsUnicode(false).ValueGeneratedNever();
        product.Property(x => x.CategoryId).HasMaxLength(40).IsUnicode(false);
        product.Property(x => x.Name).HasMaxLength(150).UseCollation("Latin1_General_100_CI_AI").IsRequired();
        product.Property(x => x.Price).HasPrecision(18, 0);
        product.Property(x => x.OriginalPrice).HasPrecision(18, 0);
        product.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
        product.Property(x => x.Description).HasMaxLength(1000);
        product.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");
        product.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        product.HasIndex(x => x.Name);
        product.HasIndex(x => x.CategoryId);
        product.HasIndex(x => new { x.CategoryId, x.Name }).IsUnique();

        var discount = modelBuilder.Entity<Discount>();
        discount.ToTable("Discounts", table => table.HasCheckConstraint("CK_Discounts_Percent", "[DiscountPercent] >= 0 AND [DiscountPercent] <= 100"));
        discount.HasKey(x => x.Id);
        discount.Property(x => x.Code).HasMaxLength(50).IsRequired();
        discount.Property(x => x.Name).HasMaxLength(100).IsRequired();
        discount.Property(x => x.DiscountPercent).HasPrecision(5, 2);
        discount.HasIndex(x => x.Code).IsUnique();

        var order = modelBuilder.Entity<Order>();
        order.ToTable("Orders", table =>
        {
            table.HasCheckConstraint("CK_Orders_Percent", "[DiscountPercent] BETWEEN 0 AND 100 AND [TaxPercent] BETWEEN 0 AND 100");
            table.HasCheckConstraint("CK_Orders_Amounts", "[Subtotal] >= 0 AND [DiscountAmount] >= 0 AND [DiscountAmount] <= [Subtotal] AND [TaxAmount] >= 0 AND [TotalAmount] = [Subtotal] - [DiscountAmount] + [TaxAmount]");
            table.HasCheckConstraint("CK_Orders_Status", "[Status] IN (N'Hoàn thành', N'Đã hủy')");
            table.HasCheckConstraint("CK_Orders_OrderType", "[OrderType] IN (N'Mang đi', N'Giao hàng', N'Tại bàn')");
            table.HasCheckConstraint("CK_Orders_PaymentMethod", "[PaymentMethod] IN (N'Tiền mặt', N'Thẻ ngân hàng', N'Ví điện tử')");
            table.HasCheckConstraint("CK_Orders_RequestId", "[RequestId] <> '00000000-0000-0000-0000-000000000000'");
        });
        order.HasKey(x => x.Id);
        order.Property(x => x.OrderNumber).HasMaxLength(20).IsUnicode(false).IsRequired();
        // API cũ giữ mã kỹ thuật; SQL lưu nhãn tiếng Việt đúng đặc tả.
        order.Property(x => x.OrderType).HasMaxLength(50).HasConversion(v => ToOrderType(v), v => FromOrderType(v));
        order.Property(x => x.PaymentMethod).HasMaxLength(50).HasConversion(v => ToPaymentMethod(v), v => FromPaymentMethod(v));
        order.Property(x => x.Status).HasMaxLength(30).IsRequired();
        order.Property(x => x.Note).HasMaxLength(500);
        order.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");
        order.Property(x => x.DiscountPercent).HasPrecision(5, 2);
        order.Property(x => x.TaxPercent).HasPrecision(5, 2);
        order.Property(x => x.Subtotal).HasPrecision(18, 0);
        order.Property(x => x.DiscountAmount).HasPrecision(18, 0);
        order.Property(x => x.TaxAmount).HasPrecision(18, 0);
        order.Property(x => x.Total).HasColumnName("TotalAmount").HasPrecision(18, 0);
        order.HasOne(x => x.Discount).WithMany(x => x.Orders).HasForeignKey(x => x.DiscountId).OnDelete(DeleteBehavior.Restrict);
        order.HasIndex(x => x.OrderNumber).IsUnique();
        order.HasIndex(x => x.RequestId).IsUnique();
        order.HasIndex(x => x.CreatedAt);

        var item = modelBuilder.Entity<OrderItem>();
        item.ToTable("OrderItems", table =>
        {
            table.HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] BETWEEN 1 AND 99");
            table.HasCheckConstraint("CK_OrderItems_UnitPrice", "[UnitPrice] > 0 AND [UnitPrice] <= 10000000");
        });
        item.HasKey(x => x.Id);
        item.Property(x => x.ProductId).HasMaxLength(40).IsUnicode(false);
        item.Property(x => x.ProductName).HasMaxLength(150).IsRequired();
        item.Property(x => x.UnitPrice).HasPrecision(18, 0);
        item.Property(x => x.LineTotal).HasPrecision(18, 0).HasComputedColumnSql("CONVERT(decimal(18,0), [UnitPrice] * [Quantity])", stored: true);
        item.HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        item.HasOne(x => x.Product).WithMany(x => x.OrderItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        item.HasIndex(x => new { x.OrderId, x.ProductId }).IsUnique();

        DbInitializer.ConfigureSeedData(modelBuilder);
    }

    private static string ToOrderType(string value) => value switch
    { "pickup" => "Mang đi", "delivery" => "Giao hàng", "dinein" => "Tại bàn", _ => throw new ArgumentException("Loại đơn không hợp lệ.") };
    private static string FromOrderType(string value) => value switch
    { "Mang đi" => "pickup", "Giao hàng" => "delivery", "Tại bàn" => "dinein", _ => throw new ArgumentException("Loại đơn trong CSDL không hợp lệ.") };
    private static string ToPaymentMethod(string value) => value switch
    { "cash" => "Tiền mặt", "card" => "Thẻ ngân hàng", "wallet" => "Ví điện tử", _ => throw new ArgumentException("Phương thức thanh toán không hợp lệ.") };
    private static string FromPaymentMethod(string value) => value switch
    { "Tiền mặt" => "cash", "Thẻ ngân hàng" => "card", "Ví điện tử" => "wallet", _ => throw new ArgumentException("Phương thức thanh toán trong CSDL không hợp lệ.") };
}

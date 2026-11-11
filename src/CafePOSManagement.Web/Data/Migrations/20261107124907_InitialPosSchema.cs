using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CafePOSManagement.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPosSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.CheckConstraint("CK_Categories_DisplayOrder", "[DisplayOrder] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.Id);
                    table.CheckConstraint("CK_Discounts_Percent", "[DiscountPercent] >= 0 AND [DiscountPercent] <= 100");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CategoryId = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.CheckConstraint("CK_Products_OriginalPrice", "[OriginalPrice] IS NULL OR ([OriginalPrice] >= [Price] AND [OriginalPrice] <= 10000000)");
                    table.CheckConstraint("CK_Products_Price", "[Price] > 0 AND [Price] <= 10000000");
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    OrderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DiscountId = table.Column<int>(type: "int", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TaxPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    Subtotal = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.CheckConstraint("CK_Orders_Amounts", "[Subtotal] >= 0 AND [DiscountAmount] >= 0 AND [DiscountAmount] <= [Subtotal] AND [TaxAmount] >= 0 AND [TotalAmount] = [Subtotal] - [DiscountAmount] + [TaxAmount]");
                    table.CheckConstraint("CK_Orders_OrderType", "[OrderType] IN (N'Mang đi', N'Giao hàng', N'Tại bàn')");
                    table.CheckConstraint("CK_Orders_PaymentMethod", "[PaymentMethod] IN (N'Tiền mặt', N'Thẻ ngân hàng', N'Ví điện tử')");
                    table.CheckConstraint("CK_Orders_Percent", "[DiscountPercent] BETWEEN 0 AND 100 AND [TaxPercent] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_Orders_RequestId", "[RequestId] <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Orders_Status", "[Status] IN (N'Hoàn thành', N'Đã hủy')");
                    table.ForeignKey(
                        name: "FK_Orders_Discounts_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "Discounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false, computedColumnSql: "CONVERT(decimal(18,0), [UnitPrice] * [Quantity])", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.CheckConstraint("CK_OrderItems_Quantity", "[Quantity] BETWEEN 1 AND 99");
                    table.CheckConstraint("CK_OrderItems_UnitPrice", "[UnitPrice] > 0 AND [UnitPrice] <= 10000000");
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Code", "CreatedAt", "DisplayOrder", "IsActive", "Name" },
                values: new object[,]
                {
                    { "coldbrew", "coldbrew", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), 4, true, "Cold Brew đặc biệt" },
                    { "espresso", "espresso", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), 5, true, "Cà phê Ý (Espresso)" },
                    { "sandwich", "sandwich", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), 3, true, "Bánh mì & Sandwich" },
                    { "seasonal", "seasonal", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), 6, true, "Đồ uống theo mùa" },
                    { "snack", "snack", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), 2, true, "Đồ ăn nhẹ" },
                    { "tea", "tea", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), 7, true, "Trà trái cây" },
                    { "traditional", "traditional", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), 1, true, "Cà phê truyền thống" }
                });

            migrationBuilder.InsertData(
                table: "Discounts",
                columns: new[] { "Id", "Code", "DiscountPercent", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "NONE", 0m, true, "Không giảm" },
                    { 2, "SALE10", 10m, true, "Giảm 10%" },
                    { 3, "THANK15", 15m, true, "Giảm 15% tri ân" },
                    { 4, "SALE20", 20m, true, "Giảm 20% Khách VIP" }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CreatedAt", "DiscountAmount", "DiscountId", "DiscountPercent", "Note", "OrderNumber", "OrderType", "PaymentMethod", "RequestId", "Status", "Subtotal", "TaxAmount", "TaxPercent", "TotalAmount" },
                values: new object[,]
                {
                    { 1L, new DateTimeOffset(new DateTime(2026, 11, 7, 8, 10, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), 0m, null, 0m, "Đơn mẫu — ít đá", "#001", "Mang đi", "Tiền mặt", new Guid("00000000-0000-4000-8000-000000000001"), "Hoàn thành", 45000m, 0m, 0m, 45000m },
                    { 2L, new DateTimeOffset(new DateTime(2026, 11, 7, 8, 20, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), 10000m, 2, 10m, "Đơn mẫu — dùng tại bàn", "#002", "Tại bàn", "Thẻ ngân hàng", new Guid("00000000-0000-4000-8000-000000000002"), "Hoàn thành", 100000m, 7200m, 8m, 97200m },
                    { 3L, new DateTimeOffset(new DateTime(2026, 11, 7, 8, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), 20250m, 3, 15m, "Đơn mẫu — giao hàng", "#003", "Giao hàng", "Ví điện tử", new Guid("00000000-0000-4000-8000-000000000003"), "Hoàn thành", 135000m, 11475m, 10m, 126225m }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "ImageUrl", "IsAvailable", "Name", "OriginalPrice", "Price" },
                values: new object[,]
                {
                    { "p1", "traditional", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Cà phê sữa đá — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85", true, "Cà phê sữa đá", 55000m, 45000m },
                    { "p10", "seasonal", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Cà phê sữa mùa đông — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1544787219-7f47ccb76574?auto=format&fit=crop&w=800&q=85", true, "Cà phê sữa mùa đông", null, 49000m },
                    { "p11", "coldbrew", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Cold Brew đá — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1517701604599-bb29b565090c?auto=format&fit=crop&w=800&q=85", true, "Cold Brew đá", 55000m, 49000m },
                    { "p12", "snack", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Bánh quy bơ — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1544787219-7f47ccb76574?auto=format&fit=crop&w=800&q=85", true, "Bánh quy bơ", 35000m, 29000m },
                    { "p13", "seasonal", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Cà phê bọt biển Whipped Coffee — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85", true, "Cà phê bọt biển Whipped Coffee", 65000m, 45000m },
                    { "p14", "traditional", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Cà phê phin Filter Coffee — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=800&q=85", true, "Cà phê phin Filter Coffee", null, 22000m },
                    { "p15", "espresso", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Latte nghệ thuật — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85", true, "Latte nghệ thuật", null, 60000m },
                    { "p16", "coldbrew", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Cold Brew cam sả — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1556679343-c7306c1976bc?auto=format&fit=crop&w=800&q=85", true, "Cold Brew cam sả", null, 45000m },
                    { "p2", "espresso", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Cappuccino — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85", true, "Cappuccino", 65000m, 55000m },
                    { "p3", "snack", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Bánh Croissant bơ — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1555507036-ab1f4038808a?auto=format&fit=crop&w=800&q=85", true, "Bánh Croissant bơ", 50000m, 45000m },
                    { "p4", "espresso", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Americano nóng — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=800&q=85", true, "Americano nóng", 45000m, 40000m },
                    { "p5", "traditional", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Bạc xỉu đá — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1517701604599-bb29b565090c?auto=format&fit=crop&w=800&q=85", true, "Bạc xỉu đá", null, 39000m },
                    { "p6", "sandwich", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Sandwich nướng — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?auto=format&fit=crop&w=800&q=85", true, "Sandwich nướng", 75000m, 65000m },
                    { "p7", "coldbrew", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Cold Brew sữa tươi — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85", true, "Cold Brew sữa tươi", 60000m, 55000m },
                    { "p8", "tea", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Trà chanh thanh mát — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1556679343-c7306c1976bc?auto=format&fit=crop&w=800&q=85", true, "Trà chanh thanh mát", null, 45000m },
                    { "p9", "espresso", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "Latte kem sữa — món mẫu của CafePOS.", "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85", true, "Latte kem sữa", 65000m, 59000m }
                });

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "Id", "OrderId", "ProductId", "ProductName", "Quantity", "UnitPrice" },
                values: new object[,]
                {
                    { 1L, 1L, "p1", "Cà phê sữa đá", 1, 45000m },
                    { 2L, 2L, "p2", "Cappuccino", 1, 55000m },
                    { 3L, 2L, "p3", "Bánh Croissant bơ", 1, 45000m },
                    { 4L, 3L, "p13", "Cà phê bọt biển Whipped Coffee", 2, 45000m },
                    { 5L, 3L, "p16", "Cold Brew cam sả", 1, 45000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Code",
                table: "Categories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_Code",
                table: "Discounts",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId_ProductId",
                table: "OrderItems",
                columns: new[] { "OrderId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DiscountId",
                table: "Orders",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RequestId",
                table: "Orders",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_Name",
                table: "Products",
                columns: new[] { "CategoryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}

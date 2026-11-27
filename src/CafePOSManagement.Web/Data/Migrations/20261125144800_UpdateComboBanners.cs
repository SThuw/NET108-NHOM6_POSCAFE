using Microsoft.EntityFrameworkCore.Migrations;

namespace CafePOSManagement.Web.Data.Migrations;

public sealed partial class UpdateComboBanners : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var created = new DateTimeOffset(2026, 11, 7, 8, 0, 0, TimeSpan.FromHours(7));

        migrationBuilder.UpdateData(
            table: "Products",
            keyColumn: "Id",
            keyValue: "p23",
            columns: new[] { "Name", "OriginalPrice", "Price", "ImageUrl", "Description" },
            values: new object?[]
            {
                "Combo Bữa Sáng: Cafe Kem Trứng + Bánh Mì Chấm",
                50000m,
                35000m,
                "/images/kiosk/banner1.png",
                "Combo Bữa Sáng: Cafe Kem Trứng + Bánh Mì Chấm — món đặc sắc của CafePOS."
            });

        migrationBuilder.UpdateData(
            table: "Products",
            keyColumn: "Id",
            keyValue: "p24",
            columns: new[] { "Name", "OriginalPrice", "Price", "ImageUrl", "Description" },
            values: new object?[]
            {
                "Combo Deal Hời: Bánh Cuộn Trà Xanh + Matcha Đá Xay",
                59000m,
                43000m,
                "/images/kiosk/banner2.png",
                "Combo Deal Hời: Bánh Cuộn Trà Xanh + Matcha Đá Xay — món đặc sắc của CafePOS."
            });

        migrationBuilder.InsertData(
            table: "Products",
            columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "ImageUrl", "IsAvailable", "Name", "OriginalPrice", "Price" },
            values: new object?[]
            {
                "p25",
                "combo",
                created,
                "Combo Deal Sốc: Trà Sữa Ô Lông + Bánh Croissant Trứng Muối — món đặc sắc của CafePOS.",
                "/images/kiosk/banner3.png",
                true,
                "Combo Deal Sốc: Trà Sữa Ô Lông + Bánh Croissant Trứng Muối",
                65000m,
                45000m
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData("Products", "Id", "p25");

        migrationBuilder.UpdateData(
            table: "Products",
            keyColumn: "Id",
            keyValue: "p23",
            columns: new[] { "Name", "OriginalPrice", "Price", "ImageUrl", "Description" },
            values: new object?[]
            {
                "Combo Sáng: Cà phê + Bánh croissant",
                95000m,
                75000m,
                "https://images.unsplash.com/photo-1525610553991-2bede1a236e2?auto=format&fit=crop&w=800&q=85",
                "Combo Sáng: Cà phê + Bánh croissant — món đặc sắc của CafePOS."
            });

        migrationBuilder.UpdateData(
            table: "Products",
            keyColumn: "Id",
            keyValue: "p24",
            columns: new[] { "Name", "OriginalPrice", "Price", "ImageUrl", "Description" },
            values: new object?[]
            {
                "Combo Trà chiều: Trà đào + Bánh",
                85000m,
                69000m,
                "https://images.unsplash.com/photo-1509440159596-0249088772ff?auto=format&fit=crop&w=800&q=85",
                "Combo Trà chiều: Trà đào + Bánh — món đặc sắc của CafePOS."
            });
    }
}

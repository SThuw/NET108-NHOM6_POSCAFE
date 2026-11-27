using Microsoft.EntityFrameworkCore.Migrations;

namespace CafePOSManagement.Web.Data.Migrations;

public sealed partial class FixProductImagesMatchContent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData("Products", "Id", "p7", "ImageUrl", "https://images.unsplash.com/photo-1517701550927-30cf4ba1dba5?auto=format&fit=crop&w=800&q=85");
        migrationBuilder.UpdateData("Products", "Id", "p11", "ImageUrl", "https://images.unsplash.com/photo-1760304737368-7d8376bde94c?auto=format&fit=crop&w=800&q=85");
        migrationBuilder.UpdateData("Products", "Id", "p14", "ImageUrl", "https://images.unsplash.com/photo-1745210358756-e7f7ff40e506?auto=format&fit=crop&w=800&q=85");
        migrationBuilder.UpdateData("Products", "Id", "p16", "ImageUrl", "https://images.unsplash.com/photo-1786783506828-d0f8b16e4fbd?auto=format&fit=crop&w=800&q=85");
        migrationBuilder.UpdateData("Products", "Id", "p18", "ImageUrl", "https://images.unsplash.com/photo-1749280447307-31a68eb38673?auto=format&fit=crop&w=800&q=85");
        migrationBuilder.UpdateData("Products", "Id", "p20", "ImageUrl", "https://images.unsplash.com/photo-1692783029695-1956cd253e10?auto=format&fit=crop&w=800&q=85");
        migrationBuilder.UpdateData("Products", "Id", "p21", "ImageUrl", "https://images.unsplash.com/photo-1788016284566-c7f5d5dadc10?auto=format&fit=crop&w=800&q=85");
        migrationBuilder.UpdateData("Products", "Id", "p22", "ImageUrl", "https://images.unsplash.com/photo-1575417634984-8e608b88a04b?auto=format&fit=crop&w=800&q=85");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData("Products", "Id", "p7", "ImageUrl", "https://images.unsplash.com/photo-1517256064527-09c73fc73e38?auto=format&fit=crop&w=800&q=85");
        migrationBuilder.UpdateData("Products", "Id", "p11", "ImageUrl", "https://images.unsplash.com/photo-1517701550927-30cf4ba1dba5?auto=format&fit=crop&w=800&q=85");
        migrationBuilder.UpdateData("Products", "Id", "p14", "ImageUrl", "https://images.unsplash.com/photo-1511920170033-f8396924c348?auto=format&fit=crop&w=800&q=85");
        migrationBuilder.UpdateData("Products", "Id", "p16", "ImageUrl", "https://images.unsplash.com/photo-1544787219-7f47ccb76574?auto=format&fit=crop&w=800&q=85");
        migrationBuilder.UpdateData("Products", "Id", "p18", "ImageUrl", "https://images.unsplash.com/photo-1515823064-d6e0c04616a7?auto=format&fit=crop&w=800&q=85");
        migrationBuilder.UpdateData("Products", "Id", "p20", "ImageUrl", "https://images.unsplash.com/photo-1525385133512-2f3bdd039054?auto=format&fit=crop&w=800&q=85");
        migrationBuilder.UpdateData("Products", "Id", "p21", "ImageUrl", "https://images.unsplash.com/photo-1576092768241-dec231879fc3?auto=format&fit=crop&w=800&q=85");
        migrationBuilder.UpdateData("Products", "Id", "p22", "ImageUrl", "https://images.unsplash.com/photo-1579954115545-a95591f28bfc?auto=format&fit=crop&w=800&q=85");
    }
}

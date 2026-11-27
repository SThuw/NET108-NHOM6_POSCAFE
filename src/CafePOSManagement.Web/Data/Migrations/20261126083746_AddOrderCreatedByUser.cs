using Microsoft.EntityFrameworkCore.Migrations;

namespace CafePOSManagement.Web.Data.Migrations;

public sealed partial class AddOrderCreatedByUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CreatedByUserId",
            table: "Orders",
            type: "varchar(40)",
            unicode: false,
            maxLength: 40,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Orders_CreatedByUserId",
            table: "Orders",
            column: "CreatedByUserId");

        migrationBuilder.AddForeignKey(
            name: "FK_Orders_Users_CreatedByUserId",
            table: "Orders",
            column: "CreatedByUserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Orders_Users_CreatedByUserId",
            table: "Orders");

        migrationBuilder.DropIndex(
            name: "IX_Orders_CreatedByUserId",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "CreatedByUserId",
            table: "Orders");
    }
}

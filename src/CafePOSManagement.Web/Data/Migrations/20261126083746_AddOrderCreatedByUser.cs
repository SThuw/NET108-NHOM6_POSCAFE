using Microsoft.EntityFrameworkCore.Migrations;

namespace CafePOSManagement.Web.Data.Migrations;

public sealed partial class AddOrderCreatedByUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Orders', 'CreatedByUserId') IS NULL
BEGIN
    ALTER TABLE [Orders] ADD [CreatedByUserId] varchar(40) NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_CreatedByUserId' AND object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    CREATE INDEX [IX_Orders_CreatedByUserId] ON [Orders] ([CreatedByUserId]);
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Orders_Users_CreatedByUserId')
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL;
END
");
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

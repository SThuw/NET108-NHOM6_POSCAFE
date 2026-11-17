using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafePOSManagement.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixStatusConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_Status",
                table: "Orders");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_Status",
                table: "Orders",
                sql: "Status IN (N'Đang chuẩn bị', N'Chờ nhận món', N'Hoàn thành', N'Hủy đơn')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_Status",
                table: "Orders");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_Status",
                table: "Orders",
                sql: "[Status] IN (N'Hoàn thành', N'Đã hủy')");
        }
    }
}

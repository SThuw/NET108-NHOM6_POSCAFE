using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CafePOSManagement.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndCardsForLab3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsOccupied = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Phone = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Role = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "Id", "Code", "CreatedAt", "IsActive", "IsOccupied", "Name" },
                values: new object[,]
                {
                    { "c1", "TB01", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), true, false, "Thẻ Bàn 01" },
                    { "c2", "TB02", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), true, true, "Thẻ Bàn 02" },
                    { "c3", "TB03", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), true, false, "Thẻ Bàn 03" },
                    { "c4", "TR01", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), true, false, "Thẻ Rung 01" },
                    { "c5", "TR02", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), true, true, "Thẻ Rung 02" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "Password", "Phone", "Role" },
                values: new object[,]
                {
                    { "u1", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "thu@polycafe.vn", "Nguyễn Kim Song Thư", true, "123456", "0901234567", true },
                    { "u2", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "kha@polycafe.vn", "Trần Thị Tuyết Kha", true, "123456", "0902345678", false },
                    { "u3", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "nguyen@polycafe.vn", "Nguyễn Trí Nguyễn", true, "123456", "0903456789", false },
                    { "u4", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "duc@polycafe.vn", "Ninh Văn Đức", true, "123456", "0904567890", false },
                    { "u5", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "tam@polycafe.vn", "Nguyễn Lâm Chí Tâm", true, "123456", "0905678901", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Code",
                table: "Cards",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

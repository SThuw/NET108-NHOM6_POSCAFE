using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CafePOSManagement.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAiStateStoreCluster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiSessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    UserId = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Context = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AiMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Role = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiMessages_AiSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AiSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiStateStores",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    StateKey = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    StateValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiStateStores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiStateStores_AiSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AiSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AiSessions",
                columns: new[] { "Id", "Context", "CreatedAt", "IsActive", "Title", "UserId" },
                values: new object[,]
                {
                    { "sess_01", "PolyCafe AI Assistant - Trợ lý quầy POS", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), true, "Gợi ý combo cà phê sáng cho khách", "u1" },
                    { "sess_02", "PolyCafe AI Assistant - Tư vấn thực đơn", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), true, "Tư vấn món trà thanh nhiệt buổi chiều", "u4" }
                });

            migrationBuilder.InsertData(
                table: "AiMessages",
                columns: new[] { "Id", "Content", "CreatedAt", "Role", "SessionId" },
                values: new object[,]
                {
                    { 1L, "Khách muốn đồ uống đậm vị cà phê và bánh ăn kèm bữa sáng", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "user", "sess_01" },
                    { 2L, "Gợi ý combo: Cà phê sữa đá (p1) + Bánh Croissant bơ (p3), áp dụng mã SALE10 giảm 10%.", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "assistant", "sess_01" },
                    { 3L, "Khách nữ thích uống ngọt dịu, thanh mát ít đá", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "user", "sess_02" },
                    { 4L, "Gợi ý: Trà chanh thanh mát (p8) hoặc Cold Brew cam sả (p16) 50% đường.", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)), "assistant", "sess_02" }
                });

            migrationBuilder.InsertData(
                table: "AiStateStores",
                columns: new[] { "Id", "SessionId", "StateKey", "StateValue", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, "sess_01", "intent", "recommend_combo", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)) },
                    { 2L, "sess_01", "suggested_items", "[\"p1\", \"p3\"]", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)) },
                    { 3L, "sess_02", "intent", "recommend_drink", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)) },
                    { 4L, "sess_02", "taste_preference", "less_sweet,fruity", new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 0, 0, 0)) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiMessages_SessionId",
                table: "AiMessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AiSessions_UserId",
                table: "AiSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AiStateStores_SessionId_StateKey",
                table: "AiStateStores",
                columns: new[] { "SessionId", "StateKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiMessages");

            migrationBuilder.DropTable(
                name: "AiStateStores");

            migrationBuilder.DropTable(
                name: "AiSessions");
        }
    }
}

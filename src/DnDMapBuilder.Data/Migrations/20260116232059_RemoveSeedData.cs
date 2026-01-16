using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnDMapBuilder.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-seed-id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "PasswordHash", "Role", "Status", "UpdatedAt", "Username" },
                values: new object[] { "admin-seed-id", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@dndmapbuilder.com", "$2a$11$X4v3HBSSmstzKiv2vzPypu2WcKMh/e8Wttppq67PBM/5jalYdz2Kq", "admin", "approved", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin" });
        }
    }
}

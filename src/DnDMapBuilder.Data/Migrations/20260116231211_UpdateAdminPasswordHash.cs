using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnDMapBuilder.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-seed-id",
                column: "PasswordHash",
                value: "$2a$11$X4v3HBSSmstzKiv2vzPypu2WcKMh/e8Wttppq67PBM/5jalYdz2Kq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-seed-id",
                column: "PasswordHash",
                value: "$2a$11$XxvU8qZ5yP.yxKxQ8zHW7O8qKFdN1LQkGxKvYxGZ.hQvZNzVZY3.S");
        }
    }
}

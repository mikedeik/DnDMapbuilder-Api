using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnDMapBuilder.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicationStatusToGameMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PublicationStatus",
                table: "GameMaps",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicationStatus",
                table: "GameMaps");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnDMapBuilder.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageFileStorageMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "TokenDefinitions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageFileId",
                table: "TokenDefinitions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ImageFileSize",
                table: "TokenDefinitions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "GameMaps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageFileId",
                table: "GameMaps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ImageFileSize",
                table: "GameMaps",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "TokenDefinitions");

            migrationBuilder.DropColumn(
                name: "ImageFileId",
                table: "TokenDefinitions");

            migrationBuilder.DropColumn(
                name: "ImageFileSize",
                table: "TokenDefinitions");

            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "GameMaps");

            migrationBuilder.DropColumn(
                name: "ImageFileId",
                table: "GameMaps");

            migrationBuilder.DropColumn(
                name: "ImageFileSize",
                table: "GameMaps");
        }
    }
}

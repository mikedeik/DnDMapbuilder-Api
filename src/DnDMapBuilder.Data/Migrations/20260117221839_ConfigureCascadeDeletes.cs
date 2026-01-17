using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnDMapBuilder.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCascadeDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Users_OwnerId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_MapTokenInstances_TokenDefinitions_TokenId",
                table: "MapTokenInstances");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Users_OwnerId",
                table: "Campaigns",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MapTokenInstances_TokenDefinitions_TokenId",
                table: "MapTokenInstances",
                column: "TokenId",
                principalTable: "TokenDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Users_OwnerId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_MapTokenInstances_TokenDefinitions_TokenId",
                table: "MapTokenInstances");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Users_OwnerId",
                table: "Campaigns",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MapTokenInstances_TokenDefinitions_TokenId",
                table: "MapTokenInstances",
                column: "TokenId",
                principalTable: "TokenDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

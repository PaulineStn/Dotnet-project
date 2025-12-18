using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gauniv.WebServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameCategories_Category_CategoryId",
                table: "GameCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGamePurchase_AspNetUsers_UserId",
                table: "UserGamePurchase");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGamePurchase_Games_GameId",
                table: "UserGamePurchase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGamePurchase",
                table: "UserGamePurchase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Category",
                table: "Category");

            migrationBuilder.RenameTable(
                name: "UserGamePurchase",
                newName: "UserGamePurchases");

            migrationBuilder.RenameTable(
                name: "Category",
                newName: "Categories");

            migrationBuilder.RenameIndex(
                name: "IX_UserGamePurchase_GameId",
                table: "UserGamePurchases",
                newName: "IX_UserGamePurchases_GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGamePurchases",
                table: "UserGamePurchases",
                columns: new[] { "UserId", "GameId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameCategories_Categories_CategoryId",
                table: "GameCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGamePurchases_AspNetUsers_UserId",
                table: "UserGamePurchases",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGamePurchases_Games_GameId",
                table: "UserGamePurchases",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameCategories_Categories_CategoryId",
                table: "GameCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGamePurchases_AspNetUsers_UserId",
                table: "UserGamePurchases");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGamePurchases_Games_GameId",
                table: "UserGamePurchases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGamePurchases",
                table: "UserGamePurchases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "UserGamePurchases",
                newName: "UserGamePurchase");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Category");

            migrationBuilder.RenameIndex(
                name: "IX_UserGamePurchases_GameId",
                table: "UserGamePurchase",
                newName: "IX_UserGamePurchase_GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGamePurchase",
                table: "UserGamePurchase",
                columns: new[] { "UserId", "GameId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Category",
                table: "Category",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameCategories_Category_CategoryId",
                table: "GameCategories",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGamePurchase_AspNetUsers_UserId",
                table: "UserGamePurchase",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGamePurchase_Games_GameId",
                table: "UserGamePurchase",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

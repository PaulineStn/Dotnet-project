using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gauniv.WebServer.Migrations
{
    /// <inheritdoc />
    public partial class AddGameFilePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Payload",
                table: "Games");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Games",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Games");

            migrationBuilder.AddColumn<byte[]>(
                name: "Payload",
                table: "Games",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}

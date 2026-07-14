using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace otelrezervation.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdSoyad",
                table: "Users",
                newName: "Telefon");

            migrationBuilder.AddColumn<string>(
                name: "Ad",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Soyad",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ad",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Soyad",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Telefon",
                table: "Users",
                newName: "AdSoyad");
        }
    }
}

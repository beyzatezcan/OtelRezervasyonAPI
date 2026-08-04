using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace otelrezervation.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleImagesAndDesc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Rooms",
                newName: "Aciklama");

            migrationBuilder.AddColumn<List<string>>(
                name: "ImageUrls",
                table: "Rooms",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "Aciklama",
                table: "Rooms",
                newName: "ImageUrl");
        }
    }
}

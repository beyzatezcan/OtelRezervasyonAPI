using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace otelrezervation.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomCategorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kapasite",
                table: "Rooms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OdaTipi",
                table: "Rooms",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kapasite",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "OdaTipi",
                table: "Rooms");
        }
    }
}

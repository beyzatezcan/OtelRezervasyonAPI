using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace otelrezervation.Migrations
{
    /// <inheritdoc />
    public partial class AddPageNameToSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PageName",
                table: "SiteSettings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PageName",
                table: "SiteSettings");
        }
    }
}

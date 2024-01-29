using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailableToPromo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Available",
                table: "Promo",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Available",
                table: "Promo");
        }
    }
}

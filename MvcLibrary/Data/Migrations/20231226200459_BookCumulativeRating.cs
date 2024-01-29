using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class BookCumulativeRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CumulativeRating",
                table: "Book",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CumulativeRating",
                table: "Book");
        }
    }
}

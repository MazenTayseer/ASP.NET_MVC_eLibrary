using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPromoDiscountToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PromoDiscount",
                table: "Cart",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PromoDiscount",
                table: "Cart");
        }
    }
}

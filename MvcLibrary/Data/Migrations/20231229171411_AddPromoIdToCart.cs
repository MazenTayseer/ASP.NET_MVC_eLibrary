using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPromoIdToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Available",
                table: "Promo",
                newName: "Used");

            migrationBuilder.AddColumn<int>(
                name: "PromoId",
                table: "Cart",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cart_PromoId",
                table: "Cart",
                column: "PromoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cart_Promo_PromoId",
                table: "Cart",
                column: "PromoId",
                principalTable: "Promo",
                principalColumn: "PromoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cart_Promo_PromoId",
                table: "Cart");

            migrationBuilder.DropIndex(
                name: "IX_Cart_PromoId",
                table: "Cart");

            migrationBuilder.DropColumn(
                name: "PromoId",
                table: "Cart");

            migrationBuilder.RenameColumn(
                name: "Used",
                table: "Promo",
                newName: "Available");
        }
    }
}

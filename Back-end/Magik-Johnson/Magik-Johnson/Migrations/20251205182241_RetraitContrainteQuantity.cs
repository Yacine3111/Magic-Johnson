using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Magik_Johnson.Migrations
{
    /// <inheritdoc />
    public partial class RetraitContrainteQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Quantity_HigherThanZero1",
                table: "CardOrders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Quantity_HigherThanZero",
                table: "CardCart");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Quantity_HigherThanZero",
                table: "CardOrders",
                sql: "[Quantity] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Quantity_HigherThanZero",
                table: "CardOrders");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Quantity_HigherThanZero1",
                table: "CardOrders",
                sql: "[Quantity] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Quantity_HigherThanZero",
                table: "CardCart",
                sql: "[Quantity] > 0");
        }
    }
}

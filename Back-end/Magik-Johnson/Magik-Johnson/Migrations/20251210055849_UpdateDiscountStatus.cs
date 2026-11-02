using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Magik_Johnson.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDiscountStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Discounts");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Discounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "En Attente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Discounts");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Discounts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

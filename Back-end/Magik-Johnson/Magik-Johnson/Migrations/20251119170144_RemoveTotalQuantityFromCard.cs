using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Magik_Johnson.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTotalQuantityFromCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TotalQuantity_NonNegative",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "TotalQuantity",
                table: "Cards");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalQuantity",
                table: "Cards",
                type: "int",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddCheckConstraint(
                name: "CK_TotalQuantity_NonNegative",
                table: "Cards",
                sql: "[TotalQuantity] >= 0");
        }
    }
}

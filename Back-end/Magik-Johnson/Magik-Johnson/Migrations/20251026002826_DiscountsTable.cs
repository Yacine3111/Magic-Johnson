using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Magik_Johnson.Migrations
{
    /// <inheritdoc />
    public partial class DiscountsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DiscountId",
                table: "Cards",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rate = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.Id);
                    table.CheckConstraint("CK_Discount_Rate_Range", "[Rate] BETWEEN 1 AND 100");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_DiscountId",
                table: "Cards",
                column: "DiscountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Discounts_DiscountId",
                table: "Cards",
                column: "DiscountId",
                principalTable: "Discounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Discounts_DiscountId",
                table: "Cards");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropIndex(
                name: "IX_Cards_DiscountId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "DiscountId",
                table: "Cards");
        }
    }
}

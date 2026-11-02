using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Magik_Johnson.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFoilAndLanguageIdToCardOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFoil",
                table: "CardOrders",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<Guid>(
                name: "LanguageId",
                table: "CardOrders",
                type: "uniqueidentifier",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_CardOrders_LanguageId",
                table: "CardOrders",
                column: "LanguageId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_CardOrders_Languages_LanguageId",
                table: "CardOrders",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardOrders_Languages_LanguageId",
                table: "CardOrders"
            );

            migrationBuilder.DropIndex(name: "IX_CardOrders_LanguageId", table: "CardOrders");

            migrationBuilder.DropColumn(name: "IsFoil", table: "CardOrders");

            migrationBuilder.DropColumn(name: "LanguageId", table: "CardOrders");
        }
    }
}

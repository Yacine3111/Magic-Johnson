using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Magik_Johnson.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLanguageCartUpdateCardCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LanguageCart");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CardCart",
                table: "CardCart");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "CardCart",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LanguageId",
                table: "CardCart",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CardCart",
                table: "CardCart",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CardCart_CardId",
                table: "CardCart",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardCart_LanguageId",
                table: "CardCart",
                column: "LanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_CardCart_Languages_LanguageId",
                table: "CardCart",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardCart_Languages_LanguageId",
                table: "CardCart");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CardCart",
                table: "CardCart");

            migrationBuilder.DropIndex(
                name: "IX_CardCart_CardId",
                table: "CardCart");

            migrationBuilder.DropIndex(
                name: "IX_CardCart_LanguageId",
                table: "CardCart");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CardCart");

            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "CardCart");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CardCart",
                table: "CardCart",
                columns: new[] { "CardId", "CartId" });

            migrationBuilder.CreateTable(
                name: "LanguageCart",
                columns: table => new
                {
                    LanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsFoil = table.Column<bool>(type: "bit", nullable: false),
                    LanguageName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageCart", x => new { x.LanguageId, x.CartId });
                    table.ForeignKey(
                        name: "FK_LanguageCart_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LanguageCart_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LanguageCart_CartId",
                table: "LanguageCart",
                column: "CartId");
        }
    }
}

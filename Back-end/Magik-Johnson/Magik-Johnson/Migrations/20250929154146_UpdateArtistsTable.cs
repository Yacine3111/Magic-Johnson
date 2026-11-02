using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Magik_Johnson.Migrations
{
    /// <inheritdoc />
    public partial class UpdateArtistsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Artists_ArstistId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_ArstistId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "ArstistId",
                table: "Cards");

            migrationBuilder.CreateTable(
                name: "ArtistCard",
                columns: table => new
                {
                    ArtistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistCard", x => new { x.ArtistId, x.CardId });
                    table.ForeignKey(
                        name: "FK_ArtistCard_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistCard_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtistCard_CardId",
                table: "ArtistCard",
                column: "CardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtistCard");

            migrationBuilder.AddColumn<Guid>(
                name: "ArstistId",
                table: "Cards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ArstistId",
                table: "Cards",
                column: "ArstistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Artists_ArstistId",
                table: "Cards",
                column: "ArstistId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

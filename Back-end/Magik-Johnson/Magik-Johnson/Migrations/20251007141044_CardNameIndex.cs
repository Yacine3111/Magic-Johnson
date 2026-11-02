using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Magik_Johnson.Migrations
{
    /// <inheritdoc />
    public partial class CardNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Cards_Name",
                table: "Cards",
                column: "Name",
                unique: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Cards_Name", table: "Cards");
        }
    }
}

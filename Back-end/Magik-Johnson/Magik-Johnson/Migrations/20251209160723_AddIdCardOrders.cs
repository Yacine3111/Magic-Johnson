using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Magik_Johnson.Migrations
{
    /// <inheritdoc />
    public partial class AddIdCardOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CardOrders",
                table: "CardOrders");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "CardOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CardOrders",
                table: "CardOrders",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CardOrders_CardId",
                table: "CardOrders",
                column: "CardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CardOrders",
                table: "CardOrders");

            migrationBuilder.DropIndex(
                name: "IX_CardOrders_CardId",
                table: "CardOrders");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CardOrders");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CardOrders",
                table: "CardOrders",
                columns: new[] { "CardId", "OrderId" });
        }
    }
}

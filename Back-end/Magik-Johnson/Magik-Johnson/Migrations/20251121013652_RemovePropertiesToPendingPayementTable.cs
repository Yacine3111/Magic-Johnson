using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Magik_Johnson.Migrations
{
    /// <inheritdoc />
    public partial class RemovePropertiesToPendingPayementTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
         

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PendingPayments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

           

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "PendingPayments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "PendingPayments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

         
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PendingPayments");

            
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "PendingPayments");

            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "PendingPayments");

        
        }
    }
}

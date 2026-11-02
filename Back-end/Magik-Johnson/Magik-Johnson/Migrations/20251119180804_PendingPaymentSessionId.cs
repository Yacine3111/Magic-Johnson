using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Magik_Johnson.Migrations
{
    /// <inheritdoc />
    public partial class PendingPaymentSessionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CreatedAt", table: "PendingPayments");

            migrationBuilder.DropColumn(name: "IsCompleted", table: "PendingPayments");

            migrationBuilder.RenameColumn(
                name: "PaymentIntentId",
                table: "PendingPayments",
                newName: "SessionId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_PendingPayments_PaymentIntentId",
                table: "PendingPayments",
                newName: "IX_PendingPayments_SessionId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "PendingPayments",
                newName: "PaymentIntentId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_PendingPayments_SessionId",
                table: "PendingPayments",
                newName: "IX_PendingPayments_PaymentIntentId"
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PendingPayments",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()"
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "PendingPayments",
                type: "bit",
                nullable: false,
                defaultValue: false
            );
        }
    }
}

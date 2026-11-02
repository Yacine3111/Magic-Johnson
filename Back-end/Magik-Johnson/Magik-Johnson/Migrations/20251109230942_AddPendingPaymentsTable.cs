using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Magik_Johnson.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingPaymentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "PendingPayments",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        UserId = table.Column<string>(
            //            type: "nvarchar(450)",
            //            maxLength: 450,
            //            nullable: false
            //        ),
            //        PaymentIntentId = table.Column<string>(
            //            type: "nvarchar(255)",
            //            maxLength: 255,
            //            nullable: false
            //        ),
            //        ShippingAddress = table.Column<string>(
            //            type: "nvarchar(1000)",
            //            maxLength: 1000,
            //            nullable: true
            //        ),
            //        TotalAmount = table.Column<decimal>(
            //            type: "decimal(10,2)",
            //            precision: 10,
            //            scale: 2,
            //            nullable: false
            //        ),
            //        IsCompleted = table.Column<bool>(
            //            type: "bit",
            //            nullable: false,
            //            defaultValue: false
            //        ),
            //        CreatedAt = table.Column<DateTime>(
            //            type: "datetime2",
            //            nullable: false,
            //            defaultValueSql: "GETUTCDATE()"
            //        ),
            //        ItemsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_PendingPayments", x => x.Id);
            //    }
            //);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Address_ProvinceId",
            //    table: "Address",
            //    column: "ProvinceId"
            //);

            //migrationBuilder.CreateIndex(
            //    name: "IX_PendingPayments_PaymentIntentId",
            //    table: "PendingPayments",
            //    column: "PaymentIntentId",
            //    unique: true
            //);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Address_Provinces_ProvinceId",
            //    table: "Address",
            //    column: "ProvinceId",
            //    principalTable: "Provinces",
            //    principalColumn: "Id"
            //);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Address_Provinces_ProvinceId",
            //    table: "Address"
            //);

            //migrationBuilder.DropTable(name: "PendingPayments");

            //migrationBuilder.DropIndex(name: "IX_Address_ProvinceId", table: "Address");
        }
    }
}

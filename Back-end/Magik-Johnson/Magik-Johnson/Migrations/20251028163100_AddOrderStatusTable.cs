using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Magik_Johnson.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStatusTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatus", x => x.Id);
                }
            );

            migrationBuilder.InsertData(
                table: "OrderStatus",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("7592b995-b9c5-43ee-90bb-02a393b3082c"), "Annulée" },
                    { new Guid("75a2c6cf-ae61-4391-8f8e-87bf4936fe97"), "Livrée" },
                    { new Guid("8a239872-a49c-473a-b718-b94efdeb1ba0"), "Remboursée" },
                    { new Guid("94308a82-238d-406e-8795-6f4e80771459"), "Expédiée" },
                    { new Guid("c520e178-7a8a-42ac-9781-b1030ea13088"), "Confirmée" },
                    { new Guid("de094427-cf62-40fc-a9c5-ee4e2e6b2446"), "En attente" },
                    { new Guid("fe772970-2e11-416f-a823-3315694dd510"), "En préparation" },
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quantities_Languages_LanguageId",
                table: "Quantities"
            );

            migrationBuilder.DropTable(name: "OrderStatus");

            migrationBuilder.DropPrimaryKey(name: "PK_Quantities", table: "Quantities");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Quantities",
                table: "Quantities",
                columns: new[] { "Id", "CardId" }
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Quantities_Languages_LanguageId",
                table: "Quantities",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id"
            );
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Magik_Johnson.Migrations
{
    /// <inheritdoc />
    public partial class OrderStatusFixId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "Id",
                keyValue: new Guid("0bfd0899-1416-4c40-9854-673762f0fafc"));

            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "Id",
                keyValue: new Guid("1071b63a-afd2-4f82-8758-ec445f1d2aa9"));

            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "Id",
                keyValue: new Guid("20787870-f9a8-441a-8ace-a84fe78818e0"));

            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "Id",
                keyValue: new Guid("2796d337-467a-4158-92c4-83ef4e5221f8"));

            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "Id",
                keyValue: new Guid("2cb37c9e-295f-4f60-952f-ae1f4e32cca8"));

            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "Id",
                keyValue: new Guid("78bfe6f1-0b3d-4c21-9f53-d821c968e016"));

            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "Id",
                keyValue: new Guid("f094ec6d-1489-4cd4-8df0-ce04e18b931d"));

            migrationBuilder.InsertData(
                table: "OrderStatus",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("2a8a04f8-ea2f-4669-949b-191940b9f489"), "Expédiée" },
                    { new Guid("69d34e1a-1b99-4b56-aa53-da04da87940d"), "Annulée" },
                    { new Guid("6a9bd7f3-d5e5-4f73-b4f9-d98e6f7d8c2a"), "Confirmée" },
                    { new Guid("6f6a895d-67f0-4da5-a873-63588d7a1c73"), "Remboursée" },
                    { new Guid("7fe741db-ec17-47f8-8a8f-548743984961"), "Livrée" },
                    { new Guid("d8c70d58-a557-4ea2-9301-5bd6626eb585"), "En attente" },
                    { new Guid("ee4bb5b8-94b0-4bfe-8c59-34a4c35ee052"), "En préparation" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "Id",
                keyValue: new Guid("2a8a04f8-ea2f-4669-949b-191940b9f489"));

            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "Id",
                keyValue: new Guid("69d34e1a-1b99-4b56-aa53-da04da87940d"));

            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "Id",
                keyValue: new Guid("6a9bd7f3-d5e5-4f73-b4f9-d98e6f7d8c2a"));

            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "Id",
                keyValue: new Guid("6f6a895d-67f0-4da5-a873-63588d7a1c73"));

            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "Id",
                keyValue: new Guid("7fe741db-ec17-47f8-8a8f-548743984961"));

            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "Id",
                keyValue: new Guid("d8c70d58-a557-4ea2-9301-5bd6626eb585"));

            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "Id",
                keyValue: new Guid("ee4bb5b8-94b0-4bfe-8c59-34a4c35ee052"));

            migrationBuilder.InsertData(
                table: "OrderStatus",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("0bfd0899-1416-4c40-9854-673762f0fafc"), "Annulée" },
                    { new Guid("1071b63a-afd2-4f82-8758-ec445f1d2aa9"), "Confirmée" },
                    { new Guid("20787870-f9a8-441a-8ace-a84fe78818e0"), "Expédiée" },
                    { new Guid("2796d337-467a-4158-92c4-83ef4e5221f8"), "En préparation" },
                    { new Guid("2cb37c9e-295f-4f60-952f-ae1f4e32cca8"), "Livrée" },
                    { new Guid("78bfe6f1-0b3d-4c21-9f53-d821c968e016"), "Remboursée" },
                    { new Guid("f094ec6d-1489-4cd4-8df0-ce04e18b931d"), "En attente" }
                });
        }
    }
}

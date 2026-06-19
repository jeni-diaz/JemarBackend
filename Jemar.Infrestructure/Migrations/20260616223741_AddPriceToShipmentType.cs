using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jemar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToShipmentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ShipmentTypes",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Shipments",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.UpdateData(
                table: "ShipmentTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Price",
                value: 3000.00m);

            migrationBuilder.UpdateData(
                table: "ShipmentTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Price",
                value: 1500.00m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "ShipmentTypes");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Shipments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldPrecision: 6,
                oldScale: 2);
        }
    }
}

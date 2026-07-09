using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jemar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePriceFromShipmentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "ShipmentTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ShipmentTypes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

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
    }
}

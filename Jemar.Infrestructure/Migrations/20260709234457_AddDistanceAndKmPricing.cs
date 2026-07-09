using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jemar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDistanceAndKmPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DistanceKm",
                table: "Shipments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "PackageSizes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RatePerKm",
                table: "PackageSizes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "PackageSizes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BasePrice", "RatePerKm" },
                values: new object[] { 1500.00m, 20.00m });

            migrationBuilder.UpdateData(
                table: "PackageSizes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BasePrice", "RatePerKm" },
                values: new object[] { 2500.00m, 35.00m });

            migrationBuilder.UpdateData(
                table: "PackageSizes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BasePrice", "RatePerKm" },
                values: new object[] { 4000.00m, 50.00m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistanceKm",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "PackageSizes");

            migrationBuilder.DropColumn(
                name: "RatePerKm",
                table: "PackageSizes");
        }
    }
}

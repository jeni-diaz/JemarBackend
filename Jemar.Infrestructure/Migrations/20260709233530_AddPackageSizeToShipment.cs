using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Jemar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageSizeToShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackageSizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<int>(type: "int", nullable: false),
                    MaxLengthCm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxWidthCm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxHeightCm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Surcharge = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageSizes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PackageSizes",
                columns: new[] { "Id", "MaxHeightCm", "MaxLengthCm", "MaxWidthCm", "Name", "Surcharge" },
                values: new object[,]
                {
                    { 1, 30.00m, 30.00m, 30.00m, 1, 0.00m },
                    { 2, 60.00m, 60.00m, 60.00m, 2, 1000.00m },
                    { 3, 120.00m, 120.00m, 120.00m, 3, 2500.00m }
                });

            migrationBuilder.AddColumn<int>(
                name: "PackageSizeId",
                table: "Shipments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_PackageSizeId",
                table: "Shipments",
                column: "PackageSizeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_PackageSizes_PackageSizeId",
                table: "Shipments",
                column: "PackageSizeId",
                principalTable: "PackageSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_PackageSizes_PackageSizeId",
                table: "Shipments");

            migrationBuilder.DropTable(
                name: "PackageSizes");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_PackageSizeId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PackageSizeId",
                table: "Shipments");
        }
    }
}

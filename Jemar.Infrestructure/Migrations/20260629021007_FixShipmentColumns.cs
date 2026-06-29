using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jemar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixShipmentColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Users_ClientId",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Users_EmployeeId",
                table: "Shipments");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Shipments",
                newName: "OnBehalfOfClientId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Shipments",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Shipments_EmployeeId",
                table: "Shipments",
                newName: "IX_Shipments_OnBehalfOfClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Shipments_ClientId",
                table: "Shipments",
                newName: "IX_Shipments_CreatedByUserId");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByRoleId",
                table: "Shipments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_CreatedByRoleId",
                table: "Shipments",
                column: "CreatedByRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Roles_CreatedByRoleId",
                table: "Shipments",
                column: "CreatedByRoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Users_CreatedByUserId",
                table: "Shipments",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Users_OnBehalfOfClientId",
                table: "Shipments",
                column: "OnBehalfOfClientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Roles_CreatedByRoleId",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Users_CreatedByUserId",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Users_OnBehalfOfClientId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_CreatedByRoleId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "CreatedByRoleId",
                table: "Shipments");

            migrationBuilder.RenameColumn(
                name: "OnBehalfOfClientId",
                table: "Shipments",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Shipments",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Shipments_OnBehalfOfClientId",
                table: "Shipments",
                newName: "IX_Shipments_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Shipments_CreatedByUserId",
                table: "Shipments",
                newName: "IX_Shipments_ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Users_ClientId",
                table: "Shipments",
                column: "ClientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Users_EmployeeId",
                table: "Shipments",
                column: "EmployeeId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

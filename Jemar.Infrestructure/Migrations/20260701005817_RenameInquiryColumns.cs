using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jemar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameInquiryColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inquiries_Users_ClientId",
                table: "Inquiries");

            migrationBuilder.DropForeignKey(
                name: "FK_Inquiries_Users_EmployeeId",
                table: "Inquiries");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Inquiries",
                newName: "RespondedByUserId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Inquiries",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Inquiries_EmployeeId",
                table: "Inquiries",
                newName: "IX_Inquiries_RespondedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Inquiries_ClientId",
                table: "Inquiries",
                newName: "IX_Inquiries_CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inquiries_Users_CreatedByUserId",
                table: "Inquiries",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inquiries_Users_RespondedByUserId",
                table: "Inquiries",
                column: "RespondedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inquiries_Users_CreatedByUserId",
                table: "Inquiries");

            migrationBuilder.DropForeignKey(
                name: "FK_Inquiries_Users_RespondedByUserId",
                table: "Inquiries");

            migrationBuilder.RenameColumn(
                name: "RespondedByUserId",
                table: "Inquiries",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Inquiries",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Inquiries_RespondedByUserId",
                table: "Inquiries",
                newName: "IX_Inquiries_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Inquiries_CreatedByUserId",
                table: "Inquiries",
                newName: "IX_Inquiries_ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inquiries_Users_ClientId",
                table: "Inquiries",
                column: "ClientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inquiries_Users_EmployeeId",
                table: "Inquiries",
                column: "EmployeeId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

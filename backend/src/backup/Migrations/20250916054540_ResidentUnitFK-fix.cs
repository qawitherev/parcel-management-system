using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ResidentUnitFKfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddForeignKey(
                name: "FK_ResidentUnits_Users_CreatedBy",
                table: "ResidentUnits",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResidentUnits_Users_UpdatedBy",
                table: "ResidentUnits",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResidentUnits_Users_CreatedBy",
                table: "ResidentUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_ResidentUnits_Users_UpdatedBy",
                table: "ResidentUnits");

            migrationBuilder.DropIndex(
                name: "IX_ResidentUnits_CreatedBy",
                table: "ResidentUnits");

            migrationBuilder.DropIndex(
                name: "IX_ResidentUnits_UpdatedBy",
                table: "ResidentUnits");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ParcelAddLockerColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LockerId",
                table: "Parcels",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Parcels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Parcels_LockerId",
                table: "Parcels",
                column: "LockerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parcels_Lockers_LockerId",
                table: "Parcels",
                column: "LockerId",
                principalTable: "Lockers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parcels_Lockers_LockerId",
                table: "Parcels");

            migrationBuilder.DropIndex(
                name: "IX_Parcels_LockerId",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "LockerId",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Parcels");
        }
    }
}

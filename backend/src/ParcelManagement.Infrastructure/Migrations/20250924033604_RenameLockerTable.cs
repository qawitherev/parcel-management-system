using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameLockerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locker_Users_CreatedBy",
                table: "Locker");

            migrationBuilder.DropForeignKey(
                name: "FK_Locker_Users_UpdatedBy",
                table: "Locker");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Locker",
                table: "Locker");

            migrationBuilder.RenameTable(
                name: "Locker",
                newName: "Lockers");

            migrationBuilder.RenameIndex(
                name: "IX_Locker_UpdatedBy",
                table: "Lockers",
                newName: "IX_Lockers_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Locker_LockerName",
                table: "Lockers",
                newName: "IX_Lockers_LockerName");

            migrationBuilder.RenameIndex(
                name: "IX_Locker_CreatedBy",
                table: "Lockers",
                newName: "IX_Lockers_CreatedBy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lockers",
                table: "Lockers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lockers_Users_CreatedBy",
                table: "Lockers",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lockers_Users_UpdatedBy",
                table: "Lockers",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lockers_Users_CreatedBy",
                table: "Lockers");

            migrationBuilder.DropForeignKey(
                name: "FK_Lockers_Users_UpdatedBy",
                table: "Lockers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lockers",
                table: "Lockers");

            migrationBuilder.RenameTable(
                name: "Lockers",
                newName: "Locker");

            migrationBuilder.RenameIndex(
                name: "IX_Lockers_UpdatedBy",
                table: "Locker",
                newName: "IX_Locker_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Lockers_LockerName",
                table: "Locker",
                newName: "IX_Locker_LockerName");

            migrationBuilder.RenameIndex(
                name: "IX_Lockers_CreatedBy",
                table: "Locker",
                newName: "IX_Locker_CreatedBy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Locker",
                table: "Locker",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Locker_Users_CreatedBy",
                table: "Locker",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Locker_Users_UpdatedBy",
                table: "Locker",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

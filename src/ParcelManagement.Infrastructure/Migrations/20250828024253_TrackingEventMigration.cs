using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TrackingEventMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parcels_ResidentUnit_ResidentUnitId",
                table: "Parcels");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResidentUnit_ResidentUnit_ResidentUnitId",
                table: "UserResidentUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResidentUnit_Users_UserId",
                table: "UserResidentUnit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserResidentUnit",
                table: "UserResidentUnit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResidentUnit",
                table: "ResidentUnit");

            migrationBuilder.RenameTable(
                name: "UserResidentUnit",
                newName: "UserResidentUnits");

            migrationBuilder.RenameTable(
                name: "ResidentUnit",
                newName: "ResidentUnits");

            migrationBuilder.RenameIndex(
                name: "IX_UserResidentUnit_ResidentUnitId",
                table: "UserResidentUnits",
                newName: "IX_UserResidentUnits_ResidentUnitId");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordSalt",
                table: "Users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ResidentUnitDeprecated",
                table: "Parcels",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "UserResidentUnits",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserResidentUnits",
                table: "UserResidentUnits",
                columns: new[] { "UserId", "ResidentUnitId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResidentUnits",
                table: "ResidentUnits",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "TrackingEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ParcelId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TrackingEventType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomEvent = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    PerformedByUser = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackingEvents_Parcels_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "Parcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ResidentUnits_UnitName",
                table: "ResidentUnits",
                column: "UnitName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackingEvents_ParcelId",
                table: "TrackingEvents",
                column: "ParcelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parcels_ResidentUnits_ResidentUnitId",
                table: "Parcels",
                column: "ResidentUnitId",
                principalTable: "ResidentUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResidentUnits_ResidentUnits_ResidentUnitId",
                table: "UserResidentUnits",
                column: "ResidentUnitId",
                principalTable: "ResidentUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResidentUnits_Users_UserId",
                table: "UserResidentUnits",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parcels_ResidentUnits_ResidentUnitId",
                table: "Parcels");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResidentUnits_ResidentUnits_ResidentUnitId",
                table: "UserResidentUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResidentUnits_Users_UserId",
                table: "UserResidentUnits");

            migrationBuilder.DropTable(
                name: "TrackingEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserResidentUnits",
                table: "UserResidentUnits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResidentUnits",
                table: "ResidentUnits");

            migrationBuilder.DropIndex(
                name: "IX_ResidentUnits_UnitName",
                table: "ResidentUnits");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserResidentUnits");

            migrationBuilder.RenameTable(
                name: "UserResidentUnits",
                newName: "UserResidentUnit");

            migrationBuilder.RenameTable(
                name: "ResidentUnits",
                newName: "ResidentUnit");

            migrationBuilder.RenameIndex(
                name: "IX_UserResidentUnits_ResidentUnitId",
                table: "UserResidentUnit",
                newName: "IX_UserResidentUnit_ResidentUnitId");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "PasswordSalt",
                keyValue: null,
                column: "PasswordSalt",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordSalt",
                table: "Users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Parcels",
                keyColumn: "ResidentUnitDeprecated",
                keyValue: null,
                column: "ResidentUnitDeprecated",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ResidentUnitDeprecated",
                table: "Parcels",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserResidentUnit",
                table: "UserResidentUnit",
                columns: new[] { "UserId", "ResidentUnitId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResidentUnit",
                table: "ResidentUnit",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Parcels_ResidentUnit_ResidentUnitId",
                table: "Parcels",
                column: "ResidentUnitId",
                principalTable: "ResidentUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResidentUnit_ResidentUnit_ResidentUnitId",
                table: "UserResidentUnit",
                column: "ResidentUnitId",
                principalTable: "ResidentUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResidentUnit_Users_UserId",
                table: "UserResidentUnit",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

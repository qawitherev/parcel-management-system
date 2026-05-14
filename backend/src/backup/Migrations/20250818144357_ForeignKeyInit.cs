using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ForeignKeyInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResidentUnit",
                table: "Users",
                newName: "ResidentUnitDeprecated");

            migrationBuilder.RenameColumn(
                name: "ResidentUnit",
                table: "Parcels",
                newName: "ResidentUnitDeprecated");

            migrationBuilder.AddColumn<Guid>(
                name: "ResidentUnitId",
                table: "Parcels",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "ResidentUnit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UnitName = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidentUnit", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserResidentUnit",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ResidentUnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserResidentUnit", x => new { x.UserId, x.ResidentUnitId });
                    table.ForeignKey(
                        name: "FK_UserResidentUnit_ResidentUnit_ResidentUnitId",
                        column: x => x.ResidentUnitId,
                        principalTable: "ResidentUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserResidentUnit_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Parcels_ResidentUnitId",
                table: "Parcels",
                column: "ResidentUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResidentUnit_ResidentUnitId",
                table: "UserResidentUnit",
                column: "ResidentUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parcels_ResidentUnit_ResidentUnitId",
                table: "Parcels",
                column: "ResidentUnitId",
                principalTable: "ResidentUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parcels_ResidentUnit_ResidentUnitId",
                table: "Parcels");

            migrationBuilder.DropTable(
                name: "UserResidentUnit");

            migrationBuilder.DropTable(
                name: "ResidentUnit");

            migrationBuilder.DropIndex(
                name: "IX_Parcels_ResidentUnitId",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "ResidentUnitId",
                table: "Parcels");

            migrationBuilder.RenameColumn(
                name: "ResidentUnitDeprecated",
                table: "Users",
                newName: "ResidentUnit");

            migrationBuilder.RenameColumn(
                name: "ResidentUnitDeprecated",
                table: "Parcels",
                newName: "ResidentUnit");
        }
    }
}

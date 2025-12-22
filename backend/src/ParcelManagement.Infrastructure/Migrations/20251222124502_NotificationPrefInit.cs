using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NotificationPrefInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationPref",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsEmailActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsWhatsAppActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsOnCheckInActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsOnClaimActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsOverdueActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QuiteHoursFrom = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    QuiteHoursTo = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPref", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationPref_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationPref_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotificationPref_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPref_CreatedBy",
                table: "NotificationPref",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPref_UpdatedBy",
                table: "NotificationPref",
                column: "UpdatedBy",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPref_UserId",
                table: "NotificationPref",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationPref");
        }
    }
}

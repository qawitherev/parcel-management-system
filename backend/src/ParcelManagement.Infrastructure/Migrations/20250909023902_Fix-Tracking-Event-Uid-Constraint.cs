using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTrackingEventUidConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TrackingEvents_PerformedByUser",
                table: "TrackingEvents",
                column: "PerformedByUser");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackingEvents_Users_PerformedByUser",
                table: "TrackingEvents",
                column: "PerformedByUser",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackingEvents_Users_PerformedByUser",
                table: "TrackingEvents");

            migrationBuilder.DropIndex(
                name: "IX_TrackingEvents_PerformedByUser",
                table: "TrackingEvents");
        }
    }
}

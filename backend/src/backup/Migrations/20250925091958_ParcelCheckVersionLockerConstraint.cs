using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ParcelCheckVersionLockerConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Parcels_LockerRequired",
                table: "Parcels",
                sql: "`Version` < 2 OR (`Version` > 1 AND `LockerId` IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Parcels_LockerRequired",
                table: "Parcels");
        }
    }
}

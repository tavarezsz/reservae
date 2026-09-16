using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservae.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadingPriceOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "CustomPricePerSpot",
                table: "BookableSlots",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "CustomPricePerSpot",
                table: "AvailabilityRules",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BookableSlots_CustomPricePerSpot",
                table: "BookableSlots",
                sql: "\"CustomPricePerSpot\" IS NULL OR \"CustomPricePerSpot\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AvailabilityRules_CustomPricePerSpot",
                table: "AvailabilityRules",
                sql: "\"CustomPricePerSpot\" IS NULL OR \"CustomPricePerSpot\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BookableSlots_CustomPricePerSpot",
                table: "BookableSlots");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AvailabilityRules_CustomPricePerSpot",
                table: "AvailabilityRules");

            migrationBuilder.AlterColumn<decimal>(
                name: "CustomPricePerSpot",
                table: "BookableSlots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CustomPricePerSpot",
                table: "AvailabilityRules",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);
        }
    }
}

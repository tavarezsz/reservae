using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservae.Migrations
{
    /// <inheritdoc />
    public partial class SupportLazyAndStandaloneSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookableSlots_AvailabilityRuleId",
                table: "BookableSlots");

            migrationBuilder.AlterColumn<int>(
                name: "AvailabilityRuleId",
                table: "BookableSlots",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "SlotDurationMinutes",
                table: "AvailabilityRules",
                type: "integer",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.CreateIndex(
                name: "IX_BookableSlots_AvailabilityRuleId_StartsAt",
                table: "BookableSlots",
                columns: new[] { "AvailabilityRuleId", "StartsAt" },
                unique: true,
                filter: "\"AvailabilityRuleId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BookableSlots_Period",
                table: "BookableSlots",
                sql: "\"EndsAt\" > \"StartsAt\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AvailabilityRules_SlotDurationMinutes",
                table: "AvailabilityRules",
                sql: "\"SlotDurationMinutes\" >= 30");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookableSlots_AvailabilityRuleId_StartsAt",
                table: "BookableSlots");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BookableSlots_Period",
                table: "BookableSlots");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AvailabilityRules_SlotDurationMinutes",
                table: "AvailabilityRules");

            migrationBuilder.DropColumn(
                name: "SlotDurationMinutes",
                table: "AvailabilityRules");

            migrationBuilder.AlterColumn<int>(
                name: "AvailabilityRuleId",
                table: "BookableSlots",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookableSlots_AvailabilityRuleId",
                table: "BookableSlots",
                column: "AvailabilityRuleId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPOBalance.Migrations
{
    /// <inheritdoc />
    public partial class AddLongTermCareAndIndustrialAccidentExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LongTermCareRate",
                table: "InsuranceRateSettings");

            migrationBuilder.AddColumn<decimal>(
                name: "IndustrialAccidentMinBaseAmount",
                table: "InsuranceRateSettings",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IndustrialAccidentMinPremium",
                table: "InsuranceRateSettings",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IndustrialAccidentRateEmployee",
                table: "InsuranceRateSettings",
                type: "TEXT",
                precision: 7,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LongTermCareMinBaseAmount",
                table: "InsuranceRateSettings",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LongTermCareMinPremium",
                table: "InsuranceRateSettings",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LongTermCareRateEmployee",
                table: "InsuranceRateSettings",
                type: "TEXT",
                precision: 7,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LongTermCareRateEmployer",
                table: "InsuranceRateSettings",
                type: "TEXT",
                precision: 7,
                scale: 6,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IndustrialAccidentMinBaseAmount",
                table: "InsuranceRateSettings");

            migrationBuilder.DropColumn(
                name: "IndustrialAccidentMinPremium",
                table: "InsuranceRateSettings");

            migrationBuilder.DropColumn(
                name: "IndustrialAccidentRateEmployee",
                table: "InsuranceRateSettings");

            migrationBuilder.DropColumn(
                name: "LongTermCareMinBaseAmount",
                table: "InsuranceRateSettings");

            migrationBuilder.DropColumn(
                name: "LongTermCareMinPremium",
                table: "InsuranceRateSettings");

            migrationBuilder.DropColumn(
                name: "LongTermCareRateEmployee",
                table: "InsuranceRateSettings");

            migrationBuilder.DropColumn(
                name: "LongTermCareRateEmployer",
                table: "InsuranceRateSettings");

            migrationBuilder.AddColumn<decimal>(
                name: "LongTermCareRate",
                table: "InsuranceRateSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

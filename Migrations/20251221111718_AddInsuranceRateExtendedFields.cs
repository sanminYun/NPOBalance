using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPOBalance.Migrations
{
    /// <inheritdoc />
    public partial class AddInsuranceRateExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EmploymentInsuranceMinBaseAmount",
                table: "InsuranceRateSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmploymentInsuranceMinPremium",
                table: "InsuranceRateSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HealthInsuranceMinBaseAmount",
                table: "InsuranceRateSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HealthInsuranceMinPremium",
                table: "InsuranceRateSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IndustrialAccidentRateEmployer",
                table: "InsuranceRateSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LongTermCareRate",
                table: "InsuranceRateSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NationalPensionMinBaseAmount",
                table: "InsuranceRateSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NationalPensionMinPremium",
                table: "InsuranceRateSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmploymentInsuranceMinBaseAmount",
                table: "InsuranceRateSettings");

            migrationBuilder.DropColumn(
                name: "EmploymentInsuranceMinPremium",
                table: "InsuranceRateSettings");

            migrationBuilder.DropColumn(
                name: "HealthInsuranceMinBaseAmount",
                table: "InsuranceRateSettings");

            migrationBuilder.DropColumn(
                name: "HealthInsuranceMinPremium",
                table: "InsuranceRateSettings");

            migrationBuilder.DropColumn(
                name: "IndustrialAccidentRateEmployer",
                table: "InsuranceRateSettings");

            migrationBuilder.DropColumn(
                name: "LongTermCareRate",
                table: "InsuranceRateSettings");

            migrationBuilder.DropColumn(
                name: "NationalPensionMinBaseAmount",
                table: "InsuranceRateSettings");

            migrationBuilder.DropColumn(
                name: "NationalPensionMinPremium",
                table: "InsuranceRateSettings");
        }
    }
}

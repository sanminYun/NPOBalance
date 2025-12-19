using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPOBalance.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyPayrollEntryDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseSalary",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "EmployerEmploymentInsuranceSettlement",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "EmployerHealthInsuranceSettlement",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "EmployerLongTermCareSettlement",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "EmployerNationalPensionSettlement",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "EmploymentInsuranceSettlement",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "HealthInsuranceSettlement",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "HolidayAllowance",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "LongTermCareSettlement",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "MaternitySupport",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "MealAllowance",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "MidtermIncomeTaxAdjustment",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "MidtermLocalTaxAdjustment",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "NationalPensionSettlement",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "RetirementDb",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "RetirementDc",
                table: "PayrollEntryDrafts");

            migrationBuilder.RenameColumn(
                name: "YearEndLocalTaxAdjustment",
                table: "PayrollEntryDrafts",
                newName: "PayItemValuesJson");

            migrationBuilder.RenameColumn(
                name: "YearEndIncomeTaxAdjustment",
                table: "PayrollEntryDrafts",
                newName: "FundingSource");

            migrationBuilder.CreateTable(
                name: "PayItemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SectionName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ItemsJson = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayItemSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayItemSettings_SectionName",
                table: "PayItemSettings",
                column: "SectionName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayItemSettings");

            migrationBuilder.RenameColumn(
                name: "PayItemValuesJson",
                table: "PayrollEntryDrafts",
                newName: "YearEndLocalTaxAdjustment");

            migrationBuilder.RenameColumn(
                name: "FundingSource",
                table: "PayrollEntryDrafts",
                newName: "YearEndIncomeTaxAdjustment");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseSalary",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployerEmploymentInsuranceSettlement",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployerHealthInsuranceSettlement",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployerLongTermCareSettlement",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployerNationalPensionSettlement",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmploymentInsuranceSettlement",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HealthInsuranceSettlement",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HolidayAllowance",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LongTermCareSettlement",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaternitySupport",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MealAllowance",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MidtermIncomeTaxAdjustment",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MidtermLocalTaxAdjustment",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NationalPensionSettlement",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RetirementDb",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RetirementDc",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPOBalance.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollEntryDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address1",
                table: "Employees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address2",
                table: "Employees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Employees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Dependents",
                table: "Employees",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedTotalSalary",
                table: "Employees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Employees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidentId",
                table: "Employees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartingSalaryStep",
                table: "Employees",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyCode",
                table: "Companies",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyType",
                table: "Companies",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CorporateRegistrationNumber",
                table: "Companies",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FiscalYearEnd",
                table: "Companies",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FiscalYearStart",
                table: "Companies",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RepresentativeName",
                table: "Companies",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxSource",
                table: "Companies",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PayrollEntryDrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "TEXT", nullable: false),
                    HolidayAllowance = table.Column<decimal>(type: "TEXT", nullable: false),
                    MealAllowance = table.Column<decimal>(type: "TEXT", nullable: false),
                    MaternitySupport = table.Column<decimal>(type: "TEXT", nullable: false),
                    NationalPensionSettlement = table.Column<decimal>(type: "TEXT", nullable: false),
                    HealthInsuranceSettlement = table.Column<decimal>(type: "TEXT", nullable: false),
                    LongTermCareSettlement = table.Column<decimal>(type: "TEXT", nullable: false),
                    EmploymentInsuranceSettlement = table.Column<decimal>(type: "TEXT", nullable: false),
                    MidtermIncomeTaxAdjustment = table.Column<decimal>(type: "TEXT", nullable: false),
                    MidtermLocalTaxAdjustment = table.Column<decimal>(type: "TEXT", nullable: false),
                    YearEndIncomeTaxAdjustment = table.Column<decimal>(type: "TEXT", nullable: false),
                    YearEndLocalTaxAdjustment = table.Column<decimal>(type: "TEXT", nullable: false),
                    EmployerNationalPensionSettlement = table.Column<decimal>(type: "TEXT", nullable: false),
                    EmployerHealthInsuranceSettlement = table.Column<decimal>(type: "TEXT", nullable: false),
                    EmployerLongTermCareSettlement = table.Column<decimal>(type: "TEXT", nullable: false),
                    EmployerEmploymentInsuranceSettlement = table.Column<decimal>(type: "TEXT", nullable: false),
                    RetirementDc = table.Column<decimal>(type: "TEXT", nullable: false),
                    RetirementDb = table.Column<decimal>(type: "TEXT", nullable: false),
                    EstimatedAnnualSalary = table.Column<decimal>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollEntryDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollEntryDrafts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollEntryDrafts_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CompanyCode",
                table: "Companies",
                column: "CompanyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntryDrafts_CompanyId_EmployeeId",
                table: "PayrollEntryDrafts",
                columns: new[] { "CompanyId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntryDrafts_EmployeeId",
                table: "PayrollEntryDrafts",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollEntryDrafts");

            migrationBuilder.DropIndex(
                name: "IX_Companies_CompanyCode",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Address1",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Address2",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Dependents",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EstimatedTotalSalary",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ResidentId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "StartingSalaryStep",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CompanyCode",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CompanyType",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CorporateRegistrationNumber",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "FiscalYearEnd",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "FiscalYearStart",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "RepresentativeName",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "TaxSource",
                table: "Companies");
        }
    }
}

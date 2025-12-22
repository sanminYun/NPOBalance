using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPOBalance.Migrations
{
    /// <inheritdoc />
    public partial class AddAccrualMonthColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayrollEntryDrafts_CompanyId_EmployeeId",
                table: "PayrollEntryDrafts");

            migrationBuilder.AddColumn<int>(
                name: "AccrualMonth",
                table: "PayrollEntryDrafts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AccrualYear",
                table: "PayrollEntryDrafts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntryDrafts_CompanyId_EmployeeId_AccrualYear_AccrualMonth",
                table: "PayrollEntryDrafts",
                columns: new[] { "CompanyId", "EmployeeId", "AccrualYear", "AccrualMonth" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayrollEntryDrafts_CompanyId_EmployeeId_AccrualYear_AccrualMonth",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "AccrualMonth",
                table: "PayrollEntryDrafts");

            migrationBuilder.DropColumn(
                name: "AccrualYear",
                table: "PayrollEntryDrafts");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntryDrafts_CompanyId_EmployeeId",
                table: "PayrollEntryDrafts",
                columns: new[] { "CompanyId", "EmployeeId" },
                unique: true);
        }
    }
}

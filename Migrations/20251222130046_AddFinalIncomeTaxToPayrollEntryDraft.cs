using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPOBalance.Migrations
{
    /// <inheritdoc />
    public partial class AddFinalIncomeTaxToPayrollEntryDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FinalIncomeTax",
                table: "PayrollEntryDrafts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalIncomeTax",
                table: "PayrollEntryDrafts");
        }
    }
}

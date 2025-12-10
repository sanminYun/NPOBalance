namespace NPOBalance.Models;

public class PayrollEntryDraft
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int EmployeeId { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal HolidayAllowance { get; set; }
    public decimal MealAllowance { get; set; }
    public decimal MaternitySupport { get; set; }
    public decimal NationalPensionSettlement { get; set; }
    public decimal HealthInsuranceSettlement { get; set; }
    public decimal LongTermCareSettlement { get; set; }
    public decimal EmploymentInsuranceSettlement { get; set; }
    public decimal MidtermIncomeTaxAdjustment { get; set; }
    public decimal MidtermLocalTaxAdjustment { get; set; }
    public decimal YearEndIncomeTaxAdjustment { get; set; }
    public decimal YearEndLocalTaxAdjustment { get; set; }
    public decimal EmployerNationalPensionSettlement { get; set; }
    public decimal EmployerHealthInsuranceSettlement { get; set; }
    public decimal EmployerLongTermCareSettlement { get; set; }
    public decimal EmployerEmploymentInsuranceSettlement { get; set; }
    public decimal RetirementDc { get; set; }
    public decimal RetirementDb { get; set; }
    public decimal EstimatedAnnualSalary { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Company Company { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}
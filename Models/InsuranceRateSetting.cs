namespace NPOBalance.Models;

public class InsuranceRateSetting
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public decimal NationalPensionRateEmployee { get; set; }
    public decimal NationalPensionRateEmployer { get; set; }
    public decimal HealthInsuranceRateEmployee { get; set; }
    public decimal HealthInsuranceRateEmployer { get; set; }
    public decimal EmploymentInsuranceRateEmployee { get; set; }
    public decimal EmploymentInsuranceRateEmployer { get; set; }
    public string RoundingRule { get; set; } = "Round";
    public int RoundingDigit { get; set; } = 1;
    public decimal? MonthlyIncomeMin { get; set; }
    public decimal? MonthlyIncomeMax { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    // Navigation properties
    public Company Company { get; set; } = null!;
}
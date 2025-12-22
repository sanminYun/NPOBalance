namespace NPOBalance.Models;

public class PayrollEntryDraft
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int EmployeeId { get; set; }
    public int AccrualYear { get; set; }
    public int AccrualMonth { get; set; }
    public string FundingSource { get; set; } = string.Empty;
    public string PayItemValuesJson { get; set; } = string.Empty;
    public decimal EstimatedAnnualSalary { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal FinalIncomeTax { get; set; }

    public Company Company { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}
namespace NPOBalance.Models;

public class Company
{
    public int Id { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? BusinessNumber { get; set; }
    public string? CorporateRegistrationNumber { get; set; }
    public string? RepresentativeName { get; set; }
    public DateTime FiscalYearStart { get; set; }
    public DateTime FiscalYearEnd { get; set; }
    public string CompanyType { get; set; } = string.Empty; // 구분
    public string TaxSource { get; set; } = string.Empty; // 원천
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<PayItemType> PayItemTypes { get; set; } = new List<PayItemType>();
    public ICollection<PayrollHeader> PayrollHeaders { get; set; } = new List<PayrollHeader>();
    public ICollection<InsuranceRateSetting> InsuranceRateSettings { get; set; } = new List<InsuranceRateSetting>();
    public ICollection<PayrollEntryDraft> PayrollEntryDrafts { get; set; } = new List<PayrollEntryDraft>();
}
namespace NPOBalance.Models;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? BusinessNumber { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<PayItemType> PayItemTypes { get; set; } = new List<PayItemType>();
    public ICollection<PayrollHeader> PayrollHeaders { get; set; } = new List<PayrollHeader>();
    public ICollection<InsuranceRateSetting> InsuranceRateSettings { get; set; } = new List<InsuranceRateSetting>();
}
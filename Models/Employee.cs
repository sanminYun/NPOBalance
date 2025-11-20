namespace NPOBalance.Models;

public class Employee
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? EmploymentStartDate { get; set; }
    public DateTime? EmploymentEndDate { get; set; }

    // Navigation properties
    public Company Company { get; set; } = null!;
    public ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
}
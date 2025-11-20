namespace NPOBalance.Models;

public class PayrollHeader
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int RunNumber { get; set; }
    public DateTime PayDate { get; set; }
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public Company Company { get; set; } = null!;
    public ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
}
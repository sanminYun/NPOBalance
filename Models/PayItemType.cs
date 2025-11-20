namespace NPOBalance.Models;

public class PayItemType
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsTaxable { get; set; }
    public bool IsEarning { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation properties
    public Company Company { get; set; } = null!;
    public ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
}
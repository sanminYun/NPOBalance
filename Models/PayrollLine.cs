namespace NPOBalance.Models;

public class PayrollLine
{
    public int Id { get; set; }
    public int PayrollHeaderId { get; set; }
    public int EmployeeId { get; set; }
    public int PayItemTypeId { get; set; }
    public decimal Amount { get; set; }
    public bool IsAutoCalculated { get; set; }

    // Navigation properties
    public PayrollHeader PayrollHeader { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public PayItemType PayItemType { get; set; } = null!;
}
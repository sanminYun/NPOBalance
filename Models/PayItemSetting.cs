namespace NPOBalance.Models;

public class PayItemSetting
{
    public int Id { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public string ItemsJson { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
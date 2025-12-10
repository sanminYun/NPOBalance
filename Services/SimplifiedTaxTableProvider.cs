using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NPOBalance.Services;

public class SimplifiedTaxTableProvider
{
    private readonly IReadOnlyList<TaxBracket> _brackets;
    private static readonly string TaxTablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "TaxTables", "simplified_tax_table.json");

    public SimplifiedTaxTableProvider()
    {
        _brackets = LoadBrackets();
    }

    public decimal GetWithholdingTax(decimal estimatedAnnualSalary)
    {
        if (estimatedAnnualSalary <= 0)
        {
            return 0m;
        }

        decimal monthlyIncome = estimatedAnnualSalary / 12m;
        var bracket = _brackets.FirstOrDefault(b => monthlyIncome >= b.MinMonthlyIncome && monthlyIncome <= b.MaxMonthlyIncome);
        if (bracket != null)
        {
            return bracket.WithholdingTax;
        }

        // fallback to 3.5% if out of range
        return Math.Round(monthlyIncome * 0.035m, 0, MidpointRounding.AwayFromZero);
    }

    private static IReadOnlyList<TaxBracket> LoadBrackets()
    {
        try
        {
            if (File.Exists(TaxTablePath))
            {
                using var stream = File.OpenRead(TaxTablePath);
                var brackets = JsonSerializer.Deserialize<List<TaxBracket>>(stream);
                if (brackets != null && brackets.Count > 0)
                {
                    return brackets
                        .OrderBy(b => b.MinMonthlyIncome)
                        .ToList();
                }
            }
        }
        catch
        {
            // ignored - fallback will be used
        }

        return GetFallback();
    }

    private static IReadOnlyList<TaxBracket> GetFallback()
    {
        return new List<TaxBracket>
        {
            new() { MinMonthlyIncome = 0m,        MaxMonthlyIncome = 2000000m, WithholdingTax = 0m },
            new() { MinMonthlyIncome = 2000000m,  MaxMonthlyIncome = 3000000m, WithholdingTax = 5000m },
            new() { MinMonthlyIncome = 3000000m,  MaxMonthlyIncome = 4000000m, WithholdingTax = 15000m },
            new() { MinMonthlyIncome = 4000000m,  MaxMonthlyIncome = 5000000m, WithholdingTax = 35000m },
            new() { MinMonthlyIncome = 5000000m,  MaxMonthlyIncome = 6000000m, WithholdingTax = 55000m },
            new() { MinMonthlyIncome = 6000000m,  MaxMonthlyIncome = 8000000m, WithholdingTax = 90000m },
            new() { MinMonthlyIncome = 8000000m,  MaxMonthlyIncome = decimal.MaxValue, WithholdingTax = 140000m }
        };
    }

    private sealed class TaxBracket
    {
        public decimal MinMonthlyIncome { get; set; }
        public decimal MaxMonthlyIncome { get; set; }
        public decimal WithholdingTax { get; set; }
    }
}

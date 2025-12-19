using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NPOBalance.Services;

public class SimplifiedTaxTableProvider
{
    private readonly IReadOnlyList<TaxBracket> _brackets;
    private static readonly string TaxTablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "TaxTables", "withholding_table_full.json");

    // 10,000천원 기준 세액 (부양가족수별)
    private static readonly decimal[] BaseTaxAt10M = new decimal[]
    {
        1552400m, 1476570m, 1245840m, 1215840m, 1185840m, 1155840m,
        1125840m, 1095840m, 1065840m, 1035840m, 1005840m
    };

    public SimplifiedTaxTableProvider()
    {
        _brackets = LoadBrackets();
        System.Diagnostics.Debug.WriteLine($"[SimplifiedTaxTableProvider] Loaded {_brackets.Count} tax brackets");
    }

    /// <summary>
    /// 예상 연봉과 부양가족수를 기반으로 월 소득세를 계산합니다.
    /// </summary>
    /// <param name="estimatedAnnualSalary">예상 연봉 (원)</param>
    /// <param name="dependents">부양가족수 (본인 포함, 1~11명)</param>
    /// <returns>월 소득세 (원)</returns>
    public decimal GetWithholdingTax(decimal estimatedAnnualSalary, int dependents = 1)
    {
        if (estimatedAnnualSalary <= 0)
        {
            return 0m;
        }

        // 부양가족수 범위 검증 (1~11명)
        int validDependents = Math.Max(1, Math.Min(11, dependents));
        int arrayIndex = validDependents - 1; // 배열 인덱스는 0부터 시작

        decimal monthlyIncome = estimatedAnnualSalary / 12m;

        // 10,000천원(10,000,000원) 초과 구간 처리
        if (monthlyIncome > 10000000m)
        {
            return CalculateHighIncomeTax(monthlyIncome, arrayIndex);
        }

        // 월급여 구간에 해당하는 bracket 찾기
        var bracket = _brackets.FirstOrDefault(b => 
            monthlyIncome >= b.MinMonthlyIncome && 
            monthlyIncome < b.MaxMonthlyIncome);

        if (bracket != null && bracket.WithholdingTax != null && arrayIndex < bracket.WithholdingTax.Count)
        {
            return bracket.WithholdingTax[arrayIndex];
        }

        // fallback: 범위를 벗어난 경우 3.5% 적용
        System.Diagnostics.Debug.WriteLine($"[WARNING] No bracket found for monthly income: {monthlyIncome:N0}원, using fallback 3.5%");
        return Math.Round(monthlyIncome * 0.035m, 0, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 부양가족수 없이 호출 시 기본값 1명 적용
    /// </summary>
    public decimal GetWithholdingTax(decimal estimatedAnnualSalary)
    {
        return GetWithholdingTax(estimatedAnnualSalary, 1);
    }

    /// <summary>
    /// 10,000천원 초과 고소득 구간 세액 계산
    /// </summary>
    private decimal CalculateHighIncomeTax(decimal monthlyIncome, int arrayIndex)
    {
        decimal baseTax = BaseTaxAt10M[arrayIndex];
        decimal excessAmount;
        decimal additionalTax;

        if (monthlyIncome <= 14000000m) // 10,000천원 초과 ~ 14,000천원 이하
        {
            excessAmount = monthlyIncome - 10000000m;
            additionalTax = excessAmount * 0.98m * 0.35m;
            return Math.Round(baseTax + additionalTax, 0, MidpointRounding.AwayFromZero);
        }
        else if (monthlyIncome <= 28000000m) // 14,000천원 초과 ~ 28,000천원 이하
        {
            excessAmount = monthlyIncome - 14000000m;
            additionalTax = 1372000m + (excessAmount * 0.98m * 0.38m);
            return Math.Round(baseTax + additionalTax, 0, MidpointRounding.AwayFromZero);
        }
        else if (monthlyIncome <= 30000000m) // 28,000천원 초과 ~ 30,000천원 이하
        {
            excessAmount = monthlyIncome - 28000000m;
            additionalTax = 6585600m + (excessAmount * 0.98m * 0.40m);
            return Math.Round(baseTax + additionalTax, 0, MidpointRounding.AwayFromZero);
        }
        else if (monthlyIncome <= 45000000m) // 30,000천원 초과 ~ 45,000천원 이하
        {
            excessAmount = monthlyIncome - 30000000m;
            additionalTax = 7369600m + (excessAmount * 0.40m);
            return Math.Round(baseTax + additionalTax, 0, MidpointRounding.AwayFromZero);
        }
        else if (monthlyIncome <= 87000000m) // 45,000천원 초과 ~ 87,000천원 이하
        {
            excessAmount = monthlyIncome - 45000000m;
            additionalTax = 13369600m + (excessAmount * 0.42m);
            return Math.Round(baseTax + additionalTax, 0, MidpointRounding.AwayFromZero);
        }
        else // 87,000천원 초과
        {
            excessAmount = monthlyIncome - 87000000m;
            additionalTax = 31009600m + (excessAmount * 0.45m);
            return Math.Round(baseTax + additionalTax, 0, MidpointRounding.AwayFromZero);
        }
    }

    private static IReadOnlyList<TaxBracket> LoadBrackets()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[SimplifiedTaxTableProvider] Attempting to load from: {TaxTablePath}");
            System.Diagnostics.Debug.WriteLine($"[SimplifiedTaxTableProvider] File exists: {File.Exists(TaxTablePath)}");

            if (File.Exists(TaxTablePath))
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                string jsonContent = File.ReadAllText(TaxTablePath);
                System.Diagnostics.Debug.WriteLine($"[SimplifiedTaxTableProvider] JSON file size: {jsonContent.Length} characters");

                var brackets = JsonSerializer.Deserialize<List<TaxBracket>>(jsonContent, jsonOptions);
                
                if (brackets != null && brackets.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[SimplifiedTaxTableProvider] Successfully loaded {brackets.Count} brackets from JSON");
                    return brackets
                        .OrderBy(b => b.MinMonthlyIncome)
                        .ToList();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[SimplifiedTaxTableProvider] JSON deserialized but brackets list is empty or null");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SimplifiedTaxTableProvider] File not found, using fallback data");
            }
        }
        catch (Exception ex)
        {
            // 로딩 실패 시 상세 로그 출력
            System.Diagnostics.Debug.WriteLine($"[SimplifiedTaxTableProvider] ERROR loading tax table: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"[SimplifiedTaxTableProvider] ERROR message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SimplifiedTaxTableProvider] ERROR stack: {ex.StackTrace}");
        }

        System.Diagnostics.Debug.WriteLine("[SimplifiedTaxTableProvider] Using fallback data");
        return GetFallback();
    }

    private static IReadOnlyList<TaxBracket> GetFallback()
    {
        // 기본 fallback 데이터 (부양가족 1명 기준)
        return new List<TaxBracket>
        {
            new() { 
                MinMonthlyIncome = 0m, 
                MaxMonthlyIncome = 1060000m, 
                WithholdingTax = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new() { 
                MinMonthlyIncome = 2000000m, 
                MaxMonthlyIncome = 3000000m, 
                WithholdingTax = new List<decimal> { 19200, 14540, 6400, 3020, 0, 0, 0, 0, 0, 0, 0 }
            },
            new() { 
                MinMonthlyIncome = 3000000m, 
                MaxMonthlyIncome = 4000000m, 
                WithholdingTax = new List<decimal> { 84850, 67350, 32490, 26690, 21440, 17100, 13730, 10350, 6980, 3600, 0 }
            },
            new() { 
                MinMonthlyIncome = 4000000m, 
                MaxMonthlyIncome = 5000000m, 
                WithholdingTax = new List<decimal> { 210960, 182950, 124590, 105840, 89050, 75920, 62800, 49670, 36550, 28320, 23070 }
            },
            new() { 
                MinMonthlyIncome = 5000000m, 
                MaxMonthlyIncome = decimal.MaxValue, 
                WithholdingTax = new List<decimal> { 364490, 335660, 265750, 247000, 228250, 209500, 190750, 172000, 153250, 134500, 115750 }
            }
        };
    }

    private sealed class TaxBracket
    {
        public decimal MinMonthlyIncome { get; set; }
        public decimal MaxMonthlyIncome { get; set; }
        public List<decimal> WithholdingTax { get; set; } = new();
    }
}

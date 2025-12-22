namespace NPOBalance.Models;

public class InsuranceRateSetting
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    
    // 요율 (소수점으로 저장, 화면에서는 %로 표시)
    public decimal NationalPensionRateEmployee { get; set; }
    public decimal NationalPensionRateEmployer { get; set; }
    public decimal HealthInsuranceRateEmployee { get; set; }
    public decimal HealthInsuranceRateEmployer { get; set; }
    public decimal LongTermCareRateEmployee { get; set; } // 추가
    public decimal LongTermCareRateEmployer { get; set; } // 추가
    public decimal EmploymentInsuranceRateEmployee { get; set; }
    public decimal EmploymentInsuranceRateEmployer { get; set; }
    public decimal IndustrialAccidentRateEmployee { get; set; } // 추가
    public decimal IndustrialAccidentRateEmployer { get; set; }
    
    // 최저한도 - 기준금액
    public decimal NationalPensionMinBaseAmount { get; set; }
    public decimal HealthInsuranceMinBaseAmount { get; set; }
    public decimal LongTermCareMinBaseAmount { get; set; } // 추가
    public decimal EmploymentInsuranceMinBaseAmount { get; set; }
    public decimal IndustrialAccidentMinBaseAmount { get; set; } // 추가
    
    // 최저한도 - 보험료
    public decimal NationalPensionMinPremium { get; set; }
    public decimal HealthInsuranceMinPremium { get; set; }
    public decimal LongTermCareMinPremium { get; set; } // 추가
    public decimal EmploymentInsuranceMinPremium { get; set; }
    public decimal IndustrialAccidentMinPremium { get; set; } // 추가
    
    public string RoundingRule { get; set; } = "Round";
    public int RoundingDigit { get; set; } = 1;
    public decimal? MonthlyIncomeMin { get; set; }
    public decimal? MonthlyIncomeMax { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    // Navigation properties
    public Company Company { get; set; } = null!;
}
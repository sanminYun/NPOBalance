namespace NPOBalance.ViewModels;

/// <summary>
/// 4대보험 요율 기본값 (Fallback)
/// 실제 사용 시에는 InsuranceRateSetting 테이블의 값을 우선 사용
/// </summary>
internal static class InsuranceRateDefaults
{
    // 국민연금 - 근로자/사업자 각 4.50%
    public const decimal NationalPensionEmployeeRate = 0.045m;
    public const decimal NationalPensionEmployerRate = 0.045m;

    // 건강보험 - 근로자/사업자 각 3.545%
    public const decimal HealthInsuranceEmployeeRate = 0.03545m;
    public const decimal HealthInsuranceEmployerRate = 0.03545m;

    // 장기요양보험 - 근로자/사업자 각 건강보험료의 6.135%
    public const decimal LongTermCareRateEmployee = 0.06135m;
    public const decimal LongTermCareRateEmployer = 0.06135m;

    // 고용보험 - 근로자 0.90%, 사업자 1.15%
    public const decimal EmploymentInsuranceEmployeeRate = 0.009m;
    public const decimal EmploymentInsuranceEmployerRate = 0.0115m;

    // 산재보험 - 근로자 0%, 사업자 7.26%
    public const decimal IndustrialAccidentEmployeeRate = 0.0m;
    public const decimal IndustrialAccidentEmployerRate = 0.0726m;

    // 최저한도 기본값 - 기준금액
    public const decimal NationalPensionMinBaseAmount = 400000m;
    public const decimal HealthInsuranceMinBaseAmount = 279266m;
    public const decimal LongTermCareMinBaseAmount = 0m;
    public const decimal EmploymentInsuranceMinBaseAmount = 400000m;
    public const decimal IndustrialAccidentMinBaseAmount = 0m;

    // 최저한도 기본값 - 보험료
    public const decimal NationalPensionMinPremium = 36000m;
    public const decimal HealthInsuranceMinPremium = 19780m;
    public const decimal LongTermCareMinPremium = 0m;
    public const decimal EmploymentInsuranceMinPremium = 18000m;
    public const decimal IndustrialAccidentMinPremium = 0m;
}
namespace NPOBalance.ViewModels;

/// <summary>
/// 급여 계산에 사용되는 4대보험 요율 설정
/// </summary>
internal static class PayrollRateConfig
{
    // 국민연금 - 근로자/사업자 각 4.50%
    public const decimal NationalPensionEmployeeRate = 0.045m;
    public const decimal NationalPensionEmployerRate = 0.045m;

    // 건강보험 - 근로자/사업자 각 3.545%
    public const decimal HealthInsuranceEmployeeRate = 0.03545m;
    public const decimal HealthInsuranceEmployerRate = 0.03545m;

    // 장기요양보험 - 건강보험료의 6.135% (건강보험료에 곱함)
    public const decimal LongTermCareRate = 0.06135m;

    // 고용보험 - 근로자 0.90%, 사업자 1.15%
    public const decimal EmploymentInsuranceEmployeeRate = 0.009m;
    public const decimal EmploymentInsuranceEmployerRate = 0.0115m;

    // 산재보험 - 사업자만 7.26%
    public const decimal IndustrialAccidentEmployerRate = 0.0726m;
}
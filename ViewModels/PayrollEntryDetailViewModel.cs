using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NPOBalance.Models;
using NPOBalance.Services;

namespace NPOBalance.ViewModels;

public class PayrollEntryDetailViewModel : ObservableObject
{
    private readonly SimplifiedTaxTableProvider _taxTableProvider;
    private readonly PayItemService _payItemService;
    private Dictionary<string, List<decimal>> _sectionValues = new();
    private decimal _estimatedAnnualSalary;
    private int _dependents = 1;

    public PayrollEntryDetailViewModel(SimplifiedTaxTableProvider taxTableProvider, PayItemService payItemService)
    {
        _taxTableProvider = taxTableProvider;
        _payItemService = payItemService;
    }

    public decimal EstimatedAnnualSalary
    {
        get => _estimatedAnnualSalary;
        private set
        {
            if (SetProperty(ref _estimatedAnnualSalary, value))
            {
                RecalculateTax();
            }
        }
    }

    public int Dependents
    {
        get => _dependents;
        private set
        {
            if (SetProperty(ref _dependents, value))
            {
                RecalculateTax();
            }
        }
    }

    // 섹션의 특정 인덱스 값 가져오기
    public decimal GetValue(string sectionKey, int index)
    {
        if (_sectionValues.TryGetValue(sectionKey, out var values) && index < values.Count)
        {
            return values[index];
        }
        return 0m;
    }

    // 섹션의 특정 인덱스 값 설정
    public void SetValue(string sectionKey, int index, decimal value)
    {
        if (!_sectionValues.ContainsKey(sectionKey))
        {
            _sectionValues[sectionKey] = new List<decimal>();
        }

        var values = _sectionValues[sectionKey];
        while (values.Count <= index)
        {
            values.Add(0m);
        }

        values[index] = value;
        RecalculateAll();
    }

    // 각 섹션의 소계
    public decimal GetSectionSubtotal(string sectionKey)
    {
        if (_sectionValues.TryGetValue(sectionKey, out var values))
        {
            return values.Sum();
        }
        return 0m;
    }

    // 주요 계산 프로퍼티들
    public decimal TaxableEarningsSubtotal => GetSectionSubtotal(PayItemService.TaxableEarnings);
    public decimal NonTaxableEarningsSubtotal => GetSectionSubtotal(PayItemService.NonTaxableEarnings);

    // 자동 계산 4대보험 (본인부담)
    public decimal EmployeeNationalPension => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.NationalPensionEmployeeRate);
    public decimal EmployeeHealthInsurance => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.HealthInsuranceEmployeeRate);
    public decimal EmployeeLongTermCare => RoundDown(EmployeeHealthInsurance * PayrollRateConfig.LongTermCareRate);
    public decimal EmployeeEmploymentInsurance => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.EmploymentInsuranceEmployeeRate);

    public decimal InsuranceDeductionSubtotal =>
        EmployeeNationalPension + EmployeeHealthInsurance + EmployeeLongTermCare + EmployeeEmploymentInsurance +
        GetSectionSubtotal(PayItemService.InsuranceDeduction);

    // 자동 계산 소득세 - 예상 총 급여 및 부양가족수 기반으로 계산
    public decimal IncomeTax => _taxTableProvider.GetWithholdingTax(EstimatedAnnualSalary, Dependents);
    public decimal LocalIncomeTax => RoundDown(IncomeTax * 0.1m);

    public decimal IncomeTaxSubtotal =>
        IncomeTax + LocalIncomeTax +
        GetSectionSubtotal(PayItemService.IncomeTaxDeduction);

    // 자동 계산 4대보험 (기업부담)
    public decimal EmployerNationalPension => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.NationalPensionEmployerRate);
    public decimal EmployerHealthInsurance => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.HealthInsuranceEmployerRate);
    public decimal EmployerLongTermCare => RoundDown(EmployerHealthInsurance * PayrollRateConfig.LongTermCareRate);
    public decimal EmployerEmploymentInsurance => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.EmploymentInsuranceEmployerRate);
    public decimal IndustrialAccidentInsurance => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.IndustrialAccidentEmployerRate);

    public decimal EmployerInsuranceSubtotal =>
        EmployerNationalPension + EmployerHealthInsurance + EmployerLongTermCare +
        EmployerEmploymentInsurance + IndustrialAccidentInsurance +
        GetSectionSubtotal(PayItemService.EmployerInsurance);

    public decimal RetirementSubtotal => GetSectionSubtotal(PayItemService.Retirement);

    // 최종 계산
    public decimal NetPay => TaxableEarningsSubtotal + NonTaxableEarningsSubtotal - InsuranceDeductionSubtotal - IncomeTaxSubtotal;
    public decimal EmployerTotalBurden => NetPay + EmployerInsuranceSubtotal + RetirementSubtotal;

    public void ApplyEmployeeContext(Employee employee)
    {
        var annualEstimate = employee.EstimatedTotalSalary ?? 0m;
        var dependents = employee.Dependents ?? 1;

        EstimatedAnnualSalary = annualEstimate;
        Dependents = dependents;
    }

    public async Task LoadFromDraftAsync(PayrollEntryDraft draft)
    {
        // Draft에서 EstimatedAnnualSalary 로드 (Draft에 저장된 값 우선)
        EstimatedAnnualSalary = draft.EstimatedAnnualSalary;

        if (string.IsNullOrWhiteSpace(draft.PayItemValuesJson) || draft.PayItemValuesJson == "{}")
        {
            _sectionValues = new Dictionary<string, List<decimal>>();
            RecalculateAll();
            return;
        }

        try
        {
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, List<decimal>>>(draft.PayItemValuesJson);
            _sectionValues = jsonData ?? new Dictionary<string, List<decimal>>();
        }
        catch
        {
            _sectionValues = new Dictionary<string, List<decimal>>();
        }

        RecalculateAll();
    }

    public async Task CopyToDraftAsync(PayrollEntryDraft draft)
    {
        draft.EstimatedAnnualSalary = EstimatedAnnualSalary;
        draft.PayItemValuesJson = JsonSerializer.Serialize(_sectionValues);
        draft.UpdatedAt = DateTime.UtcNow;
    }

    public async Task ResetAsync()
    {
        _sectionValues = new Dictionary<string, List<decimal>>();
        EstimatedAnnualSalary = 0;
        Dependents = 1;
        RecalculateAll();
    }

    private void RecalculateAll()
    {
        OnPropertyChanged(nameof(TaxableEarningsSubtotal));
        OnPropertyChanged(nameof(NonTaxableEarningsSubtotal));
        OnPropertyChanged(nameof(EmployeeNationalPension));
        OnPropertyChanged(nameof(EmployeeHealthInsurance));
        OnPropertyChanged(nameof(EmployeeLongTermCare));
        OnPropertyChanged(nameof(EmployeeEmploymentInsurance));
        OnPropertyChanged(nameof(InsuranceDeductionSubtotal));
        RecalculateTax();
        OnPropertyChanged(nameof(EmployerNationalPension));
        OnPropertyChanged(nameof(EmployerHealthInsurance));
        OnPropertyChanged(nameof(EmployerLongTermCare));
        OnPropertyChanged(nameof(EmployerEmploymentInsurance));
        OnPropertyChanged(nameof(IndustrialAccidentInsurance));
        OnPropertyChanged(nameof(EmployerInsuranceSubtotal));
        OnPropertyChanged(nameof(RetirementSubtotal));
        OnPropertyChanged(nameof(NetPay));
        OnPropertyChanged(nameof(EmployerTotalBurden));
    }

    private void RecalculateTax()
    {
        OnPropertyChanged(nameof(IncomeTax));
        OnPropertyChanged(nameof(LocalIncomeTax));
        OnPropertyChanged(nameof(IncomeTaxSubtotal));
    }

    private static decimal RoundDown(decimal value)
    {
        return value >= 0 ? Math.Floor(value) : Math.Ceiling(value);
    }
}
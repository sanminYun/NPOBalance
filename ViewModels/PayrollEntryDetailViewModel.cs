using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NPOBalance.Data;
using NPOBalance.Models;
using NPOBalance.Services;
using Microsoft.EntityFrameworkCore;

namespace NPOBalance.ViewModels;

public class PayrollEntryDetailViewModel : ObservableObject
{
    private readonly SimplifiedTaxTableProvider _taxTableProvider;
    private readonly PayItemService _payItemService;
    private Dictionary<string, List<decimal>> _sectionValues = new();
    private decimal _estimatedAnnualSalary;
    private int _dependents = 1;
    private decimal _finalIncomeTax;
    private InsuranceRateSetting? _insuranceRateSetting;
    private Company? _currentCompany;

    private decimal InsurableEarningsSubtotal => TaxableEarningsSubtotal + NonTaxableEarningsSubtotal;
    private decimal EmployeeHealthInsuranceBase =>
        TaxableEarningsSubtotal *
        (_insuranceRateSetting?.HealthInsuranceRateEmployee ?? InsuranceRateDefaults.HealthInsuranceEmployeeRate);
    private decimal EmployerHealthInsuranceBase =>
        TaxableEarningsSubtotal *
        (_insuranceRateSetting?.HealthInsuranceRateEmployer ?? InsuranceRateDefaults.HealthInsuranceEmployerRate);

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
                OnPropertyChanged(nameof(IncomeTaxEstimated));
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
                OnPropertyChanged(nameof(IncomeTaxEstimated));
                OnPropertyChanged(nameof(IncomeTaxTaxable));
                RecalculateTax();
            }
        }
    }

    public decimal FinalIncomeTax
    {
        get => _finalIncomeTax;
        set
        {
            if (SetProperty(ref _finalIncomeTax, value))
            {
                RecalculateTax();
            }
        }
    }

    public async Task SetCompanyAsync(Company company)
    {
        _currentCompany = company;
        await LoadInsuranceRateSettingAsync().ConfigureAwait(false);
    }

    private async Task LoadInsuranceRateSettingAsync()
    {
        if (_currentCompany == null)
        {
            return;
        }

        using var context = new AccountingDbContext();
        _insuranceRateSetting = await context.InsuranceRateSettings
            .Where(s => s.CompanyId == _currentCompany.Id && s.EffectiveTo == null)
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        RecalculateAll();
    }

    public decimal GetValue(string sectionKey, int index)
    {
        if (_sectionValues.TryGetValue(sectionKey, out var values) && index < values.Count)
        {
            return values[index];
        }

        return 0m;
    }

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

    public decimal GetSectionSubtotal(string sectionKey)
    {
        if (_sectionValues.TryGetValue(sectionKey, out var values))
        {
            return values.Sum();
        }

        return 0m;
    }

    public decimal TaxableEarningsSubtotal => GetSectionSubtotal(PayItemService.TaxableEarnings);
    public decimal NonTaxableEarningsSubtotal => GetSectionSubtotal(PayItemService.NonTaxableEarnings);

    public decimal EmployeeNationalPension => RoundDown(TaxableEarningsSubtotal *
        (_insuranceRateSetting?.NationalPensionRateEmployee ?? InsuranceRateDefaults.NationalPensionEmployeeRate));

    public decimal EmployeeHealthInsurance => RoundDown(EmployeeHealthInsuranceBase);

    public decimal EmployeeLongTermCare => RoundDown(TaxableEarningsSubtotal *
        (_insuranceRateSetting?.LongTermCareRateEmployee ?? InsuranceRateDefaults.LongTermCareRateEmployee));

    public decimal EmployeeEmploymentInsurance => RoundDown(TaxableEarningsSubtotal *
        (_insuranceRateSetting?.EmploymentInsuranceRateEmployee ?? InsuranceRateDefaults.EmploymentInsuranceEmployeeRate));

    public decimal InsuranceDeductionSubtotal =>
        EmployeeNationalPension + EmployeeHealthInsurance + EmployeeLongTermCare + EmployeeEmploymentInsurance +
        GetSectionSubtotal(PayItemService.InsuranceDeduction);

    // 소득세(예상 총 급여) - 예상 총 급여를 연간 금액으로 전달
    public decimal IncomeTaxEstimated => _taxTableProvider.GetWithholdingTax(EstimatedAnnualSalary, Dependents);
    
    // 소득세(과세급상여) - 과세급상여 소계(A)를 월급여로 간주하여 연간으로 환산하여 계산
    public decimal IncomeTaxTaxable => _taxTableProvider.GetWithholdingTax(TaxableEarningsSubtotal * 12m, Dependents);

    // 기존 IncomeTax는 FinalIncomeTax를 반환
    public decimal IncomeTax => FinalIncomeTax;
    
    public decimal LocalIncomeTax => RoundDown(IncomeTax * 0.1m);

    public decimal IncomeTaxSubtotal =>
        IncomeTax + LocalIncomeTax +
        GetSectionSubtotal(PayItemService.IncomeTaxDeduction);

    public decimal EmployerNationalPension => RoundDown(TaxableEarningsSubtotal *
        (_insuranceRateSetting?.NationalPensionRateEmployer ?? InsuranceRateDefaults.NationalPensionEmployerRate));

    public decimal EmployerHealthInsurance => RoundDown(EmployerHealthInsuranceBase);

    public decimal EmployerLongTermCare => RoundDown(TaxableEarningsSubtotal *
        (_insuranceRateSetting?.LongTermCareRateEmployer ?? InsuranceRateDefaults.LongTermCareRateEmployer));

    public decimal EmployerEmploymentInsurance => RoundDown(TaxableEarningsSubtotal *
        (_insuranceRateSetting?.EmploymentInsuranceRateEmployer ?? InsuranceRateDefaults.EmploymentInsuranceEmployerRate));

    public decimal IndustrialAccidentInsurance => RoundDown(InsurableEarningsSubtotal *
        (_insuranceRateSetting?.IndustrialAccidentRateEmployer ?? InsuranceRateDefaults.IndustrialAccidentEmployerRate));

    public decimal EmployerInsuranceSubtotal =>
        EmployerNationalPension + EmployerHealthInsurance + EmployerLongTermCare +
        EmployerEmploymentInsurance + IndustrialAccidentInsurance +
        GetSectionSubtotal(PayItemService.EmployerInsurance);

    public decimal RetirementSubtotal => GetSectionSubtotal(PayItemService.Retirement);

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
        EstimatedAnnualSalary = draft.EstimatedAnnualSalary;
        FinalIncomeTax = draft.FinalIncomeTax;

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

    public Task CopyToDraftAsync(PayrollEntryDraft draft)
    {
        draft.EstimatedAnnualSalary = EstimatedAnnualSalary;
        draft.FinalIncomeTax = FinalIncomeTax;
        draft.PayItemValuesJson = JsonSerializer.Serialize(_sectionValues);
        draft.UpdatedAt = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task ResetAsync()
    {
        _sectionValues = new Dictionary<string, List<decimal>>();
        EstimatedAnnualSalary = 0;
        Dependents = 1;
        FinalIncomeTax = 0;
        RecalculateAll();
        return Task.CompletedTask;
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
        OnPropertyChanged(nameof(IncomeTaxTaxable)); // 과세급상여 소득세 재계산
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
        OnPropertyChanged(nameof(IncomeTaxEstimated));
        OnPropertyChanged(nameof(IncomeTaxTaxable));
        OnPropertyChanged(nameof(IncomeTax));
        OnPropertyChanged(nameof(LocalIncomeTax));
        OnPropertyChanged(nameof(IncomeTaxSubtotal));
        OnPropertyChanged(nameof(NetPay)); // NetPay도 소득세에 영향받으므로 재계산
    }

    private static decimal RoundDown(decimal value)
    {
        return value >= 0 ? Math.Floor(value) : Math.Ceiling(value);
    }
}
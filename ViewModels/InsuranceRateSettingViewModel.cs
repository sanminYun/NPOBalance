using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NPOBalance.Data;
using NPOBalance.Models;
using Microsoft.EntityFrameworkCore;

namespace NPOBalance.ViewModels;

public class InsuranceRateSettingViewModel : INotifyPropertyChanged
{
    private InsuranceRateSetting? _currentSetting;
    private Company? _currentCompany;

    public event PropertyChangedEventHandler? PropertyChanged;

    private static decimal RoundPercent(decimal value)
    {
        return decimal.Round(value, 3, MidpointRounding.AwayFromZero);
    }

    private decimal GetRatePercent(Func<InsuranceRateSetting, decimal> selector, decimal fallback)
    {
        var baseValue = (_currentSetting != null ? selector(_currentSetting) : fallback) * 100m;
        return RoundPercent(baseValue);
    }

    private void SetRatePercent(Action<InsuranceRateSetting, decimal> setter, decimal percent, [CallerMemberName] string? propertyName = null)
    {
        if (_currentSetting == null)
        {
            return;
        }

        var roundedPercent = RoundPercent(percent);
        setter(_currentSetting, roundedPercent / 100m);
        OnPropertyChanged(propertyName);
    }

    // 국민연금 요율 (%)
    public decimal NationalPensionEmployeeRate
    {
        get => GetRatePercent(s => s.NationalPensionRateEmployee, InsuranceRateDefaults.NationalPensionEmployeeRate);
        set => SetRatePercent((s, v) => s.NationalPensionRateEmployee = v, value);
    }

    public decimal NationalPensionEmployerRate
    {
        get => GetRatePercent(s => s.NationalPensionRateEmployer, InsuranceRateDefaults.NationalPensionEmployerRate);
        set => SetRatePercent((s, v) => s.NationalPensionRateEmployer = v, value);
    }

    // 건강보험 요율 (%)
    public decimal HealthInsuranceEmployeeRate
    {
        get => GetRatePercent(s => s.HealthInsuranceRateEmployee, InsuranceRateDefaults.HealthInsuranceEmployeeRate);
        set => SetRatePercent((s, v) => s.HealthInsuranceRateEmployee = v, value);
    }

    public decimal HealthInsuranceEmployerRate
    {
        get => GetRatePercent(s => s.HealthInsuranceRateEmployer, InsuranceRateDefaults.HealthInsuranceEmployerRate);
        set => SetRatePercent((s, v) => s.HealthInsuranceRateEmployer = v, value);
    }

    // 장기요양보험 요율 (%)
    public decimal LongTermCareEmployeeRate
    {
        get => GetRatePercent(s => s.LongTermCareRateEmployee, InsuranceRateDefaults.LongTermCareRateEmployee);
        set => SetRatePercent((s, v) => s.LongTermCareRateEmployee = v, value);
    }

    public decimal LongTermCareEmployerRate
    {
        get => GetRatePercent(s => s.LongTermCareRateEmployer, InsuranceRateDefaults.LongTermCareRateEmployer);
        set => SetRatePercent((s, v) => s.LongTermCareRateEmployer = v, value);
    }

    // 고용보험 요율 (%)
    public decimal EmploymentInsuranceEmployeeRate
    {
        get => GetRatePercent(s => s.EmploymentInsuranceRateEmployee, InsuranceRateDefaults.EmploymentInsuranceEmployeeRate);
        set => SetRatePercent((s, v) => s.EmploymentInsuranceRateEmployee = v, value);
    }

    public decimal EmploymentInsuranceEmployerRate
    {
        get => GetRatePercent(s => s.EmploymentInsuranceRateEmployer, InsuranceRateDefaults.EmploymentInsuranceEmployerRate);
        set => SetRatePercent((s, v) => s.EmploymentInsuranceRateEmployer = v, value);
    }

    // 산재보험 요율 (%)
    public decimal IndustrialAccidentEmployeeRate
    {
        get => GetRatePercent(s => s.IndustrialAccidentRateEmployee, InsuranceRateDefaults.IndustrialAccidentEmployeeRate);
        set => SetRatePercent((s, v) => s.IndustrialAccidentRateEmployee = v, value);
    }

    public decimal IndustrialAccidentEmployerRate
    {
        get => GetRatePercent(s => s.IndustrialAccidentRateEmployer, InsuranceRateDefaults.IndustrialAccidentEmployerRate);
        set => SetRatePercent((s, v) => s.IndustrialAccidentRateEmployer = v, value);
    }

    // 최저한도 - 기준금액
    public decimal NationalPensionMinBaseAmount
    {
        get => _currentSetting?.NationalPensionMinBaseAmount ?? InsuranceRateDefaults.NationalPensionMinBaseAmount;
        set
        {
            if (_currentSetting != null)
            {
                _currentSetting.NationalPensionMinBaseAmount = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal HealthInsuranceMinBaseAmount
    {
        get => _currentSetting?.HealthInsuranceMinBaseAmount ?? InsuranceRateDefaults.HealthInsuranceMinBaseAmount;
        set
        {
            if (_currentSetting != null)
            {
                _currentSetting.HealthInsuranceMinBaseAmount = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal LongTermCareMinBaseAmount
    {
        get => _currentSetting?.LongTermCareMinBaseAmount ?? InsuranceRateDefaults.LongTermCareMinBaseAmount;
        set
        {
            if (_currentSetting != null)
            {
                _currentSetting.LongTermCareMinBaseAmount = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal EmploymentInsuranceMinBaseAmount
    {
        get => _currentSetting?.EmploymentInsuranceMinBaseAmount ?? InsuranceRateDefaults.EmploymentInsuranceMinBaseAmount;
        set
        {
            if (_currentSetting != null)
            {
                _currentSetting.EmploymentInsuranceMinBaseAmount = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal IndustrialAccidentMinBaseAmount
    {
        get => _currentSetting?.IndustrialAccidentMinBaseAmount ?? InsuranceRateDefaults.IndustrialAccidentMinBaseAmount;
        set
        {
            if (_currentSetting != null)
            {
                _currentSetting.IndustrialAccidentMinBaseAmount = value;
                OnPropertyChanged();
            }
        }
    }

    // 최저한도 - 보험료
    public decimal NationalPensionMinPremium
    {
        get => _currentSetting?.NationalPensionMinPremium ?? InsuranceRateDefaults.NationalPensionMinPremium;
        set
        {
            if (_currentSetting != null)
            {
                _currentSetting.NationalPensionMinPremium = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal HealthInsuranceMinPremium
    {
        get => _currentSetting?.HealthInsuranceMinPremium ?? InsuranceRateDefaults.HealthInsuranceMinPremium;
        set
        {
            if (_currentSetting != null)
            {
                _currentSetting.HealthInsuranceMinPremium = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal LongTermCareMinPremium
    {
        get => _currentSetting?.LongTermCareMinPremium ?? InsuranceRateDefaults.LongTermCareMinPremium;
        set
        {
            if (_currentSetting != null)
            {
                _currentSetting.LongTermCareMinPremium = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal EmploymentInsuranceMinPremium
    {
        get => _currentSetting?.EmploymentInsuranceMinPremium ?? InsuranceRateDefaults.EmploymentInsuranceMinPremium;
        set
        {
            if (_currentSetting != null)
            {
                _currentSetting.EmploymentInsuranceMinPremium = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal IndustrialAccidentMinPremium
    {
        get => _currentSetting?.IndustrialAccidentMinPremium ?? InsuranceRateDefaults.IndustrialAccidentMinPremium;
        set
        {
            if (_currentSetting != null)
            {
                _currentSetting.IndustrialAccidentMinPremium = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SaveCommand { get; }

    public InsuranceRateSettingViewModel()
    {
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    private bool CanSave()
    {
        return _currentSetting != null;
    }

    public async Task LoadAsync(Company company)
    {
        _currentCompany = company;

        using var context = new AccountingDbContext();

        _currentSetting = await context.InsuranceRateSettings
            .Where(s => s.CompanyId == company.Id && s.EffectiveTo == null)
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefaultAsync();

        if (_currentSetting == null)
        {
            _currentSetting = new InsuranceRateSetting
            {
                CompanyId = company.Id,
                NationalPensionRateEmployee = InsuranceRateDefaults.NationalPensionEmployeeRate,
                NationalPensionRateEmployer = InsuranceRateDefaults.NationalPensionEmployerRate,
                HealthInsuranceRateEmployee = InsuranceRateDefaults.HealthInsuranceEmployeeRate,
                HealthInsuranceRateEmployer = InsuranceRateDefaults.HealthInsuranceEmployerRate,
                LongTermCareRateEmployee = InsuranceRateDefaults.LongTermCareRateEmployee,
                LongTermCareRateEmployer = InsuranceRateDefaults.LongTermCareRateEmployer,
                EmploymentInsuranceRateEmployee = InsuranceRateDefaults.EmploymentInsuranceEmployeeRate,
                EmploymentInsuranceRateEmployer = InsuranceRateDefaults.EmploymentInsuranceEmployerRate,
                IndustrialAccidentRateEmployee = InsuranceRateDefaults.IndustrialAccidentEmployeeRate,
                IndustrialAccidentRateEmployer = InsuranceRateDefaults.IndustrialAccidentEmployerRate,
                NationalPensionMinBaseAmount = InsuranceRateDefaults.NationalPensionMinBaseAmount,
                NationalPensionMinPremium = InsuranceRateDefaults.NationalPensionMinPremium,
                HealthInsuranceMinBaseAmount = InsuranceRateDefaults.HealthInsuranceMinBaseAmount,
                HealthInsuranceMinPremium = InsuranceRateDefaults.HealthInsuranceMinPremium,
                LongTermCareMinBaseAmount = InsuranceRateDefaults.LongTermCareMinBaseAmount,
                LongTermCareMinPremium = InsuranceRateDefaults.LongTermCareMinPremium,
                EmploymentInsuranceMinBaseAmount = InsuranceRateDefaults.EmploymentInsuranceMinBaseAmount,
                EmploymentInsuranceMinPremium = InsuranceRateDefaults.EmploymentInsuranceMinPremium,
                IndustrialAccidentMinBaseAmount = InsuranceRateDefaults.IndustrialAccidentMinBaseAmount,
                IndustrialAccidentMinPremium = InsuranceRateDefaults.IndustrialAccidentMinPremium,
                EffectiveFrom = DateTime.Today
            };
        }

        OnPropertyChanged(string.Empty);
        (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
    }

    private async Task SaveAsync()
    {
        if (_currentSetting == null || _currentCompany == null) return;

        try
        {
            using var context = new AccountingDbContext();

            if (_currentSetting.Id == 0)
            {
                context.InsuranceRateSettings.Add(_currentSetting);
            }
            else
            {
                context.InsuranceRateSettings.Update(_currentSetting);
            }

            await context.SaveChangesAsync();

            System.Windows.MessageBox.Show(
                "요율 설정이 저장되었습니다.",
                "저장 완료",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"저장 중 오류가 발생했습니다: {ex.Message}",
                "오류",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
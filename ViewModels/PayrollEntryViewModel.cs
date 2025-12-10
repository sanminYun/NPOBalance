using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using NPOBalance.Data;
using NPOBalance.Models;
using NPOBalance.Services;

namespace NPOBalance.ViewModels;

public class PayrollEntryViewModel : ObservableObject
{
    private const int DefaultRowCount = 20;
    private readonly SimplifiedTaxTableProvider _taxTableProvider;
    private Company? _company;
    private PayrollEntryRowViewModel? _selectedRow;
    private int _dirtySuppressCounter;
    private bool _hasPendingChanges;

    public ObservableCollection<PayrollEntryRowViewModel> PayrollRows { get; }

    public PayrollEntryRowViewModel? SelectedRow
    {
        get => _selectedRow;
        set
        {
            if (_selectedRow != null)
            {
                _selectedRow.PropertyChanged -= SelectedRowOnPropertyChanged;
            }

            if (SetProperty(ref _selectedRow, value))
            {
                if (_selectedRow != null)
                {
                    _selectedRow.PropertyChanged += SelectedRowOnPropertyChanged;
                }

                OnPropertyChanged(nameof(HasSelectedRow));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool HasSelectedRow => SelectedRow != null;
    public Company? CurrentCompany => _company;

    public ICommand AddRowsCommand { get; }
    public ICommand ClearEmployeeCommand { get; }
    public ICommand SavePayrollCommand { get; }

    public PayrollEntryViewModel()
    {
        _taxTableProvider = new SimplifiedTaxTableProvider();

        PayrollRows = new ObservableCollection<PayrollEntryRowViewModel>();
        AddRowsCommand = new RelayCommand(_ => AddBlankRows(5));
        ClearEmployeeCommand = new RelayCommand(_ => ClearSelectedRow(), _ => SelectedRow?.HasEmployee == true);
        SavePayrollCommand = new RelayCommand(async _ => await SavePayrollAsync(), _ => CanSavePayroll());

        AddBlankRows(DefaultRowCount);
        SelectedRow = PayrollRows.FirstOrDefault();
    }

    private void SelectedRowOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PayrollEntryRowViewModel.HasEmployee))
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public async Task InitializeAsync(Company company)
    {
        _company = company;

        BeginSuppressDirty();
        try
        {
            foreach (var row in PayrollRows)
            {
                row.Detail.Reset();
                row.ResetEmployee();
            }

            using var db = new AccountingDbContext();
            var drafts = await db.PayrollEntryDrafts
                .Include(d => d.Employee)
                .Where(d => d.CompanyId == company.Id)
                .OrderBy(d => d.Employee.EmployeeCode)
                .ToListAsync();

            var requiredRows = Math.Max(DefaultRowCount, drafts.Count + 5);
            if (PayrollRows.Count < requiredRows)
            {
                AddBlankRows(requiredRows - PayrollRows.Count);
            }

            for (int i = 0; i < drafts.Count; i++)
            {
                var draft = drafts[i];
                var employee = draft.Employee ?? await db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == draft.EmployeeId);
                if (employee == null)
                {
                    continue;
                }

                var row = PayrollRows[i];
                row.AssignEmployee(employee);
                row.Detail.LoadFromDraft(draft);
            }
        }
        finally
        {
            EndSuppressDirty();
        }

        ResetDirtyFlag();
        SelectedRow = PayrollRows.FirstOrDefault(r => r.HasEmployee) ?? PayrollRows.FirstOrDefault();
    }

    public bool TryAssignEmployee(Employee employee)
    {
        if (_company == null)
        {
            MessageBox.Show("회사를 먼저 선택하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        if (SelectedRow == null)
        {
            MessageBox.Show("사원을 배치할 행을 먼저 선택하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        if (PayrollRows.Any(r => r != SelectedRow && r.EmployeeId == employee.Id))
        {
            MessageBox.Show($"'{employee.Name}' 사원은 이미 목록에 있습니다.", "중복 사원", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        BeginSuppressDirty();
        try
        {
            SelectedRow.Detail.Reset();
            SelectedRow.AssignEmployee(employee);
        }
        finally
        {
            EndSuppressDirty();
        }

        var loadedFromStore = TryLoadDraftForRow(SelectedRow);
        if (!loadedFromStore)
        {
            MarkDirty();
        }

        return true;
    }

    public void ClearSelectedRow()
    {
        if (SelectedRow == null)
        {
            return;
        }

        SelectedRow.Detail.Reset();
        SelectedRow.ResetEmployee();
    }

    private void AddBlankRows(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var row = new PayrollEntryRowViewModel(_taxTableProvider)
            {
                Sequence = PayrollRows.Count + 1
            };

            AttachRowHandlers(row);
            PayrollRows.Add(row);
        }

        RefreshSequences();
    }

    private void AttachRowHandlers(PayrollEntryRowViewModel row)
    {
        row.Detail.PropertyChanged += RowDetailOnPropertyChanged;
    }

    private void RowDetailOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        MarkDirty();
    }

    private void RefreshSequences()
    {
        for (int i = 0; i < PayrollRows.Count; i++)
        {
            PayrollRows[i].Sequence = i + 1;
        }
    }

    private void BeginSuppressDirty() => _dirtySuppressCounter++;

    private void EndSuppressDirty()
    {
        if (_dirtySuppressCounter > 0)
        {
            _dirtySuppressCounter--;
        }
    }

    private bool IsDirtySuppressed => _dirtySuppressCounter > 0;

    private void MarkDirty()
    {
        if (IsDirtySuppressed)
        {
            return;
        }

        _hasPendingChanges = true;
        CommandManager.InvalidateRequerySuggested();
    }

    private void ResetDirtyFlag()
    {
        _hasPendingChanges = false;
        CommandManager.InvalidateRequerySuggested();
    }

    private bool CanSavePayroll()
    {
        return !IsDirtySuppressed && _hasPendingChanges && _company != null;
    }

    private async Task SavePayrollAsync()
    {
        if (_company == null)
        {
            MessageBox.Show("회사를 먼저 선택하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            using var db = new AccountingDbContext();
            var companyId = _company.Id;

            var existingDrafts = await db.PayrollEntryDrafts
                .Where(d => d.CompanyId == companyId)
                .ToListAsync();

            var existingMap = existingDrafts.ToDictionary(d => d.EmployeeId);
            var activeEmployees = new HashSet<int>();

            foreach (var row in PayrollRows.Where(r => r.HasEmployee))
            {
                var employeeId = row.EmployeeId!.Value;
                activeEmployees.Add(employeeId);

                if (!existingMap.TryGetValue(employeeId, out var draft))
                {
                    draft = new PayrollEntryDraft
                    {
                        CompanyId = companyId,
                        EmployeeId = employeeId
                    };
                    db.PayrollEntryDrafts.Add(draft);
                    existingMap[employeeId] = draft;
                }

                row.Detail.CopyToDraft(draft);
            }

            foreach (var draft in existingDrafts)
            {
                if (!activeEmployees.Contains(draft.EmployeeId))
                {
                    db.PayrollEntryDrafts.Remove(draft);
                }
            }

            await db.SaveChangesAsync();
            ResetDirtyFlag();
            MessageBox.Show("급여 입력 항목이 저장되었습니다.", "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool TryLoadDraftForRow(PayrollEntryRowViewModel row)
    {
        if (_company == null || row.EmployeeId == null)
        {
            return false;
        }

        using var db = new AccountingDbContext();
        var draft = db.PayrollEntryDrafts
            .AsNoTracking()
            .FirstOrDefault(d => d.CompanyId == _company.Id && d.EmployeeId == row.EmployeeId.Value);

        if (draft == null)
        {
            return false;
        }

        BeginSuppressDirty();
        try
        {
            row.Detail.LoadFromDraft(draft);
        }
        finally
        {
            EndSuppressDirty();
        }

        return true;
    }
}

public class PayrollEntryRowViewModel : ObservableObject
{
    private readonly PayrollEntryDetailViewModel _detail;
    private int _sequence;
    private int? _employeeId;
    private string? _employeeCode;
    private string? _employeeName;
    private string? _department;

    public PayrollEntryRowViewModel(SimplifiedTaxTableProvider taxTableProvider)
    {
        _detail = new PayrollEntryDetailViewModel(taxTableProvider);
        _detail.PropertyChanged += DetailOnPropertyChanged;
    }

    public PayrollEntryDetailViewModel Detail => _detail;

    public int Sequence
    {
        get => _sequence;
        set => SetProperty(ref _sequence, value);
    }

    public int? EmployeeId
    {
        get => _employeeId;
        private set
        {
            if (SetProperty(ref _employeeId, value))
            {
                OnPropertyChanged(nameof(HasEmployee));
            }
        }
    }

    public string? EmployeeCode
    {
        get => _employeeCode;
        private set => SetProperty(ref _employeeCode, value);
    }

    public string? EmployeeName
    {
        get => _employeeName;
        private set => SetProperty(ref _employeeName, value);
    }

    public string? Department
    {
        get => _department;
        private set => SetProperty(ref _department, value);
    }

    public bool HasEmployee => EmployeeId.HasValue;

    public decimal NetPay => Detail.NetPay;
    public decimal CompanyBurden => Detail.EmployerTotalBurden;

    public void AssignEmployee(Employee employee)
    {
        EmployeeId = employee.Id;
        EmployeeCode = employee.EmployeeCode;
        EmployeeName = employee.Name;
        Department = employee.Department;
        Detail.ApplyEmployeeContext(employee);
    }

    public void ResetEmployee()
    {
        EmployeeId = null;
        EmployeeCode = null;
        EmployeeName = null;
        Department = null;
    }

    private void DetailOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PayrollEntryDetailViewModel.NetPay) ||
            e.PropertyName == nameof(PayrollEntryDetailViewModel.EmployerTotalBurden))
        {
            OnPropertyChanged(nameof(NetPay));
            OnPropertyChanged(nameof(CompanyBurden));
        }
    }
}

public class PayrollEntryDetailViewModel : ObservableObject
{
    private readonly SimplifiedTaxTableProvider _taxTableProvider;
    private decimal _baseSalary;
    private decimal _holidayAllowance;
    private decimal _mealAllowance;
    private decimal _maternitySupport;
    private decimal _nationalPensionSettlement;
    private decimal _healthInsuranceSettlement;
    private decimal _longTermCareSettlement;
    private decimal _employmentInsuranceSettlement;
    private decimal _midtermIncomeTaxAdjustment;
    private decimal _midtermLocalTaxAdjustment;
    private decimal _yearEndIncomeTaxAdjustment;
    private decimal _yearEndLocalTaxAdjustment;
    private decimal _employerNationalPensionSettlement;
    private decimal _employerHealthInsuranceSettlement;
    private decimal _employerLongTermCareSettlement;
    private decimal _employerEmploymentInsuranceSettlement;
    private decimal _retirementDc;
    private decimal _retirementDb;
    private decimal _estimatedAnnualSalary;

    public PayrollEntryDetailViewModel(SimplifiedTaxTableProvider taxTableProvider)
    {
        _taxTableProvider = taxTableProvider;
    }

    public decimal BaseSalary
    {
        get => _baseSalary;
        set
        {
            if (SetProperty(ref _baseSalary, value))
            {
                OnEarningsChanged();
            }
        }
    }

    public decimal HolidayAllowance
    {
        get => _holidayAllowance;
        set
        {
            if (SetProperty(ref _holidayAllowance, value))
            {
                OnEarningsChanged();
            }
        }
    }

    public decimal MealAllowance
    {
        get => _mealAllowance;
        set
        {
            if (SetProperty(ref _mealAllowance, value))
            {
                OnEarningsChanged();
            }
        }
    }

    public decimal MaternitySupport
    {
        get => _maternitySupport;
        set
        {
            if (SetProperty(ref _maternitySupport, value))
            {
                OnEarningsChanged();
            }
        }
    }

    public decimal NationalPensionSettlement
    {
        get => _nationalPensionSettlement;
        set
        {
            if (SetProperty(ref _nationalPensionSettlement, value))
            {
                OnInsuranceSectionChanged();
            }
        }
    }

    public decimal HealthInsuranceSettlement
    {
        get => _healthInsuranceSettlement;
        set
        {
            if (SetProperty(ref _healthInsuranceSettlement, value))
            {
                OnInsuranceSectionChanged();
            }
        }
    }

    public decimal LongTermCareSettlement
    {
        get => _longTermCareSettlement;
        set
        {
            if (SetProperty(ref _longTermCareSettlement, value))
            {
                OnInsuranceSectionChanged();
            }
        }
    }

    public decimal EmploymentInsuranceSettlement
    {
        get => _employmentInsuranceSettlement;
        set
        {
            if (SetProperty(ref _employmentInsuranceSettlement, value))
            {
                OnInsuranceSectionChanged();
            }
        }
    }

    public decimal MidtermIncomeTaxAdjustment
    {
        get => _midtermIncomeTaxAdjustment;
        set
        {
            if (SetProperty(ref _midtermIncomeTaxAdjustment, value))
            {
                OnIncomeTaxSectionChanged();
            }
        }
    }

    public decimal MidtermLocalTaxAdjustment
    {
        get => _midtermLocalTaxAdjustment;
        set
        {
            if (SetProperty(ref _midtermLocalTaxAdjustment, value))
            {
                OnIncomeTaxSectionChanged();
            }
        }
    }

    public decimal YearEndIncomeTaxAdjustment
    {
        get => _yearEndIncomeTaxAdjustment;
        set
        {
            if (SetProperty(ref _yearEndIncomeTaxAdjustment, value))
            {
                OnIncomeTaxSectionChanged();
            }
        }
    }

    public decimal YearEndLocalTaxAdjustment
    {
        get => _yearEndLocalTaxAdjustment;
        set
        {
            if (SetProperty(ref _yearEndLocalTaxAdjustment, value))
            {
                OnIncomeTaxSectionChanged();
            }
        }
    }

    public decimal EmployerNationalPensionSettlement
    {
        get => _employerNationalPensionSettlement;
        set
        {
            if (SetProperty(ref _employerNationalPensionSettlement, value))
            {
                OnEmployerContributionChanged();
            }
        }
    }

    public decimal EmployerHealthInsuranceSettlement
    {
        get => _employerHealthInsuranceSettlement;
        set
        {
            if (SetProperty(ref _employerHealthInsuranceSettlement, value))
            {
                OnEmployerContributionChanged();
            }
        }
    }

    public decimal EmployerLongTermCareSettlement
    {
        get => _employerLongTermCareSettlement;
        set
        {
            if (SetProperty(ref _employerLongTermCareSettlement, value))
            {
                OnEmployerContributionChanged();
            }
        }
    }

    public decimal EmployerEmploymentInsuranceSettlement
    {
        get => _employerEmploymentInsuranceSettlement;
        set
        {
            if (SetProperty(ref _employerEmploymentInsuranceSettlement, value))
            {
                OnEmployerContributionChanged();
            }
        }
    }

    public decimal RetirementDc
    {
        get => _retirementDc;
        set
        {
            if (SetProperty(ref _retirementDc, value))
            {
                OnRetirementSectionChanged();
            }
        }
    }

    public decimal RetirementDb
    {
        get => _retirementDb;
        set
        {
            if (SetProperty(ref _retirementDb, value))
            {
                OnRetirementSectionChanged();
            }
        }
    }

    public decimal EstimatedAnnualSalary
    {
        get => _estimatedAnnualSalary;
        private set
        {
            if (SetProperty(ref _estimatedAnnualSalary, value))
            {
                OnPropertyChanged(nameof(IncomeTax));
                OnPropertyChanged(nameof(LocalIncomeTax));
                OnIncomeTaxSectionChanged();
            }
        }
    }

    public decimal TaxableEarningsSubtotal => BaseSalary + HolidayAllowance;
    public decimal NonTaxableEarningsSubtotal => MealAllowance + MaternitySupport;

    public decimal EmployeeNationalPension => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.NationalPensionEmployeeRate);
    public decimal EmployeeHealthInsurance => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.HealthInsuranceEmployeeRate);
    public decimal EmployeeLongTermCare => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.LongTermCareEmployeeRate);
    public decimal EmployeeEmploymentInsurance => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.EmploymentInsuranceEmployeeRate);

    public decimal InsuranceDeductionSubtotal => EmployeeNationalPension + EmployeeHealthInsurance + EmployeeLongTermCare + EmployeeEmploymentInsurance +
                                                  NationalPensionSettlement + HealthInsuranceSettlement + LongTermCareSettlement + EmploymentInsuranceSettlement;

    public decimal IncomeTax => _taxTableProvider.GetWithholdingTax(EstimatedAnnualSalary);
    public decimal LocalIncomeTax => RoundDown(IncomeTax * 0.1m);

    public decimal IncomeTaxSubtotal => IncomeTax + LocalIncomeTax + MidtermIncomeTaxAdjustment + MidtermLocalTaxAdjustment +
                                         YearEndIncomeTaxAdjustment + YearEndLocalTaxAdjustment;

    public decimal EmployerNationalPension => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.NationalPensionEmployerRate);
    public decimal EmployerHealthInsurance => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.HealthInsuranceEmployerRate);
    public decimal EmployerLongTermCare => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.LongTermCareEmployerRate);
    public decimal EmployerEmploymentInsurance => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.EmploymentInsuranceEmployerRate);
    public decimal IndustrialAccidentInsurance => RoundDown(TaxableEarningsSubtotal * PayrollRateConfig.IndustrialAccidentEmployerRate);

    public decimal EmployerInsuranceSubtotal => EmployerNationalPension + EmployerHealthInsurance + EmployerLongTermCare + EmployerEmploymentInsurance + IndustrialAccidentInsurance +
                                                 EmployerNationalPensionSettlement + EmployerHealthInsuranceSettlement + EmployerLongTermCareSettlement + EmployerEmploymentInsuranceSettlement;

    public decimal RetirementSubtotal => RetirementDc + RetirementDb;

    public decimal NetPay => TaxableEarningsSubtotal + NonTaxableEarningsSubtotal - InsuranceDeductionSubtotal - IncomeTaxSubtotal;

    public decimal EmployerTotalBurden => NetPay + EmployerInsuranceSubtotal + RetirementSubtotal;

    public void ApplyEmployeeContext(Employee employee)
    {
        var annualEstimate = employee.EstimatedTotalSalary ?? 0m;
        EstimatedAnnualSalary = annualEstimate;
    }

    public void LoadFromDraft(PayrollEntryDraft draft)
    {
        BaseSalary = draft.BaseSalary;
        HolidayAllowance = draft.HolidayAllowance;
        MealAllowance = draft.MealAllowance;
        MaternitySupport = draft.MaternitySupport;
        NationalPensionSettlement = draft.NationalPensionSettlement;
        HealthInsuranceSettlement = draft.HealthInsuranceSettlement;
        LongTermCareSettlement = draft.LongTermCareSettlement;
        EmploymentInsuranceSettlement = draft.EmploymentInsuranceSettlement;
        MidtermIncomeTaxAdjustment = draft.MidtermIncomeTaxAdjustment;
        MidtermLocalTaxAdjustment = draft.MidtermLocalTaxAdjustment;
        YearEndIncomeTaxAdjustment = draft.YearEndIncomeTaxAdjustment;
        YearEndLocalTaxAdjustment = draft.YearEndLocalTaxAdjustment;
        EmployerNationalPensionSettlement = draft.EmployerNationalPensionSettlement;
        EmployerHealthInsuranceSettlement = draft.EmployerHealthInsuranceSettlement;
        EmployerLongTermCareSettlement = draft.EmployerLongTermCareSettlement;
        EmployerEmploymentInsuranceSettlement = draft.EmployerEmploymentInsuranceSettlement;
        RetirementDc = draft.RetirementDc;
        RetirementDb = draft.RetirementDb;
        EstimatedAnnualSalary = draft.EstimatedAnnualSalary;
    }

    public void CopyToDraft(PayrollEntryDraft draft)
    {
        draft.BaseSalary = BaseSalary;
        draft.HolidayAllowance = HolidayAllowance;
        draft.MealAllowance = MealAllowance;
        draft.MaternitySupport = MaternitySupport;
        draft.NationalPensionSettlement = NationalPensionSettlement;
        draft.HealthInsuranceSettlement = HealthInsuranceSettlement;
        draft.LongTermCareSettlement = LongTermCareSettlement;
        draft.EmploymentInsuranceSettlement = EmploymentInsuranceSettlement;
        draft.MidtermIncomeTaxAdjustment = MidtermIncomeTaxAdjustment;
        draft.MidtermLocalTaxAdjustment = MidtermLocalTaxAdjustment;
        draft.YearEndIncomeTaxAdjustment = YearEndIncomeTaxAdjustment;
        draft.YearEndLocalTaxAdjustment = YearEndLocalTaxAdjustment;
        draft.EmployerNationalPensionSettlement = EmployerNationalPensionSettlement;
        draft.EmployerHealthInsuranceSettlement = EmployerHealthInsuranceSettlement;
        draft.EmployerLongTermCareSettlement = EmployerLongTermCareSettlement;
        draft.EmployerEmploymentInsuranceSettlement = EmployerEmploymentInsuranceSettlement;
        draft.RetirementDc = RetirementDc;
        draft.RetirementDb = RetirementDb;
        draft.EstimatedAnnualSalary = EstimatedAnnualSalary;
        draft.UpdatedAt = DateTime.UtcNow;
    }

    public void Reset()
    {
        BaseSalary = 0;
        HolidayAllowance = 0;
        MealAllowance = 0;
        MaternitySupport = 0;
        NationalPensionSettlement = 0;
        HealthInsuranceSettlement = 0;
        LongTermCareSettlement = 0;
        EmploymentInsuranceSettlement = 0;
        MidtermIncomeTaxAdjustment = 0;
        MidtermLocalTaxAdjustment = 0;
        YearEndIncomeTaxAdjustment = 0;
        YearEndLocalTaxAdjustment = 0;
        EmployerNationalPensionSettlement = 0;
        EmployerHealthInsuranceSettlement = 0;
        EmployerLongTermCareSettlement = 0;
        EmployerEmploymentInsuranceSettlement = 0;
        RetirementDc = 0;
        RetirementDb = 0;
        EstimatedAnnualSalary = 0;
    }

    private void OnEarningsChanged()
    {
        OnPropertyChanged(nameof(TaxableEarningsSubtotal));
        OnPropertyChanged(nameof(NonTaxableEarningsSubtotal));
        OnPropertyChanged(nameof(EmployeeNationalPension));
        OnPropertyChanged(nameof(EmployeeHealthInsurance));
        OnPropertyChanged(nameof(EmployeeLongTermCare));
        OnPropertyChanged(nameof(EmployeeEmploymentInsurance));
        OnPropertyChanged(nameof(InsuranceDeductionSubtotal));
        OnPropertyChanged(nameof(EmployerNationalPension));
        OnPropertyChanged(nameof(EmployerHealthInsurance));
        OnPropertyChanged(nameof(EmployerLongTermCare));
        OnPropertyChanged(nameof(EmployerEmploymentInsurance));
        OnPropertyChanged(nameof(IndustrialAccidentInsurance));
        OnPropertyChanged(nameof(EmployerInsuranceSubtotal));
        OnPropertyChanged(nameof(NetPay));
        OnPropertyChanged(nameof(EmployerTotalBurden));
    }

    private void OnInsuranceSectionChanged()
    {
        OnPropertyChanged(nameof(InsuranceDeductionSubtotal));
        OnPropertyChanged(nameof(NetPay));
        OnPropertyChanged(nameof(EmployerTotalBurden));
    }

    private void OnIncomeTaxSectionChanged()
    {
        OnPropertyChanged(nameof(IncomeTaxSubtotal));
        OnPropertyChanged(nameof(NetPay));
        OnPropertyChanged(nameof(EmployerTotalBurden));
    }

    private void OnEmployerContributionChanged()
    {
        OnPropertyChanged(nameof(EmployerInsuranceSubtotal));
        OnPropertyChanged(nameof(EmployerTotalBurden));
    }

    private void OnRetirementSectionChanged()
    {
        OnPropertyChanged(nameof(RetirementSubtotal));
        OnPropertyChanged(nameof(EmployerTotalBurden));
    }

    private static decimal RoundDown(decimal value)
    {
        return value >= 0 ? Math.Floor(value) : Math.Ceiling(value);
    }
}

internal static class PayrollRateConfig
{
    public const decimal NationalPensionEmployeeRate = 0.045m;
    public const decimal NationalPensionEmployerRate = 0.045m;
    public const decimal HealthInsuranceEmployeeRate = 0.03545m;
    public const decimal HealthInsuranceEmployerRate = 0.03545m;
    public const decimal LongTermCareEmployeeRate = 0.00473m;
    public const decimal LongTermCareEmployerRate = 0.00473m;
    public const decimal EmploymentInsuranceEmployeeRate = 0.009m;
    public const decimal EmploymentInsuranceEmployerRate = 0.015m;
    public const decimal IndustrialAccidentEmployerRate = 0.0069m;
}

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
    private readonly PayItemService _payItemService;
    private Company? _company;
    private PayrollEntryRowViewModel? _selectedRow;
    private int _dirtySuppressCounter;
    private bool _hasPendingChanges;
    private List<string> _fundingSources = new();
    private string _selectedFundingSource = string.Empty;
    private DateTime _accrualMonth;
    private DateTime _paymentDate;

    public ObservableCollection<PayrollEntryRowViewModel> PayrollRows { get; }
    public List<string> FundingSources => _fundingSources;

    public string SelectedFundingSource
    {
        get => _selectedFundingSource;
        set
        {
            if (SetProperty(ref _selectedFundingSource, value))
            {
                MarkDirty();
            }
        }
    }

    public DateTime PaymentDate
    {
        get => _paymentDate;
        set
        {
            if (SetProperty(ref _paymentDate, value))
            {
                MarkDirty();
            }
        }
    }

    public PayrollEntryRowViewModel? SelectedRow
    {
        get => _selectedRow;
        set
        {
            // 비어있는 행 선택 방지
            if (value != null && !value.HasEmployee)
            {
                return;
            }

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

    public bool HasSelectedRow => SelectedRow?.HasEmployee == true;

    private void SelectedRowOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PayrollEntryRowViewModel.HasEmployee))
        {
            OnPropertyChanged(nameof(HasSelectedRow));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public Company? CurrentCompany => _company;

    public ICommand AddRowsCommand { get; }
    public ICommand ClearEmployeeCommand { get; }
    public ICommand SavePayrollCommand { get; }

    public PayrollEntryViewModel()
    {
        _taxTableProvider = new SimplifiedTaxTableProvider();
        _payItemService = new PayItemService();

        PayrollRows = new ObservableCollection<PayrollEntryRowViewModel>();
        AddRowsCommand = new RelayCommand(_ =>
        {
            var newRows = AddBlankRows(5);
            _ = ApplyCompanyContextToRowsAsync(newRows);
        });
        ClearEmployeeCommand = new RelayCommand(_ => ClearSelectedRow(), _ => SelectedRow?.HasEmployee == true);
        SavePayrollCommand = new RelayCommand(async _ => await SavePayrollAsync(), _ => CanSavePayroll());

        var today = DateTime.Today;
        _accrualMonth = new DateTime(today.Year, today.Month, 1);
        _paymentDate = today; // 이 줄 추가

        AddBlankRows(DefaultRowCount);
        SelectedRow = PayrollRows.FirstOrDefault();
    }

    public async Task InitializeAsync(Company company)
    {
        _company = company;
        await LoadFundingSourcesAsync().ConfigureAwait(false);
        await ReloadDraftsAsync().ConfigureAwait(false);
    }

    private async Task LoadFundingSourcesAsync()
    {
        _fundingSources = await _payItemService.GetPayItemsAsync(PayItemService.FundingSource).ConfigureAwait(false);
        OnPropertyChanged(nameof(FundingSources));

        if (_fundingSources.Count > 0 && string.IsNullOrEmpty(_selectedFundingSource))
        {
            _selectedFundingSource = _fundingSources[0];
            OnPropertyChanged(nameof(SelectedFundingSource));
        }
    }

    private async Task ReloadDraftsAsync()
    {
        if (_company == null)
        {
            return;
        }

        List<PayrollEntryDraft> drafts = new();

        await ApplyCompanyContextToRowsAsync().ConfigureAwait(false);

        BeginSuppressDirty();
        try
        {
            foreach (var row in PayrollRows)
            {
                await row.Detail.ResetAsync().ConfigureAwait(false);
                row.ResetEmployee();
            }

            using var db = new AccountingDbContext();
            drafts = await db.PayrollEntryDrafts
                .Include(d => d.Employee)
                .Where(d => d.CompanyId == _company.Id)
                .Where(d => d.AccrualYear == _accrualMonth.Year && d.AccrualMonth == _accrualMonth.Month)
                .OrderBy(d => d.Employee!.EmployeeCode)
                .ToListAsync()
                .ConfigureAwait(false);

            var requiredRows = Math.Max(DefaultRowCount, drafts.Count + 5);
            if (PayrollRows.Count < requiredRows)
            {
                var addedRows = AddBlankRows(requiredRows - PayrollRows.Count);
                await ApplyCompanyContextToRowsAsync(addedRows).ConfigureAwait(false);
            }

            for (int i = 0; i < drafts.Count; i++)
            {
                var draft = drafts[i];
                if (draft.Employee == null)
                {
                    continue;
                }

                var row = PayrollRows[i];
                row.AssignEmployee(draft.Employee);
                await row.Detail.LoadFromDraftAsync(draft).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(draft.FundingSource))
                {
                    _selectedFundingSource = draft.FundingSource;
                    OnPropertyChanged(nameof(SelectedFundingSource));
                }
            }
        }
        finally
        {
            EndSuppressDirty();
        }

        ResetDirtyFlag();

        if (drafts.Count == 0)
        {
            var defaultFunding = _fundingSources.FirstOrDefault() ?? string.Empty;
            if (_selectedFundingSource != defaultFunding)
            {
                _selectedFundingSource = defaultFunding;
                OnPropertyChanged(nameof(SelectedFundingSource));
            }

            SelectedRow = null;
            return;
        }

        var firstWithEmployee = PayrollRows.FirstOrDefault(r => r.HasEmployee);
        SelectedRow = firstWithEmployee ?? null;
    }

    public async Task RefreshEmployeeDataAsync()
    {
        if (_company == null)
        {
            return;
        }

        await ApplyCompanyContextToRowsAsync().ConfigureAwait(false);

        using var db = new AccountingDbContext();
        
        // 현재 표시된 사원들의 최신 정보를 불러옴
        var employeeIds = PayrollRows
            .Where(r => r.HasEmployee && r.EmployeeId.HasValue)
            .Select(r => r.EmployeeId!.Value)
            .ToList();

        if (employeeIds.Count == 0)
        {
            return;
        }

        var employees = await db.Employees
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        BeginSuppressDirty();
        try
        {
            foreach (var row in PayrollRows.Where(r => r.HasEmployee && r.EmployeeId.HasValue))
            {
                if (employees.TryGetValue(row.EmployeeId!.Value, out var employee))
                {
                    row.RefreshEmployeeContext(employee);
                }
            }
        }
        finally
        {
            EndSuppressDirty();
        }
    }

    public bool TryAssignEmployee(Employee employee)
    {
        if (_company == null)
        {
            MessageBox.Show("회사를 먼저 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        if (PayrollRows.Any(r => r.EmployeeId == employee.Id))
        {
            MessageBox.Show($"'{employee.Name}' 사원은 이미 등록되어 있습니다.", "중복 경고", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        var targetRow = GetNextAssignableRow();
        if (targetRow == null)
        {
            MessageBox.Show("사원을 배치할 빈 행이 추가될 수 없습니다.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        EnsureCompanyContext(targetRow);

        BeginSuppressDirty();
        try
        {
            targetRow.Detail.ResetAsync().Wait();
            targetRow.AssignEmployee(employee);
        }
        finally
        {
            EndSuppressDirty();
        }

        SelectedRow = targetRow;

        var loadedFromStore = TryLoadDraftForRow(targetRow);
        if (!loadedFromStore)
        {
            MarkDirty();
        }

        return true;
    }

    public void ClearSelectedRow()
    {
        if (SelectedRow == null || !SelectedRow.HasEmployee)
        {
            return;
        }

        var employeeName = SelectedRow.EmployeeName ?? "선택된 사원";
        var result = MessageBox.Show(
            $"'{employeeName}' 사원의 급여 데이터를 제거하시겠습니까?",
            "제거 확인",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var employeeId = SelectedRow.EmployeeId;

        SelectedRow.Detail.ResetAsync().Wait();
        SelectedRow.ResetEmployee();

        if (_company != null && employeeId.HasValue)
        {
            try
            {
                using var db = new AccountingDbContext();
                var draft = db.PayrollEntryDrafts
                    .FirstOrDefault(d =>
                        d.CompanyId == _company.Id &&
                        d.EmployeeId == employeeId.Value &&
                        d.AccrualYear == _accrualMonth.Year &&
                        d.AccrualMonth == _accrualMonth.Month);

                if (draft != null)
                {
                    db.PayrollEntryDrafts.Remove(draft);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"초안을 제거 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        MarkDirty();
    }

    private IReadOnlyList<PayrollEntryRowViewModel> AddBlankRows(int count)
    {
        if (count <= 0)
        {
            return Array.Empty<PayrollEntryRowViewModel>();
        }

        var addedRows = new List<PayrollEntryRowViewModel>(count);
        for (int i = 0; i < count; i++)
        {
            var row = new PayrollEntryRowViewModel(_taxTableProvider, _payItemService)
            {
                Sequence = PayrollRows.Count + 1
            };

            AttachRowHandlers(row);
            PayrollRows.Add(row);
            addedRows.Add(row);
        }

        RefreshSequences();
        return addedRows;
    }

    private PayrollEntryRowViewModel? GetNextAssignableRow()
    {
        var row = PayrollRows.FirstOrDefault(r => !r.HasEmployee);
        if (row != null)
        {
            return row;
        }

        var newRows = AddBlankRows(5);
        _ = ApplyCompanyContextToRowsAsync(newRows);
        return PayrollRows.FirstOrDefault(r => !r.HasEmployee);
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
            MessageBox.Show("회사를 먼저 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            using var db = new AccountingDbContext();
            var companyId = _company.Id;
            var accrualYear = _accrualMonth.Year;
            var accrualMonth = _accrualMonth.Month;

            var existingDrafts = await db.PayrollEntryDrafts
                .Where(d => d.CompanyId == companyId)
                .Where(d => d.AccrualYear == accrualYear && d.AccrualMonth == accrualMonth)
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
                        EmployeeId = employeeId,
                        AccrualYear = accrualYear,
                        AccrualMonth = accrualMonth
                    };
                    db.PayrollEntryDrafts.Add(draft);
                    existingMap[employeeId] = draft;
                }
                else
                {
                    draft.AccrualYear = accrualYear;
                    draft.AccrualMonth = accrualMonth;
                }

                await row.Detail.CopyToDraftAsync(draft);
                draft.FundingSource = SelectedFundingSource;
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
            MessageBox.Show("급여 입력 초안이 저장되었습니다.", "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
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
            .FirstOrDefault(d =>
                d.CompanyId == _company.Id &&
                d.EmployeeId == row.EmployeeId.Value &&
                d.AccrualYear == _accrualMonth.Year &&
                d.AccrualMonth == _accrualMonth.Month);

        if (draft == null)
        {
            return false;
        }

        EnsureCompanyContext(row);

        BeginSuppressDirty();
        try
        {
            row.Detail.LoadFromDraftAsync(draft).Wait();
        }
        finally
        {
            EndSuppressDirty();
        }

        return true;
    }

    private Task ApplyCompanyContextToRowsAsync(IEnumerable<PayrollEntryRowViewModel>? rows = null)
    {
        if (_company == null)
        {
            return Task.CompletedTask;
        }

        var targets = rows ?? PayrollRows;
        var tasks = targets.Select(row => row.ApplyCompanyAsync(_company)).ToArray();

        return tasks.Length == 0 ? Task.CompletedTask : Task.WhenAll(tasks);
    }

    private void EnsureCompanyContext(PayrollEntryRowViewModel row)
    {
        if (_company == null)
        {
            return;
        }

        row.ApplyCompanyAsync(_company).GetAwaiter().GetResult();
    }

    public DateTime AccrualMonth
    {
        get => _accrualMonth;
        set
        {
            var normalized = value == default
                ? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
                : new DateTime(value.Year, value.Month, 1);

            if (SetProperty(ref _accrualMonth, normalized))
            {
                if (_company != null)
                {
                    _ = ReloadDraftsAsync();
                }
            }
        }
    }
}
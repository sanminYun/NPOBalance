using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using NPOBalance.Data;
using NPOBalance.Models;

namespace NPOBalance.ViewModels;

public class EmployeeLookupViewModel : ObservableObject
{
    private readonly AccountingDbContext _context;
    private readonly Window _window;
    private readonly int _companyId;
    private string? _searchKeyword;
    private Employee? _selectedEmployee;

    public ObservableCollection<Employee> Employees { get; } = new();

    public string? SearchKeyword
    {
        get => _searchKeyword;
        set => SetProperty(ref _searchKeyword, value);
    }

    public Employee? SelectedEmployee
    {
        get => _selectedEmployee;
        set
        {
            if (SetProperty(ref _selectedEmployee, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public ICommand ConfirmCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }

    public EmployeeLookupViewModel(int companyId, Window window)
    {
        _companyId = companyId;
        _window = window;
        _context = new AccountingDbContext();

        ConfirmCommand = new RelayCommand(_ => ConfirmSelection(), _ => SelectedEmployee != null);
        SearchCommand = new RelayCommand(_ => LoadEmployees());
        ClearSearchCommand = new RelayCommand(_ =>
        {
            SearchKeyword = string.Empty;
            LoadEmployees();
        });

        LoadEmployees();
    }

    private async void LoadEmployees()
    {
        try
        {
            var query = _context.Employees
                .Where(e => e.CompanyId == _companyId && e.IsActive);

            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                string keyword = SearchKeyword.Trim();
                query = query.Where(e =>
                    e.EmployeeCode.Contains(keyword) ||
                    e.Name.Contains(keyword) ||
                    (e.ResidentId != null && e.ResidentId.Contains(keyword)));
            }

            var employees = await query
                .OrderBy(e => e.EmployeeCode)
                .ToListAsync();

            Employees.Clear();
            foreach (var employee in employees)
            {
                Employees.Add(employee);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"사원 목록을 불러오지 못했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ConfirmSelection()
    {
        if (SelectedEmployee == null)
        {
            MessageBox.Show("사원을 선택하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _window.DialogResult = true;
        _window.Close();
    }
}

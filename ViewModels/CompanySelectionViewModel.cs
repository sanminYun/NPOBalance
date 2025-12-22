using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using NPOBalance.Data;
using NPOBalance.Models;
using NPOBalance.Views;

namespace NPOBalance.ViewModels;

public class CompanySelectionViewModel : INotifyPropertyChanged
{
    private readonly AccountingDbContext _context;
    private readonly Window _window;
    private Company? _selectedCompany;

    public ObservableCollection<Company> Companies { get; set; }

    public Company? SelectedCompany
    {
        get => _selectedCompany;
        set
        {
            _selectedCompany = value;
            OnPropertyChanged();
        }
    }

    public ICommand ConfirmCommand { get; }
    public ICommand AddCompanyCommand { get; }

    public CompanySelectionViewModel(Window window)
    {
        _window = window;
        _context = new AccountingDbContext();
        Companies = new ObservableCollection<Company>();
        ConfirmCommand = new RelayCommand(ExecuteConfirm, CanExecuteConfirm);
        AddCompanyCommand = new RelayCommand(ExecuteAddCompany);
        LoadCompanies();
    }

    private async void LoadCompanies()
    {
        try
        {
            var activeCompanies = await _context.Companies
                .Where(c => c.IsActive)
                .OrderBy(c => c.CompanyCode)
                .ToListAsync();

            Companies.Clear();
            foreach (var company in activeCompanies)
            {
                Companies.Add(company);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"회사 목록을 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool CanExecuteConfirm(object? parameter)
    {
        return SelectedCompany != null;
    }

    private void ExecuteConfirm(object? parameter)
    {
        if (SelectedCompany != null)
        {
            _window.DialogResult = true;
            _window.Close();
        }
    }

    private async void ExecuteAddCompany(object? parameter)
    {
        var dialog = new CompanyAddDialog();
        if (dialog.ShowDialog() == true)
        {
            try
            {
                // 자동으로 회사코드 생성 (0000001 형식)
                var maxCompany = await _context.Companies
                    .OrderByDescending(c => c.CompanyCode)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (maxCompany != null && int.TryParse(maxCompany.CompanyCode, out int currentMax))
                {
                    nextNumber = currentMax + 1;
                }

                string newCompanyCode = nextNumber.ToString("D7");

                var newCompany = new Company
                {
                    CompanyCode = newCompanyCode,
                    Name = dialog.CompanyName,
                    FiscalYearStart = dialog.FiscalYearStart,
                    FiscalYearEnd = dialog.FiscalYearEnd,
                    BusinessNumber = dialog.BusinessNumber,
                    CorporateRegistrationNumber = dialog.CorporateRegistrationNumber,
                    RepresentativeName = dialog.RepresentativeName,
                    CompanyType = dialog.CompanyType,
                    TaxSource = dialog.TaxSource,
                    IsActive = true
                };

                _context.Companies.Add(newCompany);
                await _context.SaveChangesAsync();

                Companies.Add(newCompany);
                SelectedCompany = newCompany;

                MessageBox.Show($"회사가 성공적으로 등록되었습니다.\n회사코드: {newCompanyCode}", "등록 완료", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"회사 등록 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
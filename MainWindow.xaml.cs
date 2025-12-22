using System.Windows;
using System.Windows.Controls;
using NPOBalance.Models;
using NPOBalance.Services;
using NPOBalance.Views;

namespace NPOBalance;

public partial class MainWindow : Window
{
    private Company? _currentCompany;
    private PayrollEntryView? _cachedPayrollEntryView;

    public MainWindow()
    {
        InitializeComponent();
        InitializePayItemSettings();
    }

    private async void InitializePayItemSettings()
    {
        var service = new PayItemService();
        await service.InitializeDefaultsAsync();
    }

    public void SetCompany(Company company)
    {
        _currentCompany = company;
        CompanyContext.SetCompany(company);
        _cachedPayrollEntryView = null;
        Title = $"NPO 급여관리 - {company.Name}";
        ShowPlaceholder();
    }

    private void ShowPlaceholder()
    {
        ClearMenuSelection();
        ContentHost.Content = new PlaceholderView("메뉴를 선택하세요");
    }

    private void ClearMenuSelection()
    {
        EmployeeManagementMenuItem.Tag = null;
        PayrollEntryMenuItem.Tag = null;
    }

    private void SetMenuSelection(MenuItem menuItem)
    {
        ClearMenuSelection();
        menuItem.Tag = "Selected";
    }

    private async void EmployeeManagement_Click(object sender, RoutedEventArgs e)
    {
        if (_currentCompany == null)
        {
            MessageBox.Show("회사를 먼저 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SetMenuSelection(EmployeeManagementMenuItem);

        var view = new EmployeeManagementView();
        ContentHost.Content = view;
        if (view.ViewModel != null)
        {
            await view.ViewModel.LoadEmployeesAsync(_currentCompany);
        }
    }

    private async void PayrollEntry_Click(object sender, RoutedEventArgs e)
    {
        if (_currentCompany == null)
        {
            MessageBox.Show("회사를 먼저 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SetMenuSelection(PayrollEntryMenuItem);

        if (_cachedPayrollEntryView == null)
        {
            _cachedPayrollEntryView = new PayrollEntryView();
            if (_cachedPayrollEntryView.ViewModel != null)
            {
                await _cachedPayrollEntryView.ViewModel.InitializeAsync(_currentCompany);
            }
        }
        else if (_cachedPayrollEntryView.ViewModel != null)
        {
            if (_cachedPayrollEntryView.ViewModel.CurrentCompany?.Id != _currentCompany.Id)
            {
                await _cachedPayrollEntryView.ViewModel.InitializeAsync(_currentCompany);
            }
            else
            {
                await _cachedPayrollEntryView.ViewModel.RefreshEmployeeDataAsync();
            }
        }

        ContentHost.Content = _cachedPayrollEntryView;
    }

    private void PayItemSetting_Click(object sender, RoutedEventArgs e)
    {
        ClearMenuSelection();
        var view = new PayItemSettingView();
        ContentHost.Content = view;
    }

    private async void InsuranceRateSetting_Click(object sender, RoutedEventArgs e)
    {
        if (_currentCompany == null)
        {
            MessageBox.Show("회사를 먼저 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        ClearMenuSelection();
        
        var view = new InsuranceRateSettingView();
        ContentHost.Content = view;
        
        await view.LoadAsync(_currentCompany);
    }

    private void ReSelectCompany_Click(object sender, RoutedEventArgs e)
    {
        var companyWindow = new CompanySelectionWindow();
        if (companyWindow.ShowDialog() == true && companyWindow.SelectedCompany != null)
        {
            SetCompany(companyWindow.SelectedCompany);
            _cachedPayrollEntryView = null;
        }
    }
}
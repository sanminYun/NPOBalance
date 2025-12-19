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

        // 기존 뷰가 있으면 재사용하고 데이터만 새로고침
        if (_cachedPayrollEntryView == null)
        {
            _cachedPayrollEntryView = new PayrollEntryView();
            if (_cachedPayrollEntryView.ViewModel != null)
            {
                await _cachedPayrollEntryView.ViewModel.InitializeAsync(_currentCompany);
            }
        }
        else
        {
            // 사원 정보가 변경되었을 수 있으므로 새로고침
            if (_cachedPayrollEntryView.ViewModel != null)
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

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        // 기존 설정 메뉴 클릭 핸들러 (필요시 제거 가능)
    }

    private void ReSelectCompany_Click(object sender, RoutedEventArgs e)
    {
        var companyWindow = new CompanySelectionWindow();
        if (companyWindow.ShowDialog() == true && companyWindow.SelectedCompany != null)
        {
            SetCompany(companyWindow.SelectedCompany);
            _cachedPayrollEntryView = null; // 회사 변경 시 캐시 초기화
        }
    }
}
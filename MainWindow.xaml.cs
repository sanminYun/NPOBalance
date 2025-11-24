using System.Windows;
using NPOBalance.Models;
using NPOBalance.Views;

namespace NPOBalance
{
    public partial class MainWindow : Window
    {
        private Company? _selectedCompany;

        private readonly EmployeeManagementView _employeeView = new();
        private readonly PayrollEntryView _payrollEntryView = new();
        private readonly SettingsView _settingsView = new();

        public MainWindow()
        {
            InitializeComponent();
            ContentHost.Content = new PlaceholderView("회사를 먼저 선택하세요.");
            IsEnabled = false; // 초기에는 메뉴도 잠금
        }

        public void InitializeCompany(Company company)
        {
            _selectedCompany = company;
            Title = $"NPO 급여관리 - {company.Name}";
            IsEnabled = true; // 메뉴 활성화
            ContentHost.Content = _employeeView;
        }

        private void EmployeeManagement_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = _employeeView;
        }

        private void PayrollEntry_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = _payrollEntryView;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = _settingsView;
        }

        private void ReSelectCompany_Click(object sender, RoutedEventArgs e)
        {
            var selectionWindow = new CompanySelectionWindow { Owner = this };
            var result = selectionWindow.ShowDialog();

            if (result == true && selectionWindow.SelectedCompany != null)
            {
                InitializeCompany(selectionWindow.SelectedCompany);
            }
            else
            {
                // 취소한 경우: 기존 회사가 있으면 유지, 없으면 안내 화면 유지
                if (_selectedCompany == null)
                {
                    ContentHost.Content = new PlaceholderView("회사를 선택하지 않았습니다.");
                }
            }
        }
    }
}
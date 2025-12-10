using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NPOBalance.Models;
using NPOBalance.ViewModels;
using NPOBalance.Views;

namespace NPOBalance
{
    public partial class MainWindow : Window
    {
        private Company? _selectedCompany;
        private MenuItem? _activeMenuItem;

        private readonly EmployeeManagementView _employeeView = new();
        private readonly PayrollEntryView _payrollEntryView = new();
        private readonly SettingsView _settingsView = new();

        public MainWindow()
        {
            InitializeComponent();
            Title = "NPO 급여관리 - 회사 선택";
            ContentHost.Content = new PlaceholderView("회사를 먼저 선택하세요.");
            IsEnabled = false;
            SetActiveMenuItem(null);
        }

        public async Task InitializeCompany(Company company)
        {
            _selectedCompany = company;

            if (_employeeView.ViewModel is EmployeeManagementViewModel employeeVm)
            {
                await employeeVm.LoadEmployeesAsync(_selectedCompany);
            }

            if (_payrollEntryView.ViewModel is PayrollEntryViewModel payrollVm)
            {
                await payrollVm.InitializeAsync(_selectedCompany);
            }

            IsEnabled = true;
            ContentHost.Content = _employeeView;
            UpdateTitle("사원 정보");
            SetActiveMenuItem(EmployeeManagementMenuItem);
        }

        private void UpdateTitle(string menuName)
        {
            if (_selectedCompany != null)
            {
                Title = $"NPO 급여관리 - {_selectedCompany.Name} - {menuName}";
            }
            else
            {
                Title = $"NPO 급여관리 - {menuName}";
            }
        }

        private void SetActiveMenuItem(MenuItem? menuItem)
        {
            if (_activeMenuItem != null)
            {
                _activeMenuItem.FontWeight = FontWeights.Normal;
            }

            _activeMenuItem = menuItem;

            if (_activeMenuItem != null)
            {
                _activeMenuItem.FontWeight = FontWeights.Bold;
            }
        }

        private void EmployeeManagement_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = _employeeView;
            UpdateTitle("사원 정보");
            SetActiveMenuItem(EmployeeManagementMenuItem);
        }

        private void PayrollEntry_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = _payrollEntryView;
            UpdateTitle("급여입력");
            SetActiveMenuItem(PayrollEntryMenuItem);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = _settingsView;
            UpdateTitle("설정");
            SetActiveMenuItem(SettingsMenuItem);
        }

        private async void ReSelectCompany_Click(object sender, RoutedEventArgs e)
        {
            UpdateTitle("회사 다시 선택");
            var selectionWindow = new CompanySelectionWindow { Owner = this };
            var result = selectionWindow.ShowDialog();

            if (result == true && selectionWindow.SelectedCompany != null)
            {
                await InitializeCompany(selectionWindow.SelectedCompany);
            }
            else
            {
                if (ContentHost.Content is EmployeeManagementView)
                {
                    UpdateTitle("사원 정보");
                    SetActiveMenuItem(EmployeeManagementMenuItem);
                }
                else if (ContentHost.Content is PayrollEntryView)
                {
                    UpdateTitle("급여입력");
                    SetActiveMenuItem(PayrollEntryMenuItem);
                }
                else if (ContentHost.Content is SettingsView)
                {
                    UpdateTitle("설정");
                    SetActiveMenuItem(SettingsMenuItem);
                }
                else if (_selectedCompany == null)
                {
                    Title = "NPO 급여관리 - 회사 선택";
                    SetActiveMenuItem(null);
                }
            }
        }
    }
}
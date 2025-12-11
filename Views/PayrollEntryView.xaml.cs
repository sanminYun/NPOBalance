using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NPOBalance.ViewModels;

namespace NPOBalance.Views
{
    public partial class PayrollEntryView : UserControl
    {
        public PayrollEntryViewModel? ViewModel => DataContext as PayrollEntryViewModel;

        public PayrollEntryView()
        {
            InitializeComponent();
            this.Loaded += PayrollEntryView_Loaded;
            this.Unloaded += PayrollEntryView_Unloaded;
        }

        private void PayrollEntryView_Loaded(object sender, RoutedEventArgs e)
        {
            // UserControl이 로드되면 포커스를 받을 수 있도록 설정
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.PreviewKeyDown += Window_PreviewKeyDown;
            }
        }

        private void PayrollEntryView_Unloaded(object sender, RoutedEventArgs e)
        {
            // 메모리 누수 방지를 위해 이벤트 핸들러 제거
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.PreviewKeyDown -= Window_PreviewKeyDown;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                e.Handled = true;
                OpenEmployeeLookup();
            }
        }

        private void PayrollRowGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                e.Handled = true;
                OpenEmployeeLookup();
            }
        }

        private void SelectEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenEmployeeLookup();
        }

        private void OpenEmployeeLookup()
        {
            if (ViewModel?.CurrentCompany == null)
            {
                MessageBox.Show("회사를 먼저 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 이미 등록된 사원 ID 목록 가져오기
            var registeredEmployeeIds = ViewModel.PayrollRows
                .Where(r => r.HasEmployee)
                .Select(r => r.EmployeeId!.Value)
                .ToList();

            var dialog = new EmployeeLookupDialog(ViewModel.CurrentCompany.Id, registeredEmployeeIds)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.SelectedEmployee != null)
            {
                ViewModel.TryAssignEmployee(dialog.SelectedEmployee);
            }
        }
    }
}
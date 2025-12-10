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
                MessageBox.Show("회사를 먼저 선택하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new EmployeeLookupDialog(ViewModel.CurrentCompany.Id)
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
using System.Windows.Controls;
using NPOBalance.ViewModels;

namespace NPOBalance.Views
{
    public partial class EmployeeManagementView : UserControl
    {
        public EmployeeManagementViewModel? ViewModel => DataContext as EmployeeManagementViewModel;

        public EmployeeManagementView()
        {
            InitializeComponent();
        }
    }
}
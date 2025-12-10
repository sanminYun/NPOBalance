using System.Windows;
using System.Windows.Input;
using NPOBalance.Models;
using NPOBalance.ViewModels;

namespace NPOBalance.Views;

public partial class EmployeeLookupDialog : Window
{
    private readonly EmployeeLookupViewModel _viewModel;

    public Employee? SelectedEmployee => _viewModel.SelectedEmployee;

    public EmployeeLookupDialog(int companyId)
    {
        InitializeComponent();
        _viewModel = new EmployeeLookupViewModel(companyId, this);
        DataContext = _viewModel;
    }

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && _viewModel.SearchCommand.CanExecute(null))
        {
            _viewModel.SearchCommand.Execute(null);
        }
    }
}

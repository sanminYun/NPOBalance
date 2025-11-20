using System.Windows;
using NPOBalance.ViewModels;

namespace NPOBalance.Views;

public partial class CompanySelectionWindow : Window
{
    public CompanySelectionWindow()
    {
        InitializeComponent();
        DataContext = new CompanySelectionViewModel();
    }
}
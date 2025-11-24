using System.Windows;
using NPOBalance.Models;
using NPOBalance.ViewModels;

namespace NPOBalance.Views;

public partial class CompanySelectionWindow : Window
{
    private readonly CompanySelectionViewModel _viewModel;
    
    public Company? SelectedCompany => _viewModel.SelectedCompany;
    
    public CompanySelectionWindow()
    {
        InitializeComponent();
        _viewModel = new CompanySelectionViewModel(this);
        DataContext = _viewModel;
        
        // 데이터 로드 후 마지막 행으로 스크롤
        Loaded += CompanySelectionWindow_Loaded;
    }

    private void CompanySelectionWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (CompaniesDataGrid.Items.Count > 0)
        {
            // 마지막 행으로 스크롤
            CompaniesDataGrid.ScrollIntoView(CompaniesDataGrid.Items[CompaniesDataGrid.Items.Count - 1]);
        }
    }
}
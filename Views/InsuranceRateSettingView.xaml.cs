using System.Windows.Controls;
using NPOBalance.Models;
using NPOBalance.ViewModels;

namespace NPOBalance.Views;

public partial class InsuranceRateSettingView : UserControl
{
    public InsuranceRateSettingViewModel? ViewModel => DataContext as InsuranceRateSettingViewModel;

    public InsuranceRateSettingView()
    {
        InitializeComponent();
        DataContext = new InsuranceRateSettingViewModel();
    }

    public async Task LoadAsync(Company company)
    {
        if (ViewModel != null)
        {
            await ViewModel.LoadAsync(company);
        }
    }
}
using System.Windows.Controls;
using NPOBalance.ViewModels;

namespace NPOBalance.Views
{
    public partial class PayItemSettingView : UserControl
    {
        public PayItemSettingViewModel? ViewModel => DataContext as PayItemSettingViewModel;

        public PayItemSettingView()
        {
            InitializeComponent();
            DataContext = new PayItemSettingViewModel();
            Loaded += async (s, e) => await ViewModel!.LoadAsync();
        }
    }
}
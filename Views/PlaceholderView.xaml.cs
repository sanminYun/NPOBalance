using System.Windows.Controls;

namespace NPOBalance.Views
{
    public partial class PlaceholderView : UserControl
    {
        public PlaceholderView(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }
    }
}
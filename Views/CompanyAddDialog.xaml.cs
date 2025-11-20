using System.Windows;

namespace NPOBalance.Views;

public partial class CompanyAddDialog : Window
{
    public string CompanyName => CompanyNameTextBox.Text.Trim();
    public string BusinessNumber => BusinessNumberTextBox.Text.Trim();

    public CompanyAddDialog()
    {
        InitializeComponent();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CompanyName))
        {
            MessageBox.Show("회사명을 입력하세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
            CompanyNameTextBox.Focus();
            return;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
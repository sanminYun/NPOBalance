using System.Windows;

namespace NPOBalance.Views;

public partial class CompanyAddDialog : Window
{
    public string CompanyName => CompanyNameTextBox.Text.Trim();
    public DateTime FiscalYearStart => FiscalYearStartDatePicker.SelectedDate ?? DateTime.Now;
    public DateTime FiscalYearEnd => FiscalYearEndDatePicker.SelectedDate ?? DateTime.Now;
    public string BusinessNumber => BusinessNumberTextBox.Text.Trim();
    public string CorporateRegistrationNumber => CorporateRegistrationNumberTextBox.Text.Trim();
    public string RepresentativeName => RepresentativeNameTextBox.Text.Trim();
    public string CompanyType => (CompanyTypeComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "법인";
    public string TaxSource => (TaxSourceComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "일반";

    public CompanyAddDialog()
    {
        InitializeComponent();
        var today = DateTime.Now;
        FiscalYearStartDatePicker.SelectedDate = new DateTime(today.Year, 1, 1);
        FiscalYearEndDatePicker.SelectedDate = new DateTime(today.Year, 12, 31);
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CompanyName))
        {
            MessageBox.Show("회사명을 입력하세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
            CompanyNameTextBox.Focus();
            return;
        }

        if (FiscalYearStartDatePicker.SelectedDate == null)
        {
            MessageBox.Show("회계연도 시작일을 선택하세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
            FiscalYearStartDatePicker.Focus();
            return;
        }

        if (FiscalYearEndDatePicker.SelectedDate == null)
        {
            MessageBox.Show("회계연도 종료일을 선택하세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
            FiscalYearEndDatePicker.Focus();
            return;
        }

        if (FiscalYearEndDatePicker.SelectedDate < FiscalYearStartDatePicker.SelectedDate)
        {
            MessageBox.Show("회계연도 종료일은 시작일보다 이후여야 합니다.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
            FiscalYearEndDatePicker.Focus();
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
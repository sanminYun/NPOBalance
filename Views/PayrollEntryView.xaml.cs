using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using NPOBalance.Data;
using NPOBalance.Models;
using NPOBalance.Services;
using NPOBalance.ViewModels;
using Calendar = System.Windows.Controls.Calendar;

namespace NPOBalance.Views;

public partial class PayrollEntryView : UserControl
{
    private readonly PayItemService _payItemService = new();
    private List<string> _taxableEarningsItems = new();
    private List<string> _nonTaxableEarningsItems = new();
    private List<string> _retirementItems = new();
    private Calendar? _accrualCalendar;

    public PayrollEntryViewModel? ViewModel => DataContext as PayrollEntryViewModel;

    public PayrollEntryView()
    {
        InitializeComponent();
        Loaded += PayrollEntryView_Loaded;
        Unloaded += PayrollEntryView_Unloaded;
    }

    private async void PayrollEntryView_Loaded(object sender, RoutedEventArgs e)
    {
        // UserControl이 로드되면 포커스를 받을 수 있도록 설정
        var window = Window.GetWindow(this);
        if (window != null)
        {
            window.PreviewKeyDown += Window_PreviewKeyDown;
        }

        // PayItem 로드 및 동적 섹션 생성
        await LoadPayItemsAsync();
        RebuildDynamicSections();

        // SelectedRow 변경 이벤트 구독
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void PayrollEntryView_Unloaded(object sender, RoutedEventArgs e)
    {
        // 메모리 누수 방지를 위해 이벤트 핸들러 제거
        var window = Window.GetWindow(this);
        if (window != null)
        {
            window.PreviewKeyDown -= Window_PreviewKeyDown;
        }

        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PayrollEntryViewModel.SelectedRow))
        {
            // 선택된 행이 변경되면 동적 섹션 다시 생성
            RebuildDynamicSections();
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F2)
        {
            e.Handled = true;
            OpenEmployeeLookup();
        }
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

    private async void OpenEmployeeLookup()
    {
        var currentCompany = CompanyContext.CurrentCompany ?? ViewModel?.CurrentCompany;
        if (currentCompany == null)
        {
            MessageBox.Show("회사를 먼저 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // 이미 배치된 사원 ID 목록을 준비합니다.
        var registeredEmployeeIds = ViewModel!.PayrollRows
            .Where(r => r.HasEmployee)
            .Select(r => r.EmployeeId!.Value)
            .ToList();

        var dialog = new EmployeeLookupDialog(currentCompany.Id, registeredEmployeeIds)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true && dialog.SelectedEmployee != null)
        {
            var success = ViewModel.TryAssignEmployee(dialog.SelectedEmployee);
            if (success)
            {
                // DataGrid 즉시 업데이트를 위해 Dispatcher 사용
                await Dispatcher.InvokeAsync(() =>
                {
                    PayrollRowGrid.Items.Refresh();
                    PayrollRowGrid.UpdateLayout();
                }, System.Windows.Threading.DispatcherPriority.Background);

                await SaveDraftForCurrentRowAsync();
            }
        }
    }

    private async Task SaveDraftForCurrentRowAsync()
    {
        if (ViewModel?.CurrentCompany == null || ViewModel.SelectedRow?.EmployeeId == null)
        {
            return;
        }

        try
        {
            using var db = new AccountingDbContext();
            var companyId = ViewModel.CurrentCompany.Id;
            var employeeId = ViewModel.SelectedRow.EmployeeId.Value;
            var accrualYear = ViewModel.AccrualMonth.Year;
            var accrualMonth = ViewModel.AccrualMonth.Month;

            var draft = await db.PayrollEntryDrafts
                .FirstOrDefaultAsync(d =>
                    d.CompanyId == companyId &&
                    d.EmployeeId == employeeId &&
                    d.AccrualYear == accrualYear &&
                    d.AccrualMonth == accrualMonth);

            if (draft == null)
            {
                draft = new PayrollEntryDraft
                {
                    CompanyId = companyId,
                    EmployeeId = employeeId,
                    AccrualYear = accrualYear,
                    AccrualMonth = accrualMonth
                };
                db.PayrollEntryDrafts.Add(draft);
            }
            else
            {
                draft.AccrualYear = accrualYear;
                draft.AccrualMonth = accrualMonth;
            }

            await ViewModel.SelectedRow.Detail.CopyToDraftAsync(draft);
            draft.FundingSource = ViewModel.SelectedFundingSource;

            await db.SaveChangesAsync();
        }
        catch
        {
            // 초안 저장 실패는 무시
        }
    }

    private async Task LoadPayItemsAsync()
    {
        _taxableEarningsItems = await _payItemService.GetPayItemsAsync(PayItemService.TaxableEarnings);
        _nonTaxableEarningsItems = await _payItemService.GetPayItemsAsync(PayItemService.NonTaxableEarnings);
        _retirementItems = await _payItemService.GetPayItemsAsync(PayItemService.Retirement);
    }

    private void RebuildDynamicSections()
    {
        BuildSection(TaxableEarningsPanel, PayItemService.TaxableEarnings, _taxableEarningsItems, showSubtotal: true, subtotalLabel: "소계 (A)");
        BuildSection(NonTaxableEarningsPanel, PayItemService.NonTaxableEarnings, _nonTaxableEarningsItems, showSubtotal: true, subtotalLabel: "소계 (B)");
        BuildSection(RetirementPanel, PayItemService.Retirement, _retirementItems, showSubtotal: false);
    }

    private void BuildSection(Panel panel, string sectionKey, List<string> items, bool showSubtotal, string subtotalLabel = "")
    {
        panel.Children.Clear();

        if (ViewModel?.SelectedRow?.Detail == null)
        {
            return;
        }

        var grid = new Grid { Margin = new Thickness(0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });

        for (int i = 0; i < items.Count; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var label = new TextBlock
            {
                Text = items[i],
                Style = (Style)FindResource("FormLabelStyle")
            };
            Grid.SetRow(label, i);
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            var textBox = new TextBox
            {
                Style = (Style)FindResource("FormInputBoxStyle"),
                Tag = new SectionItemTag { SectionKey = sectionKey, Index = i }
            };

            // Converter를 사용한 양방향 바인딩
            var binding = new Binding(".")
            {
                Source = ViewModel.SelectedRow.Detail,
                Converter = new PayItemValueConverter(),
                ConverterParameter = new SectionItemTag { SectionKey = sectionKey, Index = i },
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            textBox.SetBinding(TextBox.TextProperty, binding);
            
            // 숫자만 입력 가능하도록 제한
            textBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
            
            // 실시간 콤마 포맷팅
            textBox.TextChanged += NumericTextBox_TextChanged;

            Grid.SetRow(textBox, i);
            Grid.SetColumn(textBox, 1);
            grid.Children.Add(textBox);
        }

        if (showSubtotal)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var subtotalTextBlock = new TextBlock
            {
                Text = subtotalLabel,
                Style = (Style)FindResource("FormLabelStyle"),
                Margin = new Thickness(0, 12, 0, 0)
            };
            Grid.SetRow(subtotalTextBlock, items.Count);
            Grid.SetColumn(subtotalTextBlock, 0);
            grid.Children.Add(subtotalTextBlock);

            var subtotalBox = new TextBox
            {
                Style = (Style)FindResource("ReadonlyBox"),
                Margin = new Thickness(0, 12, 0, 0)
            };

            var bindingPath = sectionKey == PayItemService.TaxableEarnings 
                ? "TaxableEarningsSubtotal" 
                : "NonTaxableEarningsSubtotal";

            subtotalBox.SetBinding(TextBox.TextProperty, new Binding(bindingPath)
            {
                Source = ViewModel.SelectedRow.Detail,
                Mode = BindingMode.OneWay,
                StringFormat = "{0:N0}"
            });

            Grid.SetRow(subtotalBox, items.Count);
            Grid.SetColumn(subtotalBox, 1);
            grid.Children.Add(subtotalBox);
        }

        panel.Children.Add(grid);
    }

    private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // 숫자만 입력 허용
        e.Handled = !IsTextNumeric(e.Text);
    }

    private void NumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox || textBox.Tag is not SectionItemTag tag)
        {
            return;
        }

        // 이벤트 핸들러가 재귀적으로 호출되는 것을 방지하기 위한 플래그
        if (textBox.GetValue(IsFormattingProperty) is bool isFormatting && isFormatting)
        {
            return;
        }

        // 커서 위치 저장
        int cursorPosition = textBox.SelectionStart;
        string originalText = textBox.Text;
        
        // 콤마 제거한 원본 숫자
        string cleanText = originalText.Replace(",", "");

        // 숫자로 파싱 가능한 경우에만 처리
        if (decimal.TryParse(cleanText, out var value))
        {
            // ViewModel 업데이트
            ViewModel?.SelectedRow?.Detail.SetValue(tag.SectionKey, tag.Index, value);
            
            // 천단위 콤마 적용된 텍스트
            string formattedText = value.ToString("N0");
            
            // 텍스트가 변경된 경우에만 업데이트 (무한 루프 방지)
            if (originalText != formattedText)
            {
                // 콤마 개수 차이 계산하여 커서 위치 조정
                int commasBeforeCursor = originalText.Take(cursorPosition).Count(c => c == ',');
                int digitsBeforeCursor = originalText.Take(cursorPosition).Count(c => char.IsDigit(c));
                
                // 포맷팅 플래그 설정
                textBox.SetValue(IsFormattingProperty, true);
                
                // 텍스트 업데이트
                textBox.Text = formattedText;
                
                // 새로운 커서 위치 계산
                int newCursorPosition = 0;
                int digitCount = 0;
                
                for (int i = 0; i < formattedText.Length; i++)
                {
                    if (char.IsDigit(formattedText[i]))
                    {
                        digitCount++;
                        if (digitCount >= digitsBeforeCursor)
                        {
                            newCursorPosition = i + 1;
                            break;
                        }
                    }
                }
                
                // 커서 위치가 유효한 범위 내에 있는지 확인
                textBox.SelectionStart = Math.Min(newCursorPosition, formattedText.Length);
                
                // 포맷팅 플래그 해제
                textBox.SetValue(IsFormattingProperty, false);
            }
        }
        else if (string.IsNullOrWhiteSpace(cleanText))
        {
            // 빈 문자열인 경우 0으로 설정
            ViewModel?.SelectedRow?.Detail.SetValue(tag.SectionKey, tag.Index, 0);
        }
    }

    private static bool IsTextNumeric(string text)
    {
        return text.All(char.IsDigit);
    }

    // 포맷팅 중임을 나타내는 Attached Property
    private static readonly DependencyProperty IsFormattingProperty =
        DependencyProperty.RegisterAttached(
            "IsFormatting",
            typeof(bool),
            typeof(PayrollEntryView),
            new PropertyMetadata(false));

    private class SectionItemTag
    {
        public string SectionKey { get; set; } = string.Empty;
        public int Index { get; set; }
    }

    private class PayItemValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not SectionItemTag tag || value is not PayrollEntryDetailViewModel viewModel)
            {
                return "0";
            }

            var decimalValue = viewModel.GetValue(tag.SectionKey, tag.Index);
            return decimalValue.ToString("N0", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text)
            {
                return 0m;
            }

            var cleanedText = text.Replace(",", "").Replace(" ", "");
            
            if (decimal.TryParse(cleanedText, NumberStyles.Any, culture, out var result))
            {
                return result;
            }

            return 0m;
        }
    }

    private void AccrualMonthPicker_CalendarOpened(object sender, RoutedEventArgs e)
    {
        if (sender is not DatePicker picker)
        {
            return;
        }

        _accrualCalendar = picker.Template.FindName("PART_Calendar", picker) as Calendar;
        if (_accrualCalendar == null)
        {
            return;
        }

        var referenceDate = ViewModel?.AccrualMonth ?? DateTime.Today;
        _accrualCalendar.DisplayMode = CalendarMode.Year;
        _accrualCalendar.DisplayDate = referenceDate;
        _accrualCalendar.SelectedDate = referenceDate;
        _accrualCalendar.DisplayModeChanged -= AccrualCalendar_DisplayModeChanged;
        _accrualCalendar.DisplayModeChanged += AccrualCalendar_DisplayModeChanged;
    }

    private void AccrualMonthPicker_CalendarClosed(object sender, RoutedEventArgs e)
    {
        if (_accrualCalendar != null)
        {
            _accrualCalendar.DisplayModeChanged -= AccrualCalendar_DisplayModeChanged;
            _accrualCalendar = null;
        }
    }

    private void AccrualCalendar_DisplayModeChanged(object? sender, CalendarModeChangedEventArgs e)
    {
        if (sender is not Calendar calendar || AccrualMonthPicker == null || calendar.DisplayMode != CalendarMode.Month)
        {
            return;
        }

        var selectedMonth = new DateTime(calendar.DisplayDate.Year, calendar.DisplayDate.Month, 1);
        calendar.SelectedDate = selectedMonth;
        AccrualMonthPicker.SelectedDate = selectedMonth;
        AccrualMonthPicker.IsDropDownOpen = false;
        calendar.DisplayMode = CalendarMode.Year;
    }

    private async void ExportPdf_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.SelectedRow == null || !ViewModel.SelectedRow.HasEmployee)
        {
            MessageBox.Show("PDF로 출력할 사원을 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (ViewModel.CurrentCompany == null)
        {
            MessageBox.Show("회사 정보를 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            using var db = new AccountingDbContext();
            var employee = await db.Employees
                .FirstOrDefaultAsync(e => e.Id == ViewModel.SelectedRow.EmployeeId);

            if (employee == null)
            {
                MessageBox.Show("사원 정보를 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var pdfService = new PayrollPdfService();
            var pdfBytes = pdfService.GeneratePayrollSlip(
                ViewModel.CurrentCompany,
                employee,
                ViewModel.SelectedRow.Detail,
                ViewModel.AccrualMonth,
                ViewModel.PaymentDate,
                ViewModel.SelectedFundingSource,
                _taxableEarningsItems,
                _nonTaxableEarningsItems,
                _retirementItems);

            var defaultFileName = $"{ViewModel.AccrualMonth:yyyy-MM}_{employee.Name}_급여명세서.pdf";

            // 미리보기 창 열기
            var previewWindow = new PdfPreviewWindow(pdfBytes, defaultFileName)
            {
                Owner = Window.GetWindow(this)
            };

            previewWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"PDF 생성 중 오류가 발생했습니다:\n{ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
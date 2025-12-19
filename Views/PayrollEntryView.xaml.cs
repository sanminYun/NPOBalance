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

namespace NPOBalance.Views;

public partial class PayrollEntryView : UserControl
{
    private readonly PayItemService _payItemService = new();
    private List<string> _taxableEarningsItems = new();
    private List<string> _nonTaxableEarningsItems = new();
    private List<string> _retirementItems = new();

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
        if (ViewModel?.CurrentCompany == null)
        {
            MessageBox.Show("회사를 먼저 선택하세요.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // 이미 등록된 사원 ID 목록 가져오기
        var registeredEmployeeIds = ViewModel.PayrollRows
            .Where(r => r.HasEmployee)
            .Select(r => r.EmployeeId!.Value)
            .ToList();

        var dialog = new EmployeeLookupDialog(ViewModel.CurrentCompany.Id, registeredEmployeeIds)
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

            var draft = await db.PayrollEntryDrafts
                .FirstOrDefaultAsync(d => d.CompanyId == companyId && d.EmployeeId == employeeId);

            if (draft == null)
            {
                draft = new PayrollEntryDraft
                {
                    CompanyId = companyId,
                    EmployeeId = employeeId
                };
                db.PayrollEntryDrafts.Add(draft);
            }

            await ViewModel.SelectedRow.Detail.CopyToDraftAsync(draft);
            draft.FundingSource = ViewModel.SelectedFundingSource;

            await db.SaveChangesAsync();
        }
        catch
        {
            // 초안 저장 실패는 무시 (사용자에게 알리지 않음)
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
            textBox.TextChanged += (s, e) =>
            {
                if (s is TextBox tb && tb.Tag is SectionItemTag tag)
                {
                    if (decimal.TryParse(tb.Text.Replace(",", ""), out var value))
                    {
                        ViewModel.SelectedRow.Detail.SetValue(tag.SectionKey, tag.Index, value);
                    }
                    else if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        ViewModel.SelectedRow.Detail.SetValue(tag.SectionKey, tag.Index, 0);
                    }
                }
            };

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
}
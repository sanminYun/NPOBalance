using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NPOBalance.Services;

namespace NPOBalance.ViewModels;

public class PayItemSettingViewModel : ObservableObject
{
    private readonly PayItemService _payItemService;
    private const int MaxItemsPerSection = 15;

    public ObservableCollection<PayItemSectionViewModel> Sections { get; }
    public ICommand SaveCommand { get; }

    public PayItemSettingViewModel()
    {
        _payItemService = new PayItemService();

        Sections = new ObservableCollection<PayItemSectionViewModel>
        {
            new PayItemSectionViewModel("과세급상여", PayItemService.TaxableEarnings),
            new PayItemSectionViewModel("비과세급상여", PayItemService.NonTaxableEarnings),
            new PayItemSectionViewModel("4대보험 공제항목", PayItemService.InsuranceDeduction),
            new PayItemSectionViewModel("소득세 공제항목", PayItemService.IncomeTaxDeduction),
            new PayItemSectionViewModel("4대보험 기업부담금", PayItemService.EmployerInsurance),
            new PayItemSectionViewModel("퇴직적립금", PayItemService.Retirement),
            new PayItemSectionViewModel("재원 설정", PayItemService.FundingSource)
        };

        SaveCommand = new RelayCommand(async _ => await SaveAsync());
    }

    public async Task LoadAsync()
    {
        foreach (var section in Sections)
        {
            var items = await _payItemService.GetPayItemsAsync(section.SectionKey);

            section.Items.Clear();
            for (int i = 0; i < MaxItemsPerSection; i++)
            {
                var itemName = i < items.Count ? items[i] : string.Empty;
                section.Items.Add(new PayItemViewModel { Index = i + 1, Name = itemName });
            }
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            foreach (var section in Sections)
            {
                var items = section.Items
                    .Where(item => !string.IsNullOrWhiteSpace(item.Name))
                    .Select(item => item.Name!)
                    .ToList();

                await _payItemService.SavePayItemsAsync(section.SectionKey, items);
            }

            MessageBox.Show("계정과목 설정이 저장되었습니다.", "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

public class PayItemSectionViewModel : ObservableObject
{
    public string DisplayName { get; }
    public string SectionKey { get; }
    public ObservableCollection<PayItemViewModel> Items { get; }

    public PayItemSectionViewModel(string displayName, string sectionKey)
    {
        DisplayName = displayName;
        SectionKey = sectionKey;
        Items = new ObservableCollection<PayItemViewModel>();
    }
}

public class PayItemViewModel : ObservableObject
{
    private int _index;
    private string? _name;

    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
    }

    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using NPOBalance.Data;
using NPOBalance.Models;
using NPOBalance.Views;

namespace NPOBalance.ViewModels;

public class CompanySelectionViewModel : INotifyPropertyChanged
{
    private readonly AccountingDbContext _context;
    private Company? _selectedCompany;

    public ObservableCollection<Company> Companies { get; set; }

    public Company? SelectedCompany
    {
        get => _selectedCompany;
        set
        {
            _selectedCompany = value;
            OnPropertyChanged();
        }
    }

    public ICommand ConfirmCommand { get; }
    public ICommand AddCompanyCommand { get; }

    public CompanySelectionViewModel()
    {
        _context = new AccountingDbContext();
        Companies = new ObservableCollection<Company>();
        ConfirmCommand = new RelayCommand(ExecuteConfirm, CanExecuteConfirm);
        AddCompanyCommand = new RelayCommand(ExecuteAddCompany);
        LoadCompanies();
    }

    private async void LoadCompanies()
    {
        var activeCompanies = await _context.Companies
            .Where(c => c.IsActive)
            .ToListAsync();

        Companies.Clear();
        foreach (var company in activeCompanies)
        {
            Companies.Add(company);
        }
    }

    private bool CanExecuteConfirm(object? parameter)
    {
        return SelectedCompany != null;
    }

    private void ExecuteConfirm(object? parameter)
    {
        if (SelectedCompany != null)
        {
            // Hook to open main window later
        }
    }

    private async void ExecuteAddCompany(object? parameter)
    {
        var dialog = new CompanyAddDialog();
        if (dialog.ShowDialog() == true)
        {
            var newCompany = new Company
            {
                Name = dialog.CompanyName,
                BusinessNumber = dialog.BusinessNumber,
                IsActive = true
            };

            _context.Companies.Add(newCompany);
            await _context.SaveChangesAsync();

            Companies.Add(newCompany);
            SelectedCompany = newCompany;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
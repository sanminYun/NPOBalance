using System.ComponentModel;
using System.Threading.Tasks;
using NPOBalance.Models;
using NPOBalance.Services;

namespace NPOBalance.ViewModels;

public class PayrollEntryRowViewModel : ObservableObject
{
    private readonly PayrollEntryDetailViewModel _detail;
    private int _sequence;
    private int? _employeeId;
    private string? _employeeCode;
    private string? _employeeName;
    private string? _department;

    public PayrollEntryRowViewModel(SimplifiedTaxTableProvider taxTableProvider, PayItemService payItemService)
    {
        _detail = new PayrollEntryDetailViewModel(taxTableProvider, payItemService);
        _detail.PropertyChanged += DetailOnPropertyChanged;
    }

    public PayrollEntryDetailViewModel Detail => _detail;

    public int Sequence
    {
        get => _sequence;
        set => SetProperty(ref _sequence, value);
    }

    public int? EmployeeId
    {
        get => _employeeId;
        private set
        {
            if (SetProperty(ref _employeeId, value))
            {
                OnPropertyChanged(nameof(HasEmployee));
            }
        }
    }

    public string? EmployeeCode
    {
        get => _employeeCode;
        private set => SetProperty(ref _employeeCode, value);
    }

    public string? EmployeeName
    {
        get => _employeeName;
        private set => SetProperty(ref _employeeName, value);
    }

    public string? Department
    {
        get => _department;
        private set => SetProperty(ref _department, value);
    }

    public bool HasEmployee => EmployeeId.HasValue;

    public decimal NetPay => Detail.NetPay;
    public decimal CompanyBurden => Detail.EmployerTotalBurden;

    public void AssignEmployee(Employee employee)
    {
        EmployeeId = employee.Id;
        EmployeeCode = employee.EmployeeCode;
        EmployeeName = employee.Name;
        Department = employee.Department;
        Detail.ApplyEmployeeContext(employee);
    }

    public void RefreshEmployeeContext(Employee employee)
    {
        // 사원 기본 정보 갱신
        EmployeeCode = employee.EmployeeCode;
        EmployeeName = employee.Name;
        Department = employee.Department;
        
        // 예상총급여액 및 부양가족수 갱신
        Detail.ApplyEmployeeContext(employee);
    }

    public void ResetEmployee()
    {
        EmployeeId = null;
        EmployeeCode = null;
        EmployeeName = null;
        Department = null;
    }

    private void DetailOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PayrollEntryDetailViewModel.NetPay) ||
            e.PropertyName == nameof(PayrollEntryDetailViewModel.EmployerTotalBurden))
        {
            OnPropertyChanged(nameof(NetPay));
            OnPropertyChanged(nameof(CompanyBurden));
        }
    }

    public Task ApplyCompanyAsync(Company company)
    {
        return _detail.SetCompanyAsync(company);
    }
}
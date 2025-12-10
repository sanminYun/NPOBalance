using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using NPOBalance.Data;
using NPOBalance.Models;

namespace NPOBalance.ViewModels
{
    public class EmployeeManagementViewModel : INotifyPropertyChanged
    {
        private const int DefaultRowCount = 100;
        private readonly AccountingDbContext _context;
        private Company _company = null!;
        private Employee? _selectedEmployee;
        private bool _isEmployeeSelected;
        private bool _isRefreshing;

        public ObservableCollection<Employee> Employees { get; set; }

        public Employee? SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                // 빈 행(Id == 0)을 선택하려고 할 때, 첫 번째 빈 행으로 강제 변경
                if (value != null && value.Id == 0)
                {
                    var firstEmptyEmployee = Employees.FirstOrDefault(e => e.Id == 0);
                    if (firstEmptyEmployee != null && value != firstEmptyEmployee)
                    {
                        // 첫 번째 빈 행이 아니면 첫 번째 빈 행으로 강제 변경
                        value = firstEmptyEmployee;
                    }
                }

                if (_selectedEmployee != value)
                {
                    if (value != null && value.Id == 0)
                    {
                        PrepareNewEmployee(value);
                    }

                    _selectedEmployee = value;
                    OnPropertyChanged();
                    IsEmployeeSelected = _selectedEmployee != null;
                }
            }
        }

        public bool IsEmployeeSelected
        {
            get => _isEmployeeSelected;
            set
            {
                _isEmployeeSelected = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddEmployeeCommand { get; }
        public ICommand SaveEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }

        public EmployeeManagementViewModel()
        {
            _context = new AccountingDbContext();
            Employees = new ObservableCollection<Employee>();
            
            AddEmployeeCommand = new RelayCommand(_ => AddMoreRows());
            SaveEmployeeCommand = new RelayCommand(async _ => await SaveChangesAsync(), _ => SelectedEmployee != null);
            DeleteEmployeeCommand = new RelayCommand(async _ => await DeleteEmployee(), _ => SelectedEmployee != null && SelectedEmployee.Id != 0);
        }

        public async Task LoadEmployeesAsync(Company company)
        {
            _company = company;
            _isRefreshing = true;
            try
            {
                var employees = await _context.Employees
                    .Where(e => e.CompanyId == _company.Id && e.IsActive)
                    .OrderBy(e => e.EmployeeCode)
                    .ToListAsync();
                
                Employees.Clear();
                foreach (var emp in employees)
                {
                    Employees.Add(emp);
                }

                while (Employees.Count < DefaultRowCount)
                {
                    Employees.Add(new Employee());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"사원 목록 로딩 중 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private void AddMoreRows()
        {
            for (int i = 0; i < 10; i++)
            {
                Employees.Add(new Employee());
            }
        }

        private async void PrepareNewEmployee(Employee newEmployee)
        {
            try
            {
                var lastEmployee = await _context.Employees
                    .Where(e => e.CompanyId == _company.Id)
                    .OrderByDescending(e => e.EmployeeCode)
                    .FirstOrDefaultAsync();

                int nextCodeNumber = 1;
                if (lastEmployee != null && int.TryParse(lastEmployee.EmployeeCode, out int lastCode))
                {
                    nextCodeNumber = lastCode + 1;
                }

                newEmployee.CompanyId = _company.Id;
                newEmployee.EmployeeCode = nextCodeNumber.ToString("D4");
                newEmployee.IsActive = true;
                newEmployee.EmploymentStartDate = DateTime.Today;

                OnPropertyChanged(nameof(SelectedEmployee));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"새 사원 준비 중 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveChangesAsync()
        {
            if (SelectedEmployee == null || _isRefreshing) return;

            try
            {
                if (SelectedEmployee.Id == 0)
                {
                    _context.Employees.Add(SelectedEmployee);
                }

                await _context.SaveChangesAsync();
                MessageBox.Show("저장되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);

                // 저장 후 목록 새로고침 (선택된 항목 유지)
                int selectedIndex = Employees.IndexOf(SelectedEmployee);
                await LoadEmployeesAsync(_company);
                
                // 저장된 항목을 다시 선택
                if (selectedIndex >= 0 && selectedIndex < Employees.Count)
                {
                    SelectedEmployee = Employees[selectedIndex];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 중 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteEmployee()
        {
            if (SelectedEmployee == null || SelectedEmployee.Id == 0) return;

            var result = MessageBox.Show($"'{SelectedEmployee.Name}' 사원 정보를 삭제하시겠습니까? (비활성 처리됩니다)", "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    SelectedEmployee.IsActive = false;
                    await _context.SaveChangesAsync();
                    
                    Employees.Remove(SelectedEmployee);
                    Employees.Add(new Employee());
                    SelectedEmployee = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"삭제 중 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
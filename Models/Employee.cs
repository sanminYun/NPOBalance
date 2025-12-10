using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NPOBalance.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty; // 사원번호
        public string Name { get; set; } = string.Empty; // 성명
        public bool IsActive { get; set; } = true;

        // 요청된 필드 추가
        public string? ResidentId { get; set; } // 주민등록번호
        public string? Address1 { get; set; } // 주소1
        public string? Address2 { get; set; } // 주소2
        public DateTime? EmploymentStartDate { get; set; } // 입사일 (기존 필드명 유지)
        public DateTime? EmploymentEndDate { get; set; } // 퇴사일 (기존 필드명 유지)
        public int? StartingSalaryStep { get; set; } // 입사호봉
        public string? Department { get; set; } // 부서
        public string? Position { get; set; } // 직위
        public decimal? EstimatedTotalSalary { get; set; } // 예상총급여액
        public int? Dependents { get; set; } // 부양가족수

        // Navigation properties
        public Company Company { get; set; } = null!;
        public ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
        public ICollection<PayrollEntryDraft> PayrollEntryDrafts { get; set; } = new List<PayrollEntryDraft>();

        [NotMapped]
        public string FullAddress
        {
            get => $"{Address1} {Address2}".Trim();
        }

        [NotMapped]
        public int? Age
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ResidentId))
                    return null;

                try
                {
                    string cleanId = ResidentId.Replace("-", "");
                    if (cleanId.Length < 7)
                        return null;

                    string birthPart = cleanId.Substring(0, 6);
                    char genderDigit = cleanId[6];

                    int birthYear = int.Parse(birthPart.Substring(0, 2));
                    int birthMonth = int.Parse(birthPart.Substring(2, 2));
                    int birthDay = int.Parse(birthPart.Substring(4, 2));

                    int century = genderDigit switch
                    {
                        '1' or '2' or '5' or '6' => 1900,
                        '3' or '4' or '7' or '8' => 2000,
                        '9' or '0' => 1800,
                        _ => -1
                    };

                    if (century == -1)
                    {
                        return null;
                    }

                    int fullBirthYear = century + birthYear;

                    if (birthMonth < 1 || birthMonth > 12 || birthDay < 1 || birthDay > 31)
                        return null;

                    DateTime birthDate;
                    try
                    {
                        birthDate = new DateTime(fullBirthYear, birthMonth, birthDay);
                    }
                    catch
                    {
                        return null;
                    }

                    int age = DateTime.Today.Year - birthDate.Year;

                    if (birthDate > DateTime.Today.AddYears(-age))
                    {
                        age--;
                    }

                    return age >= 0 ? age : null;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
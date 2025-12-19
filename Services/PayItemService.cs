using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NPOBalance.Data;
using NPOBalance.Models;

namespace NPOBalance.Services;

public class PayItemService
{
    public const string TaxableEarnings = "TaxableEarnings";
    public const string NonTaxableEarnings = "NonTaxableEarnings";
    public const string InsuranceDeduction = "InsuranceDeduction";
    public const string IncomeTaxDeduction = "IncomeTaxDeduction";
    public const string EmployerInsurance = "EmployerInsurance";
    public const string Retirement = "Retirement";
    public const string FundingSource = "FundingSource";

    public async Task<List<string>> GetPayItemsAsync(string sectionName)
    {
        try
        {
            using var db = new AccountingDbContext();
            var setting = await db.PayItemSettings.FirstOrDefaultAsync(s => s.SectionName == sectionName);

            if (setting == null)
            {
                return GetDefaultItems(sectionName);
            }

            var items = JsonSerializer.Deserialize<List<string>>(setting.ItemsJson);
            return items?.Where(s => !string.IsNullOrWhiteSpace(s)).ToList() ?? new List<string>();
        }
        catch
        {
            // DB 테이블이 없거나 접근 불가 시 기본값 반환
            return GetDefaultItems(sectionName);
        }
    }

    public async Task SavePayItemsAsync(string sectionName, List<string> items)
    {
        using var db = new AccountingDbContext();
        var setting = await db.PayItemSettings.FirstOrDefaultAsync(s => s.SectionName == sectionName);

        var json = JsonSerializer.Serialize(items);

        if (setting == null)
        {
            setting = new PayItemSetting
            {
                SectionName = sectionName,
                ItemsJson = json,
                UpdatedAt = DateTime.UtcNow
            };
            db.PayItemSettings.Add(setting);
        }
        else
        {
            setting.ItemsJson = json;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    public async Task InitializeDefaultsAsync()
    {
        try
        {
            using var db = new AccountingDbContext();
            
            // DB가 준비되지 않았을 수 있으므로 테이블 존재 여부 확인
            var canConnect = await db.Database.CanConnectAsync();
            if (!canConnect)
            {
                return;
            }

            var existingSections = await db.PayItemSettings.Select(s => s.SectionName).ToListAsync();

            var sectionsToInit = new[]
            {
                TaxableEarnings, NonTaxableEarnings, InsuranceDeduction,
                IncomeTaxDeduction, EmployerInsurance, Retirement, FundingSource
            };

            foreach (var section in sectionsToInit)
            {
                if (!existingSections.Contains(section))
                {
                    var defaultItems = GetDefaultItems(section);
                    await SavePayItemsAsync(section, defaultItems);
                }
            }
        }
        catch
        {
            // DB 초기화 실패 시 무시 (마이그레이션 진행 중일 수 있음)
        }
    }

    private List<string> GetDefaultItems(string sectionName)
    {
        return sectionName switch
        {
            TaxableEarnings => new List<string>
            {
                "봉급", "가족수당", "시간외수당", "처우개선비", "특수근무수당", "명절수당", "식대"
            },
            NonTaxableEarnings => new List<string>
            {
                "식대", "자가운전보조금", "출산휴가지원"
            },
            InsuranceDeduction => new List<string>
            {
                "국민연금", "건강보험", "장기요양보험", "고용보험",
                "국민연금정산", "건강보험정산", "장기요양보험정산", "고용보험정산"
            },
            IncomeTaxDeduction => new List<string>
            {
                "소득세", "지방소득세", "중도정산소득세", "중도정산지방소득세",
                "연말정산소득세", "연말정산지방소득세"
            },
            EmployerInsurance => new List<string>
            {
                "국민연금", "건강보험", "장기요양보험", "고용보험",
                "국민연금정산", "건강보험정산", "장기요양보험정산", "고용보험정산", "산재보험"
            },
            Retirement => new List<string>
            {
                "퇴직연금 - DC형", "퇴직연금 - DB형"
            },
            FundingSource => new List<string>
            {
                "보조금", "후원금", "시설부담"
            },
            _ => new List<string>()
        };
    }
}
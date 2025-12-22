using Microsoft.EntityFrameworkCore;
using NPOBalance.Models;

namespace NPOBalance.Data;

public class AccountingDbContext : DbContext
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<PayItemType> PayItemTypes { get; set; }
    public DbSet<PayrollHeader> PayrollHeaders { get; set; }
    public DbSet<PayrollLine> PayrollLines { get; set; }
    public DbSet<InsuranceRateSetting> InsuranceRateSettings { get; set; }
    public DbSet<PayrollEntryDraft> PayrollEntryDrafts { get; set; }
    public DbSet<PayItemSetting> PayItemSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=nonprofit_payroll.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Company
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FiscalYearStart).IsRequired();
            entity.Property(e => e.FiscalYearEnd).IsRequired();
            entity.Property(e => e.BusinessNumber).HasMaxLength(30);
            entity.Property(e => e.CorporateRegistrationNumber).HasMaxLength(30);
            entity.Property(e => e.RepresentativeName).HasMaxLength(100);
            entity.Property(e => e.CompanyType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TaxSource).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.CompanyCode).IsUnique();
        });

        // Employee
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.Employees)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.CompanyId, e.EmployeeCode }).IsUnique();
        });

        // PayItemType
        modelBuilder.Entity<PayItemType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.PayItemTypes)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
        });

        // PayrollHeader
        modelBuilder.Entity<PayrollHeader>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.PayrollHeaders)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.CompanyId, e.Year, e.Month, e.RunNumber }).IsUnique();
        });

        // PayrollLine
        modelBuilder.Entity<PayrollLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            entity.HasOne(e => e.PayrollHeader)
                .WithMany(h => h.PayrollLines)
                .HasForeignKey(e => e.PayrollHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Employee)
                .WithMany(emp => emp.PayrollLines)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PayItemType)
                .WithMany(p => p.PayrollLines)
                .HasForeignKey(e => e.PayItemTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PayrollEntryDraft
        modelBuilder.Entity<PayrollEntryDraft>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.PayItemValuesJson).IsRequired();
            entity.Property(e => e.AccrualYear).IsRequired();
            entity.Property(e => e.AccrualMonth).IsRequired();

            entity.HasOne(e => e.Company)
                .WithMany(c => c.PayrollEntryDrafts)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Employee)
                .WithMany(emp => emp.PayrollEntryDrafts)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.CompanyId, e.EmployeeId, e.AccrualYear, e.AccrualMonth }).IsUnique();
        });

        // InsuranceRateSetting - 모든 필드 포함
        modelBuilder.Entity<InsuranceRateSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // 요율 필드
            entity.Property(e => e.NationalPensionRateEmployee).HasPrecision(7, 6);
            entity.Property(e => e.NationalPensionRateEmployer).HasPrecision(7, 6);
            entity.Property(e => e.HealthInsuranceRateEmployee).HasPrecision(7, 6);
            entity.Property(e => e.HealthInsuranceRateEmployer).HasPrecision(7, 6);
            entity.Property(e => e.LongTermCareRateEmployee).HasPrecision(7, 6);
            entity.Property(e => e.LongTermCareRateEmployer).HasPrecision(7, 6);
            entity.Property(e => e.EmploymentInsuranceRateEmployee).HasPrecision(7, 6);
            entity.Property(e => e.EmploymentInsuranceRateEmployer).HasPrecision(7, 6);
            entity.Property(e => e.IndustrialAccidentRateEmployee).HasPrecision(7, 6);
            entity.Property(e => e.IndustrialAccidentRateEmployer).HasPrecision(7, 6);
            
            // 최저한도 - 기준금액
            entity.Property(e => e.NationalPensionMinBaseAmount).HasPrecision(18, 2);
            entity.Property(e => e.HealthInsuranceMinBaseAmount).HasPrecision(18, 2);
            entity.Property(e => e.LongTermCareMinBaseAmount).HasPrecision(18, 2);
            entity.Property(e => e.EmploymentInsuranceMinBaseAmount).HasPrecision(18, 2);
            entity.Property(e => e.IndustrialAccidentMinBaseAmount).HasPrecision(18, 2);
            
            // 최저한도 - 보험료
            entity.Property(e => e.NationalPensionMinPremium).HasPrecision(18, 2);
            entity.Property(e => e.HealthInsuranceMinPremium).HasPrecision(18, 2);
            entity.Property(e => e.LongTermCareMinPremium).HasPrecision(18, 2);
            entity.Property(e => e.EmploymentInsuranceMinPremium).HasPrecision(18, 2);
            entity.Property(e => e.IndustrialAccidentMinPremium).HasPrecision(18, 2);
            
            entity.Property(e => e.RoundingRule).IsRequired().HasMaxLength(20);
            entity.Property(e => e.MonthlyIncomeMin).HasPrecision(18, 2);
            entity.Property(e => e.MonthlyIncomeMax).HasPrecision(18, 2);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.InsuranceRateSettings)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PayItemSetting
        modelBuilder.Entity<PayItemSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SectionName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ItemsJson).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasIndex(e => e.SectionName).IsUnique();
        });
    }
}
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
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BusinessNumber).HasMaxLength(30);
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

        // InsuranceRateSetting
        modelBuilder.Entity<InsuranceRateSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NationalPensionRateEmployee).HasPrecision(5, 4);
            entity.Property(e => e.NationalPensionRateEmployer).HasPrecision(5, 4);
            entity.Property(e => e.HealthInsuranceRateEmployee).HasPrecision(5, 4);
            entity.Property(e => e.HealthInsuranceRateEmployer).HasPrecision(5, 4);
            entity.Property(e => e.EmploymentInsuranceRateEmployee).HasPrecision(5, 4);
            entity.Property(e => e.EmploymentInsuranceRateEmployer).HasPrecision(5, 4);
            entity.Property(e => e.RoundingRule).IsRequired().HasMaxLength(20);
            entity.Property(e => e.MonthlyIncomeMin).HasPrecision(18, 2);
            entity.Property(e => e.MonthlyIncomeMax).HasPrecision(18, 2);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.InsuranceRateSettings)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
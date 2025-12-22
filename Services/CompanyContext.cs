using System;
using NPOBalance.Models;

namespace NPOBalance.Services;

public static class CompanyContext
{
    private static Company? _currentCompany;

    public static event EventHandler<Company?>? CompanyChanged;

    public static Company? CurrentCompany => _currentCompany;

    public static void SetCompany(Company? company)
    {
        if (ReferenceEquals(_currentCompany, company))
        {
            return;
        }

        _currentCompany = company;
        CompanyChanged?.Invoke(null, company);
    }
}
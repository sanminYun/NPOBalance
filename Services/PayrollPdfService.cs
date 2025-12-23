using System;
using System.Collections.Generic;
using System.Linq;
using NPOBalance.Models;
using NPOBalance.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NPOBalance.Services;

public class PayrollPdfService
{
    private const int MinimumRows = 15;
    
    // 전역 변수로 테두리 두께 정의
    private const float ThickBorder = 1f;   // 두꺼운 테두리
    private const float ThinBorder = 0.5f;  // 얇은 테두리

    public byte[] GeneratePayrollSlip(
        Company company,
        Employee employee,
        PayrollEntryDetailViewModel detail,
        DateTime accrualMonth,
        DateTime paymentDate,
        string fundingSource,
        List<string> taxableEarningsItems,
        List<string> nonTaxableEarningsItems,
        List<string> retirementItems)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Malgun Gothic"));

                page.Content().Column(column =>
                {
                    column.Spacing(8);

                    // 제목
                    column.Item().AlignCenter().Text($"{accrualMonth:yyyy년 MM월}분 급여명세서")
                        .FontSize(18).Bold();

                    // 회사 및 사원 정보
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text($"회 사 명: {company.Name}");
                            col.Item().Text($"성    명: {employee.Name}");
                            col.Item().Text($"부    서: {employee.Department ?? "-"}");
                            col.Item().Text($"직    위: {employee.Position ?? "-"}");
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignRight().Text($"귀속일자: {accrualMonth:yyyy.MM.01}");
                            col.Item().AlignRight().Text($"지급일자: {paymentDate:yyyy.MM.dd}");
                        });
                    });

                    // 헤더와 본문 사이 여백
                    column.Item().PaddingVertical(15);

                    // 지급내역 | 공제내역 테이블 (외곽 테두리로 감싸기)
                    column.Item().Border(ThickBorder).BorderColor(Colors.Black).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1); // 지급 항목
                            columns.RelativeColumn(1); // 지급 금액
                            columns.RelativeColumn(1); // 공제 항목
                            columns.RelativeColumn(1); // 공제 금액
                        });

                        // 헤더 (세부 내역 포함)
                        table.Header(header =>
                        {
                            // 세부 내역 행 (두꺼운 테두리)
                            header.Cell().ColumnSpan(4)
                                .Border(ThickBorder).BorderColor(Colors.Black)
                                .Background(Colors.Grey.Lighten2)
                                .Padding(6).AlignCenter().Text("세부 내역").FontSize(12).Bold();

                            // 지급 내역 (왼쪽 2열)
                            header.Cell().ColumnSpan(2)
                                .Border(ThickBorder).BorderColor(Colors.Black)
                                .Background(Colors.Grey.Lighten3)
                                .Padding(5).AlignCenter().Text("지급 내역").Bold();
                            
                            // 공제 내역 (오른쪽 2열)
                            header.Cell().ColumnSpan(2)
                                .Border(ThickBorder).BorderColor(Colors.Black)
                                .Background(Colors.Grey.Lighten3)
                                .Padding(5).AlignCenter().Text("공제 내역").Bold();

                            // 항목 (지급 - 왼쪽 첫번째)
                            header.Cell()
                                .Border(ThickBorder).BorderColor(Colors.Black)
                                .Background(Colors.Grey.Lighten3)
                                .Padding(5).AlignCenter().Text("항목").Bold();
                            
                            // 지급금액 (지급 - 왼쪽 두번째)
                            header.Cell()
                                .Border(ThickBorder).BorderColor(Colors.Black)
                                .Background(Colors.Grey.Lighten3)
                                .Padding(5).AlignCenter().Text("지급금액").Bold();
                            
                            // 항목 (공제 - 오른쪽 첫번째)
                            header.Cell()
                                .Border(ThickBorder).BorderColor(Colors.Black)
                                .Background(Colors.Grey.Lighten3)
                                .Padding(5).AlignCenter().Text("항목").Bold();
                            
                            // 공제금액 (공제 - 오른쪽 두번째)
                            header.Cell()
                                .Border(ThickBorder).BorderColor(Colors.Black)
                                .Background(Colors.Grey.Lighten3)
                                .Padding(5).AlignCenter().Text("공제금액").Bold();
                        });

                        // 지급내역 데이터 준비 (모든 항목 포함)
                        var earnings = new List<(string Name, decimal Amount)>();
                        
                        // 과세급상여 (모든 항목 포함)
                        for (int i = 0; i < taxableEarningsItems.Count; i++)
                        {
                            var value = detail.GetValue(PayItemService.TaxableEarnings, i);
                            earnings.Add((taxableEarningsItems[i], value));
                        }

                        // 비과세급상여 (모든 항목 포함)
                        for (int i = 0; i < nonTaxableEarningsItems.Count; i++)
                        {
                            var value = detail.GetValue(PayItemService.NonTaxableEarnings, i);
                            earnings.Add((nonTaxableEarningsItems[i], value));
                        }

                        // 공제내역 데이터 준비 (모든 항목 포함)
                        var deductions = new List<(string Name, decimal Amount)>
                        {
                            ("국민연금", detail.EmployeeNationalPension),
                            ("건강보험", detail.EmployeeHealthInsurance),
                            ("장기요양보험", detail.EmployeeLongTermCare),
                            ("고용보험", detail.EmployeeEmploymentInsurance),
                            ("소득세", detail.FinalIncomeTax),
                            ("지방소득세", detail.LocalIncomeTax)
                        };

                        // 행 수는 지급/공제 중 더 긴 쪽에 맞춤 (최소 15행)
                        int maxRows = Math.Max(Math.Max(earnings.Count, deductions.Count), MinimumRows);

                        for (int i = 0; i < maxRows; i++)
                        {
                            var isLastRow = (i == maxRows - 1);
                            
                            // 지급내역 - 항목 (왼쪽 첫번째)
                            if (i < earnings.Count)
                            {
                                table.Cell()
                                    .BorderBottom(isLastRow ? ThickBorder : ThinBorder)
                                    .BorderRight(ThinBorder)
                                    .BorderColor(Colors.Black)
                                    .Padding(5).Text(earnings[i].Name);
                            }
                            else
                            {
                                table.Cell()
                                    .BorderBottom(isLastRow ? ThickBorder : ThinBorder)
                                    .BorderRight(ThinBorder)
                                    .BorderColor(Colors.Black)
                                    .Padding(5).Text("");
                            }

                            // 지급내역 - 금액 (왼쪽 두번째)
                            if (i < earnings.Count)
                            {
                                table.Cell()
                                    .BorderBottom(isLastRow ? ThickBorder : ThinBorder)
                                    .BorderRight(ThinBorder)
                                    .BorderColor(Colors.Black)
                                    .Padding(5).AlignRight()
                                    .Text(earnings[i].Amount > 0 ? $"{earnings[i].Amount:N0}" : "");
                            }
                            else
                            {
                                table.Cell()
                                    .BorderBottom(isLastRow ? ThickBorder : ThinBorder)
                                    .BorderRight(ThinBorder)
                                    .BorderColor(Colors.Black)
                                    .Padding(5).Text("");
                            }

                            // 공제내역 - 항목 (오른쪽 첫번째)
                            if (i < deductions.Count)
                            {
                                table.Cell()
                                    .BorderBottom(isLastRow ? ThickBorder : ThinBorder)
                                    .BorderRight(ThinBorder)
                                    .BorderColor(Colors.Black)
                                    .Padding(5).Text(deductions[i].Name);
                            }
                            else
                            {
                                table.Cell()
                                    .BorderBottom(isLastRow ? ThickBorder : ThinBorder)
                                    .BorderRight(ThinBorder)
                                    .BorderColor(Colors.Black)
                                    .Padding(5).Text("");
                            }

                            // 공제내역 - 금액 (오른쪽 두번째)
                            if (i < deductions.Count)
                            {
                                table.Cell()
                                    .BorderBottom(isLastRow ? ThickBorder : ThinBorder)
                                    .BorderColor(Colors.Black)
                                    .Padding(5).AlignRight()
                                    .Text(deductions[i].Amount > 0 ? $"{deductions[i].Amount:N0}" : "");
                            }
                            else
                            {
                                table.Cell()
                                    .BorderBottom(isLastRow ? ThickBorder : ThinBorder)
                                    .BorderColor(Colors.Black)
                                    .Padding(5).Text("");
                            }
                        }

                        // 합계 행
                        var totalEarnings = detail.TaxableEarningsSubtotal + detail.NonTaxableEarningsSubtotal;
                        var totalDeductions = detail.InsuranceDeductionSubtotal + detail.IncomeTaxSubtotal;

                        table.Cell()
                            .Border(ThickBorder).BorderColor(Colors.Black)
                            .Background(Colors.Grey.Lighten4)
                            .Padding(5).AlignCenter().Text("지급액계").Bold();
                        table.Cell()
                            .Border(ThickBorder).BorderColor(Colors.Black)
                            .Background(Colors.Grey.Lighten4)
                            .Padding(5).AlignCenter().Text($"{totalEarnings:N0}").Bold();
                        table.Cell()
                            .Border(ThickBorder).BorderColor(Colors.Black)
                            .Background(Colors.Grey.Lighten4)
                            .Padding(5).AlignCenter().Text("공제액계").Bold();
                        table.Cell()
                            .Border(ThickBorder).BorderColor(Colors.Black)
                            .Background(Colors.Grey.Lighten4)
                            .Padding(5).AlignCenter().Text($"{totalDeductions:N0}").Bold();
                    });

                    // 실수령액 (두꺼운 외곽 테두리)
                    column.Item().PaddingTop(10).Border(ThickBorder).BorderColor(Colors.Black).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Cell()
                            .Border(ThickBorder).BorderColor(Colors.Black)
                            .Background(Colors.Grey.Lighten4)
                            .Padding(8).AlignCenter().Text("실수령액").FontSize(11).Bold();
                        table.Cell()
                            .BorderColor(Colors.Black)
                            .Background(Colors.Grey.Lighten4)
                            .Padding(8).AlignCenter().Text($"{detail.NetPay:N0} 원").FontSize(11).Bold();
                    });
                });
            });
        }).GeneratePdf();
    }
}
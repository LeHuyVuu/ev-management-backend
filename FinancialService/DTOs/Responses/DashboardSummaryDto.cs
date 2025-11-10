namespace FinancialService.DTOs.Responses;

public record DashboardSummaryDto(
    decimal TotalMonthlyRevenueVnd,
    decimal RevenueMoMChangePct,
    int CarsSoldThisMonth,
    decimal CarsSoldMoMChangePct,
    decimal TargetAchievedPct,
    string TargetBadge,
    decimal CustomerDebtVnd,
    decimal CustomerDebtMoMChangePct
);

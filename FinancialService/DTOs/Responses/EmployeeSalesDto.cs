namespace FinancialService.DTOs.Responses;

public record EmployeeSalesDto(
    string EmployeeName,
    decimal TotalSalesVnd
);

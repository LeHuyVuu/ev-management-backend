namespace FinancialService.Model;

public class PaginationParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "start_date";
    public string? SortOrder { get; set; } = "asc";
    public string? Status { get; set; }
    public string? Keyword { get; set; }
}
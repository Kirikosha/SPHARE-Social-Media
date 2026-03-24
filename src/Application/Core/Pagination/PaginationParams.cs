using Microsoft.AspNetCore.Mvc;

namespace Application.Core.Pagination;

public class PaginationParams
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    
    [FromQuery(Name = "page")]
    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
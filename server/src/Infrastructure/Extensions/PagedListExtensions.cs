using Application.Core.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class PagedListExtensions
{
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> source, 
        int pageNumber, 
        int pageSize, 
        CancellationToken ct = default)
    {
        var totalCount = await source.CountAsync(ct);
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedList<T>.Create(items, totalCount, pageNumber, pageSize);
    }
}
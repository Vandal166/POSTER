using Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Common;

/// <summary>
/// Needs to be ordered beforehand.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PagedList<T> : IPagedList<T>
    where T : class
{
    public List<T> Items { get; }
    
    public int Page { get; }
    
    public int PageSize { get; }
    
    public int TotalCount { get; }
    
    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;
    
    
    private PagedList(List<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
    
    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int page, int pageSize, CancellationToken ct = default)
    {
        var validPage     = Math.Max(1, page);
        var validPageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await source.CountAsync(ct);
        var items = await source
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .ToListAsync(ct);

        return new PagedList<T>(items, validPage, validPageSize, totalCount);
    }
}
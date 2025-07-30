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
    
    public bool HasNextPage => Page < (int)Math.Ceiling((double)TotalCount / PageSize);    
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
        var validPageSize = Math.Clamp(pageSize, 1, 100);
        var totalCount = await source.CountAsync(ct);
        if(totalCount == 0)
            return new PagedList<T>(new List<T>(), 1, validPageSize, 0);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / validPageSize);
        var validPage = Math.Clamp(page, 1, totalPages);

        var items = await source
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .ToListAsync(ct);

        return new PagedList<T>(items, validPage, validPageSize, totalCount); // totalCount = item count
    }
}
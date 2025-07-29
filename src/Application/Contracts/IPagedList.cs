namespace Application.Contracts;

public interface IPagedList<T>
{
    List<T> Items { get; }
    int TotalCount { get; }
    int Page { get; }
    int PageSize { get; }
    bool HasNextPage { get; }
    bool HasPreviousPage { get; }
}
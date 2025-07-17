namespace Application.DTOs;

public record CreatePostDto(string Content);

public record GetPostQueryDto(int Page = 1, int PageSize = 10);


public record PaginatedResponse<T>(List<T> Items, int Page, int PageSize, int TotalCount)
{
    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;
}
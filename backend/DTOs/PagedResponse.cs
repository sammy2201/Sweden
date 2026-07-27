namespace SwedenStart;

public class PagedResponse<T>
{
     public List<T> Items { get; set; } = [];

     public int Page { get; set; } = 1;

     public int PageSize { get; set; } = 20;

     public int TotalCount { get; set; }

     public int TotalPages => PageSize <= 0
          ? 0
          : (int)Math.Ceiling((double)TotalCount / PageSize);

     public bool HasNextPage => Page < TotalPages;

     public bool HasPreviousPage => Page > 1;
}
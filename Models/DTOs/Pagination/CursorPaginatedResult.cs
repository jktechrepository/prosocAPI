namespace Prosoc.Models.DTOs
{
    public class CursorPaginatedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public string? NextCursor { get; set; }
        public string? PreviousCursor { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public int PageSize { get; set; }
    }
}

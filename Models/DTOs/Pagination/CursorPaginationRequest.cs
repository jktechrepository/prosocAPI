namespace Prosoc.Models.DTOs
{
    public class CursorPaginationRequest
    {
        public string? Cursor { get; set; }
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";
    }
}

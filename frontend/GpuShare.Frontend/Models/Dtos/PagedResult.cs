namespace GpuShare.Frontend.Models.Dtos
{

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = [];

        public int TotalCount { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }

    public class PaginationQuery
    {
        public int Page { get; set; } = 1;
        public int Count { get; set; } = 10;
    }

    public class LimitQuery
    {
        public int Limit { get; set; } = 10;
    }
}
using Microsoft.EntityFrameworkCore;

namespace Core.Common
{
    public sealed class PaginatedList<T> : List<T>
    {
        public int PageNumber { get; }
        public int TotalPages { get; }
        public int TotalCount { get; }

        public PaginatedList()
        {
            PageNumber = 1;
            TotalPages = 1;
            TotalCount = 0;
        }

        public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            AddRange(items);
        }

        public PaginatedList(int pageNumber, int totalPages, int totalCount, List<T> items)
        {
            PageNumber = pageNumber;
            TotalPages = totalPages;
            TotalCount = totalCount;
            AddRange(items);
        }

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }
    }
}

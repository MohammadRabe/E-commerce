using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace E_commerce.Data.Wrappers
{
    public class PagedList<T>
    {
        public PagedList(int pageNumber, int pageSize, List<T> items, int totalCount)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            Items = items;
            TotalCount = totalCount;
        }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public bool Next => PageSize * PageNumber < TotalCount;
        public bool Prev => PageNumber > 1;
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }

        
        public static async Task<PagedList<T>> GetPagedListAsync(
            int pageNumber,
            int pageSize,
            IQueryable<T> query,
            CancellationToken cancellationToken = default)
        {
            var count = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            return new(pageNumber, pageSize, items, count);
        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace E_commerce.Data.Wrappers
{
    public static class PagedListBuilder
    {
        public static Task<PagedList<T>> ToPagedList<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return PagedList<T>.GetPagedListAsync(query: query, pageNumber: pageNumber,
                pageSize: pageSize, cancellationToken: cancellationToken);
        }
    }
}

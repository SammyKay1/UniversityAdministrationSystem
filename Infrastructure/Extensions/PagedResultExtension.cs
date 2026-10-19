
using Microsoft.EntityFrameworkCore;

public static class PagedRequestExtension
{

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, PagedRequest request)
    {
        var total =
            await query.CountAsync();

        var items =
            await query
                .ApplyPaging(request)
                .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,

            TotalCount = total,

            PageNumber = request.PageNumber,

            PageSize = request.PageSize
        };
    }
}
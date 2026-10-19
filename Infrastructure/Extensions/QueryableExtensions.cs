
public static class QueryableExtensions
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, PagedRequest request)
    {
        return query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);
    }
}
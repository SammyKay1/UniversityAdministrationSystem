
public class PagedResultGeneric<T>
{
    public List<T> Items { get; set; } = new();

    public int TotalRecords { get; set; }

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

}
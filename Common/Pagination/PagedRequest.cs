
public class PagedRequest
{
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;

    private int _pageSize = 5;

    public int PageSize
    {
        get => _pageSize;

        //set => _pageSize = value > MaxPageSize  ? MaxPageSize : value;
        set => _pageSize = Math.Min(value, MaxPageSize);

    }

    public string? Search { get; set; }

    public string? SortColumn { get; set; }

    public bool SortDescending { get; set; }

    //Generic filters
    public Dictionary<string, string> Filters { get; set; } = new();
}
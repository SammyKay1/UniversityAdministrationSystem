
public class UserListRequest
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string SearchTerm { get; set; } = string.Empty;

    public string SortColumn { get; set; } = "UserName";

    public bool SortDescending { get; set; }
}
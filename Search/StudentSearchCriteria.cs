
public class StudentSearchCriteria
{
    public string?   Name { get; set; }

    public int? DepartmentId { get; set; }

    public StudentStatus? Status { get; set; }

    public int? Age { get; set; }

    public string? SortBy { get; set; }

    public bool Descending { get; set; } 

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;




}
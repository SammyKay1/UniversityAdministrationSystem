
public class CourseEditDto
{
    public int Id { get; set; }

    public string CourseCode { get; set; } = "";

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public int CreditHours { get; set; }

    public int Capacity { get; set; }

    public int DepartmentId { get; set; }
    
    public int? TeacherId { get; set; }
}
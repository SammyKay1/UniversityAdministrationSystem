public class CourseDetailsDto
{
    public int Id { get; set; }

    public string CourseCode { get; set; } = "";

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public int CreditHours { get; set; }

    public int Capacity { get; set; }

    public string Department { get; set; } = "";

    public int? TeacherId { get; set; }

    public string? TeacherName { get; set; }

    public bool IsArchived { get; set; }
}
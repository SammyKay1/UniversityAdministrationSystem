
public class StudentEnrollmentHeaderDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = "";

    public string Department { get; set; } = "";

    public StudentStatus Status { get; set; }
}

public class StudentListDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";
    public string Department { get; set; } = "";

    public StudentStatus Status { get; set; }
}
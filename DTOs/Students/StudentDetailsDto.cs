
public class StudentDetailsDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public string Department { get; set; } = "";
    public string Address { get; set; } = "";
    public List<string> Courses { get; set; } = new();
    
}

public class StudentCreateDto
{
    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string Email { get; set; } = "";

    public DateTime DateOfBirth { get; set; }

    public int DepartmentId { get; set; }

    public AddressDto Address {get; set; } = new();
}
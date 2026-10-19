
using System.Security.Cryptography.X509Certificates;

public class Coursera
{

public Coursera(string name, string courseCode, int creditHours )
{
    Name = name;
    CourseCode = courseCode;

    ValidateCreditHours(creditHours);
    CreditHours = creditHours;
}
    
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string CourseCode { get; set; } = "";

    public int? TeacherId { get; set; }

    public Teacher? Teacher { get; set; }

    public int CreditHours { get; set; }


    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    private static void ValidateCreditHours(int creditHours)
{
    if (creditHours < 1 || creditHours > 6)
    {
        throw new ArgumentOutOfRangeException(
            nameof(creditHours),
            "Credit hours must be between 1 and 6.");
    }
}
}
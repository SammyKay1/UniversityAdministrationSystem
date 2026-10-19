
public class Course : BaseEntity
{
    private Course()
    {
    }

    public Course(string courseCode, string name, string description, int creditHours, int capacity, int departmentId)
    {
        Update(courseCode, name, description, creditHours, capacity, departmentId);
    }

   
    public string CourseCode { get; private set; } = "";

    public string Name { get; private set; } = "";

    public string Description { get; private set; } = "";

    public int CreditHours { get; private set; }

    public int Capacity { get; private set; }

    public int? TeacherId { get; set; }

    public Teacher? Teacher { get; set; }

    public bool IsArchived { get; private set; }

    public int DepartmentId { get; private set; }

    public Department Department { get; private set; } = null!;

    public ICollection<Enrollment> Enrollments { get; private set; }
        = new List<Enrollment>();


    public void Update(string courseCode, string name, string description,  int creditHours, int capacity,
        int departmentId)
    {
        Validate(courseCode, name, creditHours, capacity);

        CourseCode = courseCode.Trim();

        Name = name.Trim();

        Description = description.Trim();

        CreditHours = creditHours;

        Capacity = capacity;

        DepartmentId = departmentId;
    }


    public void Archive()
    {
        IsArchived = true;
    }


    public void Restore()
    {
        IsArchived = false;
    }


    public void ChangeCapacity(int capacity)
    {
        if (capacity < 1)
            throw new ArgumentException(
                "Capacity must be greater than zero.");

        if (Enrollments.Count(e => e.Status == EnrollmentStatus.Active) > capacity)
            throw new InvalidOperationException(
                "Capacity cannot be less than the number of active enrollments.");

        Capacity = capacity;
    }


    public void ChangeCreditHours(int hours)
    {
        if (hours < 1 || hours > 6)
            throw new ArgumentException(
                "Credit hours must be between 1 and 6.");

        CreditHours = hours;
    }


    private static void Validate(string courseCode, string name, int creditHours, int capacity)
    {
        if (string.IsNullOrWhiteSpace(courseCode))
            throw new ArgumentException("Course code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Course name is required.");

        if (creditHours < 1 || creditHours > 6)
            throw new ArgumentException(
                "Credit hours must be between 1 and 6.");

        if (capacity < 1)
            throw new ArgumentException(
                "Capacity must be greater than zero.");
    }
}
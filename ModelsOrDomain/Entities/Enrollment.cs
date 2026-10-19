
public class Enrollment
{


    //Parameterless constructor: Required by EF Core.
    public Enrollment()
    {
        
    }
    public Enrollment(int studentId, int courseId)
    {
        StudentId = studentId;
        CourseId = courseId;
        EnrollmentDate = DateTime.UtcNow;
        Grade = Grade.None;
        Status = EnrollmentStatus.Active;
    }


    public int Id { get; private set; }

    public int StudentId { get; private set; }

    public Student Student { get; private set; } = null!;

    public int CourseId { get; private set; }

    public Course Course { get; private set; } = null!;

    public DateTime EnrollmentDate { get; private set; }

    public Grade Grade { get; private set; } = Grade.None;

    public EnrollmentStatus Status { get; private set; }




    public void AssignGrade(Grade grade)
    {
        if(grade == Grade.None)
        {
            throw new InvalidOperationException(
                "A valid grade must be assigned.");
        }

        Grade = grade;
    }

    public void Drop()
    {
        if(Status != EnrollmentStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active enrollments can be dropped.");
        }

        Status = EnrollmentStatus.Dropped;
    }

    public void Complete()
    {
        if(Status != EnrollmentStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active enrollments can be completed.");
        }

        Status = EnrollmentStatus.Completed;
    }

    public bool CanBeDropped()
    {
        return Status == EnrollmentStatus.Active;
    }

}
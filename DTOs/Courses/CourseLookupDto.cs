public class CourseLookupDto
{
    public int Id { get; set; }

    public string CourseCode { get; set; } = "";

    public string CourseName { get; set; } = "";

    public int Capacity { get; set; }

    public int ActiveEnrollmentCount { get; set; }

    public int AvailableSeats =>   Math.Max(0, Capacity - ActiveEnrollmentCount);
}
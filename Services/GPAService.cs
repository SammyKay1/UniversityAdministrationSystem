
public class GPAService
{
    public decimal CalculateGPA(IEnumerable<TranscriptCourseDto> courses)
    {
        var completedCourses = courses
                                .Where(c =>
                                    c.Status == EnrollmentStatus.Completed &&
                                    c.Grade != Grade.None);


        var totalCredits =
            completedCourses
            .Sum(c => c.CreditHours);


        if(totalCredits == 0)
            return 0;


        var totalPoints =
            completedCourses
            .Sum(c =>
                c.CreditHours *
                c.Grade.GetGradePoint());


        return Math.Round(
            totalPoints / totalCredits,
            2);
    }
}
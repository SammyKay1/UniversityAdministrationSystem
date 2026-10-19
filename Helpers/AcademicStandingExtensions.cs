
public static class AcademicStandingExtensions
{
    public static string DisplayName(this AcademicStanding standing)
    {
        return standing switch
        {
            AcademicStanding.DeanList =>
                "Dean's List",

            AcademicStanding.GoodStanding =>
                "Good Standing",

            AcademicStanding.Probation =>
                "Academic Probation",

            AcademicStanding.Suspension =>
                "Suspended",

            _ => "Unknown"
        };
    }
}
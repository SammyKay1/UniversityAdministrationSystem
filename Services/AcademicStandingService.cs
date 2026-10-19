
public class AcademicStandingService
{

    public AcademicStanding Determine(decimal gpa)
    {
        if (gpa >= 3.5m)
        {
            return AcademicStanding.DeanList;
        }


        if (gpa >= 2.0m)
        {
            return AcademicStanding.GoodStanding;
        }


        if (gpa >= 1.0m)
        {
            return AcademicStanding.Probation;
        }


        return AcademicStanding.Suspension;
    }
}
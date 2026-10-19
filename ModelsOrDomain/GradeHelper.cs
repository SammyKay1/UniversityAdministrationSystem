
public static class GradeHelper
{
    public static IEnumerable<Grade> GetGrades()
    {
        return Enum.GetValues<Grade>()
            .Where(g => g != Grade.None);
    }
}
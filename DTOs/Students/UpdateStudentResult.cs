
public class UpdateStudentResult
{
    public bool Success { get; set; }

    public bool HasConcurrencyConflict { get; set; }

    public string Message { get; set; } = "";

    public StudentEditDto? CurrentStudent { get; set; }
}
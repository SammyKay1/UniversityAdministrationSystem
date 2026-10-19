
public class StudentTranscriptDto
{
   public StudentTranscriptHeaderDto Header { get; set; }= new();

    public List<TranscriptCourseDto> Courses { get; set; } = [];
}

public class TranscriptService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly GPAService _gpaService;

    private readonly AcademicStandingService _standingService;


    public TranscriptService(IUnitOfWork unitOfWork, GPAService gpaService, AcademicStandingService academicStanding)
    {
        _unitOfWork = unitOfWork;

        _gpaService = gpaService;

        _standingService = academicStanding;
    }



    public async Task<StudentTranscriptDto?> GetTranscriptAsync(int studentId)
    {

        var transcript =
            await _unitOfWork
            .Students
            .GetTranscriptAsync(studentId);

        if(transcript is null)
            return null;
        
        transcript.Header.TotalCreditsEarned = transcript.Courses
                                                .Where(c =>
                                                    c.Status == EnrollmentStatus.Completed)
                                                .Sum(c => c.CreditHours);



    transcript.Header.CurrentCredits =transcript.Courses
                                        .Where(c =>
                                            c.Status == EnrollmentStatus.Active)
                                        .Sum(c => c.CreditHours);


        
        transcript.Header.GPA = _gpaService.CalculateGPA(transcript.Courses);

        transcript.Header.AcademicStanding =  _standingService.Determine(transcript.Header.GPA);


        return transcript;
    }
}
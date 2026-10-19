using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Services;

public class TranscriptServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly GPAService _gpaService;
    private readonly AcademicStandingService _standingService;
    private readonly TranscriptService _service;

    public TranscriptServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _gpaService = new GPAService();
        _standingService = new AcademicStandingService();
        _service = new TranscriptService(_unitOfWorkMock.Object, _gpaService, _standingService);
    }

    [Fact]
    public async Task GetTranscriptAsync_StudentNotFound_ReturnsNull()
    {
        _unitOfWorkMock.Setup(x => x.Students.GetTranscriptAsync(999))
            .ReturnsAsync((StudentTranscriptDto?)null);

        var result = await _service.GetTranscriptAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTranscriptAsync_StudentWithNoCourses_ReturnsTranscriptWithEmptyCourses()
    {
        var transcript = new StudentTranscriptDto
        {
            Header = new StudentTranscriptHeaderDto
            {
                StudentName = "John Doe",
                Department = "Computer Science"
            },
            Courses = new List<TranscriptCourseDto>()
        };

        _unitOfWorkMock.Setup(x => x.Students.GetTranscriptAsync(1))
            .ReturnsAsync(transcript);

        var result = await _service.GetTranscriptAsync(1);

        result.Should().NotBeNull();
        result.Header.StudentName.Should().Be("John Doe");
        result.Courses.Should().BeEmpty();
        result.Header.TotalCreditsEarned.Should().Be(0);
        result.Header.CurrentCredits.Should().Be(0);
        result.Header.GPA.Should().Be(0);
        result.Header.AcademicStanding.Should().Be(AcademicStanding.Suspension); // GPA 0 = Suspension
    }

    [Fact]
    public async Task GetTranscriptAsync_CalculatesTotalCreditsEarned()
    {
        var transcript = new StudentTranscriptDto
        {
            Header = new StudentTranscriptHeaderDto
            {
                StudentName = "John Doe",
                Department = "Computer Science"
            },
            Courses = new List<TranscriptCourseDto>
            {
                new TranscriptCourseDto
                {
                    CourseCode = "CS101",
                    CourseName = "Intro to CS",
                    CreditHours = 3,
                    Grade = Grade.A,
                    Status = EnrollmentStatus.Completed
                },
                new TranscriptCourseDto
                {
                    CourseCode = "MATH101",
                    CourseName = "Calculus",
                    CreditHours = 4,
                    Grade = Grade.B,
                    Status = EnrollmentStatus.Completed
                },
                new TranscriptCourseDto
                {
                    CourseCode = "ENG101",
                    CourseName = "English",
                    CreditHours = 3,
                    Grade = Grade.C,
                    Status = EnrollmentStatus.Active
                }
            }
        };

        _unitOfWorkMock.Setup(x => x.Students.GetTranscriptAsync(1))
            .ReturnsAsync(transcript);

        var result = await _service.GetTranscriptAsync(1);

        result.Should().NotBeNull();
        result.Header.TotalCreditsEarned.Should().Be(7); // 3 + 4
        result.Header.CurrentCredits.Should().Be(3); // Only ENG101 is active
    }

    [Fact]
    public async Task GetTranscriptAsync_CalculatesGPA()
    {
        var transcript = new StudentTranscriptDto
        {
            Header = new StudentTranscriptHeaderDto
            {
                StudentName = "John Doe",
                Department = "Computer Science"
            },
            Courses = new List<TranscriptCourseDto>
            {
                new TranscriptCourseDto
                {
                    CreditHours = 3,
                    Grade = Grade.A,
                    Status = EnrollmentStatus.Completed
                },
                new TranscriptCourseDto
                {
                    CreditHours = 4,
                    Grade = Grade.B,
                    Status = EnrollmentStatus.Completed
                }
            }
        };

        _unitOfWorkMock.Setup(x => x.Students.GetTranscriptAsync(1))
            .ReturnsAsync(transcript);

        var result = await _service.GetTranscriptAsync(1);

        result.Should().NotBeNull();
        // (3*4.0 + 4*3.0) / 7 = 24/7 ≈ 3.43
        result.Header.GPA.Should().BeApproximately(3.43m, 0.01m);
    }

    [Fact]
    public async Task GetTranscriptAsync_CalculatesAcademicStanding()
    {
        var transcript = new StudentTranscriptDto
        {
            Header = new StudentTranscriptHeaderDto
            {
                StudentName = "John Doe",
                Department = "Computer Science"
            },
            Courses = new List<TranscriptCourseDto>
            {
                new TranscriptCourseDto
                {
                    CreditHours = 3,
                    Grade = Grade.A,
                    Status = EnrollmentStatus.Completed
                }
            }
        };

        _unitOfWorkMock.Setup(x => x.Students.GetTranscriptAsync(1))
            .ReturnsAsync(transcript);

        var result = await _service.GetTranscriptAsync(1);

        result.Should().NotBeNull();
        result.Header.GPA.Should().Be(4.0m);
        result.Header.AcademicStanding.Should().Be(AcademicStanding.DeanList); // GPA >= 3.5
    }

    [Fact]
    public async Task GetTranscriptAsync_AcademicStandingProbation()
    {
        var transcript = new StudentTranscriptDto
        {
            Header = new StudentTranscriptHeaderDto
            {
                StudentName = "John Doe",
                Department = "Computer Science"
            },
            Courses = new List<TranscriptCourseDto>
            {
                new TranscriptCourseDto
                {
                    CreditHours = 3,
                    Grade = Grade.C,
                    Status = EnrollmentStatus.Completed
                },
                new TranscriptCourseDto
                {
                    CreditHours = 3,
                    Grade = Grade.D,
                    Status = EnrollmentStatus.Completed
                }
            }
        };

        _unitOfWorkMock.Setup(x => x.Students.GetTranscriptAsync(1))
            .ReturnsAsync(transcript);

        var result = await _service.GetTranscriptAsync(1);

        result.Should().NotBeNull();
        // (3*2.0 + 3*1.0) / 6 = 9/6 = 1.5
        result.Header.GPA.Should().Be(1.5m);
        result.Header.AcademicStanding.Should().Be(AcademicStanding.Probation); // GPA >= 1.0
    }

    [Fact]
    public async Task GetTranscriptAsync_AcademicStandingSuspension()
    {
        var transcript = new StudentTranscriptDto
        {
            Header = new StudentTranscriptHeaderDto
            {
                StudentName = "John Doe",
                Department = "Computer Science"
            },
            Courses = new List<TranscriptCourseDto>
            {
                new TranscriptCourseDto
                {
                    CreditHours = 3,
                    Grade = Grade.F,
                    Status = EnrollmentStatus.Completed
                }
            }
        };

        _unitOfWorkMock.Setup(x => x.Students.GetTranscriptAsync(1))
            .ReturnsAsync(transcript);

        var result = await _service.GetTranscriptAsync(1);

        result.Should().NotBeNull();
        result.Header.GPA.Should().Be(0.0m);
        result.Header.AcademicStanding.Should().Be(AcademicStanding.Suspension); // GPA < 1.0
    }
}
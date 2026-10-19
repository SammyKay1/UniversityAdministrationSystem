using System;
using System.Collections.Generic;
using FluentAssertions;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Services;

public class GPAServiceTests
{
    private readonly GPAService _service = new();

    [Fact]
    public void CalculateGPA_NoCourses_ReturnsZero()
    {
        var courses = new List<TranscriptCourseDto>();

        var result = _service.CalculateGPA(courses);

        result.Should().Be(0);
    }

    [Fact]
    public void CalculateGPA_OnlyActiveCourses_ReturnsZero()
    {
        var courses = new List<TranscriptCourseDto>
        {
            new TranscriptCourseDto
            {
                CourseCode = "CS101",
                CourseName = "Intro to CS",
                CreditHours = 3,
                Grade = Grade.A,
                Status = EnrollmentStatus.Active
            },
            new TranscriptCourseDto
            {
                CourseCode = "MATH101",
                CourseName = "Calculus",
                CreditHours = 4,
                Grade = Grade.B,
                Status = EnrollmentStatus.Active
            }
        };

        var result = _service.CalculateGPA(courses);

        result.Should().Be(0);
    }

    [Fact]
    public void CalculateGPA_OnlyIncompleteGrades_ReturnsZero()
    {
        var courses = new List<TranscriptCourseDto>
        {
            new TranscriptCourseDto
            {
                CourseCode = "CS101",
                CourseName = "Intro to CS",
                CreditHours = 3,
                Grade = Grade.None,
                Status = EnrollmentStatus.Completed
            }
        };

        var result = _service.CalculateGPA(courses);

        result.Should().Be(0);
    }

    [Fact]
    public void CalculateGPA_SingleCompletedCourseWithGradeA_Returns4()
    {
        var courses = new List<TranscriptCourseDto>
        {
            new TranscriptCourseDto
            {
                CourseCode = "CS101",
                CourseName = "Intro to CS",
                CreditHours = 3,
                Grade = Grade.A,
                Status = EnrollmentStatus.Completed
            }
        };

        var result = _service.CalculateGPA(courses);

        result.Should().Be(4.0m);
    }

    [Fact]
    public void CalculateGPA_MultipleCourses_WeightedAverage()
    {
        var courses = new List<TranscriptCourseDto>
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
            }
        };

        var result = _service.CalculateGPA(courses);

        // (3*4.0 + 4*3.0) / (3+4) = (12 + 12) / 7 = 24/7 ≈ 3.43
        result.Should().BeApproximately(3.43m, 0.01m);
    }

    [Fact]
    public void CalculateGPA_AllGrades_CalculatesCorrectly()
    {
        var courses = new List<TranscriptCourseDto>
        {
            new TranscriptCourseDto { CreditHours = 3, Grade = Grade.A, Status = EnrollmentStatus.Completed }, // 12 points
            new TranscriptCourseDto { CreditHours = 3, Grade = Grade.B, Status = EnrollmentStatus.Completed }, // 9 points
            new TranscriptCourseDto { CreditHours = 3, Grade = Grade.C, Status = EnrollmentStatus.Completed }, // 6 points
            new TranscriptCourseDto { CreditHours = 3, Grade = Grade.D, Status = EnrollmentStatus.Completed }, // 3 points
            new TranscriptCourseDto { CreditHours = 3, Grade = Grade.F, Status = EnrollmentStatus.Completed }, // 0 points
        };

        var result = _service.CalculateGPA(courses);

        // Total credits = 15, Total points = 30, GPA = 2.0
        result.Should().Be(2.0m);
    }

    [Fact]
    public void CalculateGPA_MixedActiveAndCompleted_OnlyCountsCompleted()
    {
        var courses = new List<TranscriptCourseDto>
        {
            new TranscriptCourseDto
            {
                CreditHours = 3,
                Grade = Grade.A,
                Status = EnrollmentStatus.Completed
            },
            new TranscriptCourseDto
            {
                CreditHours = 3,
                Grade = Grade.A,
                Status = EnrollmentStatus.Active
            }
        };

        var result = _service.CalculateGPA(courses);

        // Only the completed course should count: 3 credits * 4.0 = 12 / 3 = 4.0
        result.Should().Be(4.0m);
    }

    [Fact]
    public void CalculateGPA_RoundsToTwoDecimalPlaces()
    {
        var courses = new List<TranscriptCourseDto>
        {
            new TranscriptCourseDto
            {
                CreditHours = 1,
                Grade = Grade.A,
                Status = EnrollmentStatus.Completed
            },
            new TranscriptCourseDto
            {
                CreditHours = 2,
                Grade = Grade.B,
                Status = EnrollmentStatus.Completed
            }
        };

        var result = _service.CalculateGPA(courses);

        // (1*4.0 + 2*3.0) / 3 = 10/3 = 3.333... -> 3.33
        result.Should().Be(3.33m);
    }

    [Fact]
    public void CalculateGPA_WithDroppedCourses_ExcludesDropped()
    {
        var courses = new List<TranscriptCourseDto>
        {
            new TranscriptCourseDto
            {
                CreditHours = 3,
                Grade = Grade.A,
                Status = EnrollmentStatus.Completed
            },
            new TranscriptCourseDto
            {
                CreditHours = 3,
                Grade = Grade.F,
                Status = EnrollmentStatus.Dropped
            }
        };

        var result = _service.CalculateGPA(courses);

        // Only the completed course should count
        result.Should().Be(4.0m);
    }
}
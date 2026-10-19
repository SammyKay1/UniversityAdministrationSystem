using System;
using System.Text.Json.Serialization;

namespace UniversityAdministrationSystem.IntegrationTests.Api;

public class EnrollmentListDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("courseName")]
    public string CourseName { get; set; } = "";

    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = "";

    [JsonPropertyName("enrollmentDate")]
    public DateTime EnrollmentDate { get; set; }

    [JsonPropertyName("grade")]
    [JsonConverter(typeof(EnumStringConverter<Grade>))]
    public Grade Grade { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(EnumStringConverter<EnrollmentStatus>))]
    public EnrollmentStatus Status { get; set; }
}

public class EnrollmentResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
}

public class DropEnrollmentResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
}

public class CompleteEnrollmentResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
}

public class AssignGradeResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
}

public class AssignGradeDto
{
    [JsonPropertyName("enrollmentId")]
    public int EnrollmentId { get; set; }

    [JsonPropertyName("grade")]
    public string Grade { get; set; } = "";
}
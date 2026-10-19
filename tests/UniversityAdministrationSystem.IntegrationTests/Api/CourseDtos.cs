using System;
using System.Text.Json.Serialization;

namespace UniversityAdministrationSystem.IntegrationTests.Api;

public class CourseListDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("creditHours")]
    public int CreditHours { get; set; }

    [JsonPropertyName("capacity")]
    public int Capacity { get; set; }

    [JsonPropertyName("isArchived")]
    public bool IsArchived { get; set; }

    [JsonPropertyName("departmentName")]
    public string DepartmentName { get; set; } = "";

    [JsonPropertyName("teacherName")]
    public string? TeacherName { get; set; }
}

public class CourseDetailsDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("creditHours")]
    public int CreditHours { get; set; }

    [JsonPropertyName("capacity")]
    public int Capacity { get; set; }

    [JsonPropertyName("isArchived")]
    public bool IsArchived { get; set; }

    [JsonPropertyName("departmentId")]
    public int DepartmentId { get; set; }

    [JsonPropertyName("departmentName")]
    public string DepartmentName { get; set; } = "";

    [JsonPropertyName("teacherId")]
    public int? TeacherId { get; set; }

    [JsonPropertyName("teacherName")]
    public string? TeacherName { get; set; }

    [JsonPropertyName("createdDateUtc")]
    public DateTime CreatedDateUtc { get; set; }

    [JsonPropertyName("createdBy")]
    public string CreatedBy { get; set; } = "";

    [JsonPropertyName("modifiedDateUtc")]
    public DateTime? ModifiedDateUtc { get; set; }

    [JsonPropertyName("modifiedBy")]
    public string? ModifiedBy { get; set; }

    [JsonPropertyName("concurrencyToken")]
    public Guid ConcurrencyToken { get; set; }
}

public class CourseLookupDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UniversityAdministrationSystem.IntegrationTests.Api;

public class TeacherListDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = "";

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("departmentName")]
    public string DepartmentName { get; set; } = "";

    [JsonPropertyName("courseCount")]
    public int CourseCount { get; set; }
}

public class TeacherLookupDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = "";

    [JsonPropertyName("departmentName")]
    public string DepartmentName { get; set; } = "";
}

public class TeacherDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = "";

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = "";

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("dateOfBirth")]
    public DateTime DateOfBirth { get; set; }

    [JsonPropertyName("departmentId")]
    public int DepartmentId { get; set; }

    [JsonPropertyName("departmentName")]
    public string DepartmentName { get; set; } = "";

    [JsonPropertyName("courseCount")]
    public int CourseCount { get; set; }

    [JsonPropertyName("courses")]
    public List<TeacherCourseDto>? Courses { get; set; }
}

public class TeacherCourseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}
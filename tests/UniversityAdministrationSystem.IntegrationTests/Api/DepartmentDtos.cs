using System;
using System.Text.Json.Serialization;

namespace UniversityAdministrationSystem.IntegrationTests.Api;

public class DepartmentDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("code")]
    public string Code { get; set; } = "";

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("studentCount")]
    public int StudentCount { get; set; }

    [JsonPropertyName("teacherCount")]
    public int TeacherCount { get; set; }
}

public class DepartmentLookUpDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}
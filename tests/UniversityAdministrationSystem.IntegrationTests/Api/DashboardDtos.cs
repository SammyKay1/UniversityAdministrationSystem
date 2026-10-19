using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UniversityAdministrationSystem.IntegrationTests.Api;

public class EnumStringConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var value = reader.GetInt32();
            return (T)Enum.ToObject(typeof(T), value);
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (Enum.TryParse(typeof(T), stringValue, true, out var result))
            {
                return (T)result;
            }
        }
        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public class DashboardDto
{
    [JsonPropertyName("totalStudents")]
    public int TotalStudents { get; set; }

    [JsonPropertyName("totalTeachers")]
    public int TotalTeachers { get; set; }

    [JsonPropertyName("totalCourses")]
    public int TotalCourses { get; set; }

    [JsonPropertyName("totalDepartments")]
    public int TotalDepartments { get; set; }

    [JsonPropertyName("totalEnrollments")]
    public int TotalEnrollments { get; set; }

    [JsonPropertyName("activeEnrollments")]
    public int ActiveEnrollments { get; set; }

    [JsonPropertyName("completedEnrollments")]
    public int CompletedEnrollments { get; set; }

    [JsonPropertyName("droppedEnrollments")]
    public int DroppedEnrollments { get; set; }

    [JsonPropertyName("archivedCourses")]
    public int ArchivedCourses { get; set; }

    [JsonPropertyName("recentEnrollments")]
    public IEnumerable<RecentEnrollmentDto> RecentEnrollments { get; set; } = [];

    [JsonPropertyName("departmentStatistics")]
    public IEnumerable<DepartmentStatisticsDto> DepartmentStatistics { get; set; } = [];
}

public class RecentEnrollmentDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("studentName")]
    public string StudentName { get; set; } = "";

    [JsonPropertyName("courseName")]
    public string CourseName { get; set; } = "";

    [JsonPropertyName("enrollmentDate")]
    public DateTime EnrollmentDate { get; set; }
}

public class DepartmentStatisticsDto
{
    [JsonPropertyName("departmentId")]
    public int DepartmentId { get; set; }

    [JsonPropertyName("departmentName")]
    public string DepartmentName { get; set; } = "";

    [JsonPropertyName("studentCount")]
    public int StudentCount { get; set; }

    [JsonPropertyName("teacherCount")]
    public int TeacherCount { get; set; }

    [JsonPropertyName("courseCount")]
    public int CourseCount { get; set; }

    [JsonPropertyName("enrollmentCount")]
    public int EnrollmentCount { get; set; }
}

public class DepartmentReportDto
{
    [JsonPropertyName("departmentId")]
    public int DepartmentId { get; set; }

    [JsonPropertyName("departmentName")]
    public string DepartmentName { get; set; } = "";

    [JsonPropertyName("departmentCode")]
    public string DepartmentCode { get; set; } = "";

    [JsonPropertyName("studentCount")]
    public int StudentCount { get; set; }

    [JsonPropertyName("teacherCount")]
    public int TeacherCount { get; set; }

    [JsonPropertyName("courseCount")]
    public int CourseCount { get; set; }

    [JsonPropertyName("enrollmentCount")]
    public int EnrollmentCount { get; set; }
}

public class EnrollmentReportDto
{
    [JsonPropertyName("courseId")]
    public int CourseId { get; set; }

    [JsonPropertyName("departmentId")]
    public int DepartmentId { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(EnumStringConverter<EnrollmentStatus>))]
    public EnrollmentStatus Status { get; set; }

    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = "";

    [JsonPropertyName("courseName")]
    public string CourseName { get; set; } = "";

    [JsonPropertyName("departmentName")]
    public string DepartmentName { get; set; } = "";

    [JsonPropertyName("activeCount")]
    public int ActiveCount { get; set; }

    [JsonPropertyName("completedCount")]
    public int CompletedCount { get; set; }

    [JsonPropertyName("droppedCount")]
    public int DroppedCount { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
}

public class AcademicPerformanceReportDto
{
    [JsonPropertyName("courseId")]
    public int CourseId { get; set; }

    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = "";

    [JsonPropertyName("courseName")]
    public string CourseName { get; set; } = "";

    [JsonPropertyName("departmentId")]
    public int DepartmentId { get; set; }

    [JsonPropertyName("departmentName")]
    public string DepartmentName { get; set; } = "";

    [JsonPropertyName("gradedCount")]
    public int GradedCount { get; set; }

    [JsonPropertyName("pendingGradeCount")]
    public int PendingGradeCount { get; set; }

    [JsonPropertyName("aCount")]
    public int ACount { get; set; }

    [JsonPropertyName("bCount")]
    public int BCount { get; set; }

    [JsonPropertyName("cCount")]
    public int CCount { get; set; }

    [JsonPropertyName("dCount")]
    public int DCount { get; set; }

    [JsonPropertyName("fCount")]
    public int FCount { get; set; }

    [JsonPropertyName("failedCount")]
    public int FailedCount { get; set; }

    [JsonPropertyName("passedCount")]
    public int PassedCount { get; set; }

    [JsonPropertyName("passRate")]
    public double PassRate { get; set; }
}
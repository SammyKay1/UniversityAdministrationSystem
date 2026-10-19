using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniversityAdministrationSystem;
using UniversityAdministrationSystem.IntegrationTests.Fixtures;
using Xunit;

namespace UniversityAdministrationSystem.IntegrationTests.Api;

[Collection("Integration")]
public class ReportsApiIntegrationTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private readonly IntegrationTestFactory _factory;
    private HttpClient _client = null!;

    public ReportsApiIntegrationTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetDepartmentReport_Anonymous_ReturnsUnauthorizedOrRedirect()
    {
        var response = await _client.GetAsync("/api/reports/departments");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GetDepartmentReport_AuthenticatedWithPermission_ReturnsOk()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/reports/departments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<DepartmentReportDto>>();
        report.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDepartmentReport_ReturnsSeededDepartment()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/reports/departments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<DepartmentReportDto>>();
        report.Should().NotBeNull();
        report!.Should().Contain(r => r.DepartmentCode == "CS");
    }

    [Fact]
    public async Task GetDepartmentReport_WithData_ReturnsCorrectCounts()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        // Create test data
        var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var dept = new Department
        {
            Name = "Report Test",
            Code = "RPT",
            Email = "rpt@university.edu",
            Description = "Report Test Department"
        };
        context.Departments.Add(dept);
        await context.SaveChangesAsync();

        var student = new Student
        {
            FirstName = "Report",
            LastName = "Student",
            Email = "report.student@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = dept.Id
        };
        context.Students.Add(student);

        var teacher = new Teacher
        {
            FirstName = "Report",
            LastName = "Teacher",
            Email = "report.teacher@test.com",
            DateOfBirth = DateTime.Today.AddYears(-35),
            DepartmentId = dept.Id
        };
        context.Teachers.Add(teacher);

        var course = new Course("RPT101", "Report Course", "Description", 3, 30, dept.Id);
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync();

        var response = await adminClient.GetAsync("/api/reports/departments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<DepartmentReportDto>>();
        report.Should().NotBeNull();
        var deptReport = report!.FirstOrDefault(r => r.DepartmentCode == "RPT");
        deptReport.Should().NotBeNull();
        deptReport!.StudentCount.Should().BeGreaterOrEqualTo(1);
        deptReport.TeacherCount.Should().BeGreaterOrEqualTo(1);
        deptReport.CourseCount.Should().BeGreaterOrEqualTo(1);
        deptReport.EnrollmentCount.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GetDepartmentReport_StructureValidation()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/reports/departments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<DepartmentReportDto>>();
        report.Should().NotBeNull();
        
        foreach (var item in report!)
        {
            item.DepartmentId.Should().BeGreaterThan(0);
            item.DepartmentName.Should().NotBeNullOrEmpty();
            item.DepartmentCode.Should().NotBeNullOrEmpty();
            item.StudentCount.Should().BeGreaterOrEqualTo(0);
            item.TeacherCount.Should().BeGreaterOrEqualTo(0);
            item.CourseCount.Should().BeGreaterOrEqualTo(0);
            item.EnrollmentCount.Should().BeGreaterOrEqualTo(0);
        }
    }

    [Fact]
    public async Task GetEnrollmentReport_Anonymous_ReturnsUnauthorizedOrRedirect()
    {
        var response = await _client.GetAsync("/api/reports/enrollments");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GetEnrollmentReport_AuthenticatedWithPermission_ReturnsOk()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/reports/enrollments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<EnrollmentReportDto>>();
        report.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEnrollmentReport_WithData_ReturnsCorrectStatusCounts()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        // Create test data
        var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var dept = new Department
        {
            Name = "Enrollment Report",
            Code = "ENRPT",
            Email = "enrpt@university.edu",
            Description = "Enrollment Report Department"
        };
        context.Departments.Add(dept);
        await context.SaveChangesAsync();

        var course = new Course("ENR101", "Enrollment Course", "Description", 3, 30, dept.Id);
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var student1 = new Student
        {
            FirstName = "Active",
            LastName = "Student",
            Email = "active.student@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = dept.Id
        };
        var student2 = new Student
        {
            FirstName = "Completed",
            LastName = "Student",
            Email = "completed.student@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = dept.Id
        };
        var student3 = new Student
        {
            FirstName = "Dropped",
            LastName = "Student",
            Email = "dropped.student@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = dept.Id
        };
        context.Students.AddRange(student1, student2, student3);
        await context.SaveChangesAsync();

        var enrollment1 = new Enrollment(student1.Id, course.Id); // Defaults to Active
        var enrollment2 = new Enrollment(student2.Id, course.Id);
        enrollment2.Complete(); // Set to Completed
        var enrollment3 = new Enrollment(student3.Id, course.Id);
        enrollment3.Drop(); // Set to Dropped
        context.Enrollments.AddRange(enrollment1, enrollment2, enrollment3);
        await context.SaveChangesAsync();

        var response = await adminClient.GetAsync("/api/reports/enrollments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<EnrollmentReportDto>>();
        report.Should().NotBeNull();
        
        var courseReport = report!.FirstOrDefault(r => r.CourseCode == "ENR101");
        courseReport.Should().NotBeNull();
        courseReport!.ActiveCount.Should().BeGreaterOrEqualTo(1);
        courseReport.CompletedCount.Should().BeGreaterOrEqualTo(1);
        courseReport.DroppedCount.Should().BeGreaterOrEqualTo(1);
        courseReport.TotalCount.Should().Be(courseReport.ActiveCount + courseReport.CompletedCount + courseReport.DroppedCount);
    }

    [Fact]
    public async Task GetEnrollmentReport_StructureValidation()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/reports/enrollments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<EnrollmentReportDto>>();
        report.Should().NotBeNull();
        
        foreach (var item in report!)
        {
            item.CourseId.Should().BeGreaterThan(0);
            item.CourseCode.Should().NotBeNullOrEmpty();
            item.CourseName.Should().NotBeNullOrEmpty();
            item.DepartmentId.Should().BeGreaterThan(0);
            item.DepartmentName.Should().NotBeNullOrEmpty();
            item.ActiveCount.Should().BeGreaterOrEqualTo(0);
            item.CompletedCount.Should().BeGreaterOrEqualTo(0);
            item.DroppedCount.Should().BeGreaterOrEqualTo(0);
            item.TotalCount.Should().Be(item.ActiveCount + item.CompletedCount + item.DroppedCount);
        }
    }

    [Fact]
    public async Task GetAcademicPerformanceReport_Anonymous_ReturnsUnauthorizedOrRedirect()
    {
        var response = await _client.GetAsync("/api/reports/academic-performance");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GetAcademicPerformanceReport_AuthenticatedWithPermission_ReturnsOk()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/reports/academic-performance");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<AcademicPerformanceReportDto>>();
        report.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAcademicPerformanceReport_WithGradedEnrollments_ReturnsCorrectGradeCounts()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        // Create test data
        var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var dept = new Department
        {
            Name = "Academic Perf",
            Code = "ACPERF",
            Email = "acperf@university.edu",
            Description = "Academic Performance Department"
        };
        context.Departments.Add(dept);
        await context.SaveChangesAsync();

        var course = new Course("ACP101", "Academic Course", "Description", 3, 30, dept.Id);
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Create students with graded enrollments
        var students = new List<Student>
        {
            new() { FirstName = "Grade", LastName = "A1", Email = "a1@test.com", DateOfBirth = DateTime.Today.AddYears(-20), DepartmentId = dept.Id },
            new() { FirstName = "Grade", LastName = "A2", Email = "a2@test.com", DateOfBirth = DateTime.Today.AddYears(-20), DepartmentId = dept.Id },
            new() { FirstName = "Grade", LastName = "B1", Email = "b1@test.com", DateOfBirth = DateTime.Today.AddYears(-20), DepartmentId = dept.Id },
            new() { FirstName = "Grade", LastName = "C1", Email = "c1@test.com", DateOfBirth = DateTime.Today.AddYears(-20), DepartmentId = dept.Id },
            new() { FirstName = "Grade", LastName = "F1", Email = "f1@test.com", DateOfBirth = DateTime.Today.AddYears(-20), DepartmentId = dept.Id },
        };
        context.Students.AddRange(students);
        await context.SaveChangesAsync();

        // A grades
        var enrollment1 = new Enrollment(students[0].Id, course.Id);
        enrollment1.Complete();
        enrollment1.AssignGrade(Grade.A);
        context.Enrollments.Add(enrollment1);

        var enrollment2 = new Enrollment(students[1].Id, course.Id);
        enrollment2.Complete();
        enrollment2.AssignGrade(Grade.A);
        context.Enrollments.Add(enrollment2);

        // B grade
        var enrollment3 = new Enrollment(students[2].Id, course.Id);
        enrollment3.Complete();
        enrollment3.AssignGrade(Grade.B);
        context.Enrollments.Add(enrollment3);

        // C grade
        var enrollment4 = new Enrollment(students[3].Id, course.Id);
        enrollment4.Complete();
        enrollment4.AssignGrade(Grade.C);
        context.Enrollments.Add(enrollment4);

        // F grade
        var enrollment5 = new Enrollment(students[4].Id, course.Id);
        enrollment5.Complete();
        enrollment5.AssignGrade(Grade.F);
        context.Enrollments.Add(enrollment5);

        await context.SaveChangesAsync();

        var response = await adminClient.GetAsync("/api/reports/academic-performance");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<AcademicPerformanceReportDto>>();
        report.Should().NotBeNull();
        
        var courseReport = report!.FirstOrDefault(r => r.CourseCode == "ACP101");
        courseReport.Should().NotBeNull();
        courseReport!.GradedCount.Should().BeGreaterOrEqualTo(5);
        courseReport.ACount.Should().BeGreaterOrEqualTo(2);
        courseReport.BCount.Should().BeGreaterOrEqualTo(1);
        courseReport.CCount.Should().BeGreaterOrEqualTo(1);
        courseReport.FCount.Should().BeGreaterOrEqualTo(1);
        courseReport.PassedCount.Should().BeGreaterOrEqualTo(4); // A, B, C are passing
        courseReport.FailedCount.Should().BeGreaterOrEqualTo(1); // F is failing
        courseReport.PassRate.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAcademicPerformanceReport_StructureValidation()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/reports/academic-performance");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<AcademicPerformanceReportDto>>();
        report.Should().NotBeNull();
        
        foreach (var item in report!)
        {
            item.CourseId.Should().BeGreaterThan(0);
            item.CourseCode.Should().NotBeNullOrEmpty();
            item.CourseName.Should().NotBeNullOrEmpty();
            item.DepartmentId.Should().BeGreaterThan(0);
            item.DepartmentName.Should().NotBeNullOrEmpty();
            item.GradedCount.Should().BeGreaterOrEqualTo(0);
            item.PendingGradeCount.Should().BeGreaterOrEqualTo(0);
            item.ACount.Should().BeGreaterOrEqualTo(0);
            item.BCount.Should().BeGreaterOrEqualTo(0);
            item.CCount.Should().BeGreaterOrEqualTo(0);
            item.DCount.Should().BeGreaterOrEqualTo(0);
            item.FCount.Should().BeGreaterOrEqualTo(0);
            item.FailedCount.Should().BeGreaterOrEqualTo(0);
            item.PassedCount.Should().BeGreaterOrEqualTo(0);
            item.PassRate.Should().BeGreaterOrEqualTo(0);
            item.PassRate.Should().BeLessOrEqualTo(100);
        }
    }
}
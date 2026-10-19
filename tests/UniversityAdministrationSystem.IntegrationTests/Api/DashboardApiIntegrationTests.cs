using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UniversityAdministrationSystem;
using UniversityAdministrationSystem.IntegrationTests.Fixtures;
using Xunit;

namespace UniversityAdministrationSystem.IntegrationTests.Api;

[Collection("Integration")]
public class DashboardApiIntegrationTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private readonly IntegrationTestFactory _factory;
    private HttpClient _client = null!;

    public DashboardApiIntegrationTests(IntegrationTestFactory factory)
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
    public async Task GetDashboard_Anonymous_ReturnsUnauthorizedOrRedirect()
    {
        var response = await _client.GetAsync("/api/dashboard");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GetDashboard_AuthenticatedWithPermission_ReturnsOk()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dashboard = await response.Content.ReadFromJsonAsync<DashboardDto>();
        dashboard.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDashboard_ReturnsCorrectStructure()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dashboard = await response.Content.ReadFromJsonAsync<DashboardDto>();
        dashboard.Should().NotBeNull();
        dashboard!.TotalStudents.Should().BeGreaterOrEqualTo(0);
        dashboard.TotalTeachers.Should().BeGreaterOrEqualTo(0);
        dashboard.TotalCourses.Should().BeGreaterOrEqualTo(0);
        dashboard.TotalDepartments.Should().BeGreaterOrEqualTo(0);
        dashboard.TotalEnrollments.Should().BeGreaterOrEqualTo(0);
        dashboard.ActiveEnrollments.Should().BeGreaterOrEqualTo(0);
        dashboard.CompletedEnrollments.Should().BeGreaterOrEqualTo(0);
        dashboard.DroppedEnrollments.Should().BeGreaterOrEqualTo(0);
        dashboard.ArchivedCourses.Should().BeGreaterOrEqualTo(0);
        dashboard.RecentEnrollments.Should().NotBeNull();
        dashboard.DepartmentStatistics.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDashboard_WithData_ReturnsPopulatedCounts()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        // Create test data
        var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var dept = new Department
        {
            Name = "Dashboard Test",
            Code = "DASH",
            Email = "dash@university.edu",
            Description = "Dashboard Test Department"
        };
        context.Departments.Add(dept);
        await context.SaveChangesAsync();

        var student = new Student
        {
            FirstName = "Dashboard",
            LastName = "Student",
            Email = "dashboard.student@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = dept.Id
        };
        context.Students.Add(student);

        var teacher = new Teacher
        {
            FirstName = "Dashboard",
            LastName = "Teacher",
            Email = "dashboard.teacher@test.com",
            DateOfBirth = DateTime.Today.AddYears(-35),
            DepartmentId = dept.Id
        };
        context.Teachers.Add(teacher);

        var course = new Course("DASH101", "Dashboard Course", "Description", 3, 30, dept.Id);
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync();

        var response = await adminClient.GetAsync("/api/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dashboard = await response.Content.ReadFromJsonAsync<DashboardDto>();
        dashboard.Should().NotBeNull();
        dashboard!.TotalStudents.Should().BeGreaterOrEqualTo(1);
        dashboard.TotalTeachers.Should().BeGreaterOrEqualTo(1);
        dashboard.TotalCourses.Should().BeGreaterOrEqualTo(1);
        dashboard.TotalDepartments.Should().BeGreaterOrEqualTo(1);
        dashboard.TotalEnrollments.Should().BeGreaterOrEqualTo(1);
        dashboard.ActiveEnrollments.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GetDashboard_DepartmentStatistics_ReturnsDepartmentData()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dashboard = await response.Content.ReadFromJsonAsync<DashboardDto>();
        dashboard.Should().NotBeNull();
        dashboard!.DepartmentStatistics.Should().NotBeNull();
        
        var deptStats = dashboard.DepartmentStatistics.ToList();
        deptStats.Should().NotBeEmpty();
        
        foreach (var stat in deptStats)
        {
            stat.DepartmentId.Should().BeGreaterThan(0);
            stat.DepartmentName.Should().NotBeNullOrEmpty();
            stat.StudentCount.Should().BeGreaterOrEqualTo(0);
            stat.TeacherCount.Should().BeGreaterOrEqualTo(0);
            stat.CourseCount.Should().BeGreaterOrEqualTo(0);
            stat.EnrollmentCount.Should().BeGreaterOrEqualTo(0);
        }
    }

    [Fact]
    public async Task GetDashboard_RecentEnrollments_ReturnsRecentData()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dashboard = await response.Content.ReadFromJsonAsync<DashboardDto>();
        dashboard.Should().NotBeNull();
        dashboard!.RecentEnrollments.Should().NotBeNull();
        
        var recent = dashboard.RecentEnrollments.ToList();
        foreach (var enrollment in recent)
        {
            enrollment.Id.Should().BeGreaterThan(0);
            enrollment.StudentName.Should().NotBeNullOrEmpty();
            enrollment.CourseName.Should().NotBeNullOrEmpty();
            enrollment.EnrollmentDate.Should().NotBe(default);
        }
    }
}
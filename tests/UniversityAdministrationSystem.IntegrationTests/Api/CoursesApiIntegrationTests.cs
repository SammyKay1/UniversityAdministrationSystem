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
public class CoursesApiIntegrationTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private readonly IntegrationTestFactory _factory;
    private HttpClient _client = null!;
    private ApplicationDbContext _context = null!;

    public CoursesApiIntegrationTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _context = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetCourses_Anonymous_ReturnsUnauthorizedOrRedirect()
    {
        var response = await _client.GetAsync("/api/courses");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GetCourses_AuthenticatedWithPermission_ReturnsOk()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/courses");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var courses = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<CourseListDto>>();
        courses.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCourse_ValidId_ReturnsCourseDetails()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var course = new Course("API101", "API Course", "Description", 3, 30, 1);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var response = await adminClient.GetAsync($"/api/courses/{course.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var courseDto = await response.Content.ReadFromJsonAsync<CourseDetailsDto>();
        courseDto.Should().NotBeNull();
        courseDto!.CourseCode.Should().Be("API101");
        courseDto.Name.Should().Be("API Course");
    }

    [Fact]
    public async Task GetCourse_NonExistentId_ReturnsNotFound()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/courses/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCourse_ValidRequest_ReturnsCreatedAndPersists()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var dto = new
        {
            CourseCode = "NEW101",
            Name = "New Course",
            Description = "New course description",
            CreditHours = 3,
            Capacity = 25,
            DepartmentId = 1,
            TeacherId = (int?)null
        };

        var response = await adminClient.PostAsJsonAsync("/api/courses", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdCourse = await _context.Courses.FirstOrDefaultAsync<Course>(c => c.CourseCode == "NEW101");
        createdCourse.Should().NotBeNull();
        createdCourse!.Name.Should().Be("New Course");
        createdCourse.CreditHours.Should().Be(3);
        createdCourse.Capacity.Should().Be(25);
    }

    [Fact]
    public async Task CreateCourse_DuplicateCourseCode_ReturnsConflict()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var existingCourse = new Course("DUP101", "Existing Course", "Description", 3, 30, 1);
        _context.Courses.Add(existingCourse);
        await _context.SaveChangesAsync();

        var dto = new
        {
            CourseCode = "DUP101",
            Name = "Duplicate Course",
            Description = "Should fail",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1
        };

        var response = await adminClient.PostAsJsonAsync("/api/courses", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateCourse_InvalidCreditHours_ReturnsValidationProblem()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var dto = new
        {
            CourseCode = "INV101",
            Name = "Invalid Course",
            CreditHours = 0,
            Capacity = 30,
            DepartmentId = 1
        };

        var response = await adminClient.PostAsJsonAsync("/api/courses", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCourse_InvalidCapacity_ReturnsValidationProblem()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var dto = new
        {
            CourseCode = "CAP101",
            Name = "Capacity Course",
            CreditHours = 3,
            Capacity = 0,
            DepartmentId = 1
        };

        var response = await adminClient.PostAsJsonAsync("/api/courses", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCourse_ValidRequest_ReturnsOk()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var course = new Course("UPD101", "Original", "Description", 3, 30, 1);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var dto = new
        {
            Id = course.Id,
            CourseCode = "UPD101",
            Name = "Updated Course",
            Description = "Updated description",
            CreditHours = 4,
            Capacity = 35,
            DepartmentId = 1
        };

        var response = await adminClient.PutAsJsonAsync($"/api/courses/{course.Id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Use a fresh DbContext to verify persistence (avoid tracked entity)
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updated = await verifyContext.Courses.FindAsync(course.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Course");
        updated.CreditHours.Should().Be(4);
        updated.Capacity.Should().Be(35);
    }

    [Fact]
    public async Task UpdateCourse_DuplicateCourseCode_ReturnsConflict()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var course1 = new Course("ORIG101", "Original", "Description", 3, 30, 1);
        var course2 = new Course("OTHER101", "Other", "Description", 3, 30, 1);
        _context.Courses.AddRange(course1, course2);
        await _context.SaveChangesAsync();

        var dto = new
        {
            Id = course1.Id,
            CourseCode = "OTHER101",
            Name = "Updated",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1
        };

        var response = await adminClient.PutAsJsonAsync($"/api/courses/{course1.Id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ArchiveCourse_ValidCourse_ReturnsOk()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var course = new Course("ARC101", "Archive Course", "Description", 3, 30, 1);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var response = await adminClient.DeleteAsync($"/api/courses/{course.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Use a fresh DbContext to verify persistence (avoid tracked entity)
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var archived = await verifyContext.Courses.FindAsync(course.Id);
        archived.Should().NotBeNull();
        archived!.IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task ArchiveCourse_WithActiveEnrollments_ReturnsConflict()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Enrolled",
            LastName = "Student",
            Email = "enrolled.archive@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };

        var course = new Course("ARC_ENR", "Archive Enrolled", "Description", 3, 30, 1);

        _context.Students.Add(student);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        var response = await adminClient.DeleteAsync($"/api/courses/{course.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetCourseLookup_ReturnsLookupList()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/courses/lookup");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lookups = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<CourseLookupDto>>();
        lookups.Should().NotBeNull();
    }
}
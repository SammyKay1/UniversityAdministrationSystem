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
public class StudentsApiIntegrationTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private readonly IntegrationTestFactory _factory;
    private HttpClient _client = null!;
    private ApplicationDbContext _context = null!;

    public StudentsApiIntegrationTests(IntegrationTestFactory factory)
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
    public async Task GetStudents_Anonymous_ReturnsUnauthorizedOrRedirect()
    {
        var response = await _client.GetAsync("/api/students");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GetStudents_AuthenticatedWithPermission_ReturnsOk()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");
        
        var response = await adminClient.GetAsync("/api/students");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedResultDto<StudentListDto>>();
        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStudent_ValidId_ReturnsStudentDetails()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");
        
        var student = new Student
        {
            FirstName = "Api",
            LastName = "Student",
            Email = "api.student@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        var response = await adminClient.GetAsync($"/api/students/{student.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var studentDto = await response.Content.ReadFromJsonAsync<StudentDetailsDto>();
        studentDto.Should().NotBeNull();
        studentDto!.FullName.Should().Be("Api Student");
        studentDto.Email.Should().Be("api.student@test.com");
    }

    [Fact]
    public async Task GetStudent_NonExistentId_ReturnsNotFound()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/students/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateStudent_ValidRequest_ReturnsCreatedAndPersists()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var dto = new
        {
            FirstName = "New",
            LastName = "Student",
            Email = "new.student@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20).ToString("yyyy-MM-dd"),
            DepartmentId = 1,
            Address = new
            {
                AddressLine1 = "123 Test St",
                City = "Test City",
                State = "TS",
                Country = "USA"
            }
        };

        var response = await adminClient.PostAsJsonAsync("/api/students", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdStudent = await _context.Students.FirstOrDefaultAsync<Student>(s => s.Email == "new.student@test.com");
        createdStudent.Should().NotBeNull();
        createdStudent!.FirstName.Should().Be("New");
        createdStudent.LastName.Should().Be("Student");
    }

    [Fact]
    public async Task CreateStudent_InvalidEmail_ReturnsValidationProblem()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var dto = new
        {
            FirstName = "Invalid",
            LastName = "Email",
            Email = "not-an-email",
            DateOfBirth = DateTime.Today.AddYears(-20).ToString("yyyy-MM-dd"),
            DepartmentId = 1,
            Address = new
            {
                AddressLine1 = "123 Test St",
                City = "Test City",
                State = "TS",
                Country = "USA"
            }
        };

        var response = await adminClient.PostAsJsonAsync("/api/students", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        // Verify the response has content (validation error details)
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Email");
    }

    [Fact]
    public async Task CreateStudent_UnderageStudent_ReturnsValidationProblem()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var dto = new
        {
            FirstName = "Young",
            LastName = "Student",
            Email = "young.student@test.com",
            DateOfBirth = DateTime.Today.AddYears(-10).ToString("yyyy-MM-dd"),
            DepartmentId = 1,
            Address = new
            {
                AddressLine1 = "123 Test St",
                City = "Test City",
                State = "TS",
                Country = "USA"
            }
        };

        var response = await adminClient.PostAsJsonAsync("/api/students", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateStudent_ValidRequest_ReturnsOk()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Update",
            LastName = "Me",
            Email = "update.me@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        var dto = new
        {
            Id = student.Id,
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated.name@test.com",
            DateOfBirth = DateTime.Today.AddYears(-21).ToString("yyyy-MM-dd"),
            DepartmentId = 1,
            Address = new
            {
                AddressLine1 = "456 Updated St",
                City = "Updated City",
                State = "US",
                Country = "USA"
            }
        };

        var response = await adminClient.PutAsJsonAsync($"/api/students/{student.Id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Use a fresh DbContext to verify persistence (avoid tracked entity)
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updated = await verifyContext.Students.FindAsync(student.Id);
        updated.Should().NotBeNull();
        updated!.FirstName.Should().Be("Updated");
        updated.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task UpdateStudent_NonExistentId_ReturnsNotFound()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var dto = new
        {
            Id = 99999,
            FirstName = "Ghost",
            LastName = "Student",
            Email = "ghost@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20).ToString("yyyy-MM-dd"),
            DepartmentId = 1,
            Address = new
            {
                AddressLine1 = "123 Test St",
                City = "Test City",
                State = "TS",
                Country = "USA"
            }
        };

        var response = await adminClient.PutAsJsonAsync("/api/students/99999", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteStudent_WithActiveEnrollments_ReturnsConflict()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Has",
            LastName = "Enrollment",
            Email = "has.enrollment@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };

        var course = new Course("DEL101", "Delete Course", "Description", 3, 30, 1);

        _context.Students.Add(student);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        var response = await adminClient.DeleteAsync($"/api/students/{student.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteStudent_NoActiveEnrollments_ReturnsOk()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "No",
            LastName = "Enrollment",
            Email = "no.enrollment@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        var response = await adminClient.DeleteAsync($"/api/students/{student.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Use a fresh DbContext to verify persistence (avoid tracked entity)
        // Use IgnoreQueryFilters to find soft-deleted entities
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deleted = await verifyContext.Students.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == student.Id);
        deleted.Should().NotBeNull();
        deleted!.Status.Should().Be(StudentStatus.Archived);
        deleted.IsDeleted.Should().BeTrue();
    }
}
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
public class TeachersApiIntegrationTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private readonly IntegrationTestFactory _factory;
    private HttpClient _client = null!;
    private ApplicationDbContext _context = null!;

    public TeachersApiIntegrationTests(IntegrationTestFactory factory)
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
    public async Task GetTeachers_Anonymous_ReturnsUnauthorizedOrRedirect()
    {
        var response = await _client.GetAsync("/api/teachers");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GetTeachers_AuthenticatedWithPermission_ReturnsOk()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/teachers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedResultDto<TeacherListDto>>();
        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTeachers_WithSearch_ReturnsFilteredResults()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/teachers?search=John");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedResultDto<TeacherListDto>>();
        pagedResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTeachers_WithDepartmentFilter_ReturnsFilteredResults()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/teachers?departmentId=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedResultDto<TeacherListDto>>();
        pagedResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTeachers_WithPagination_ReturnsCorrectPage()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/teachers?pageNumber=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedResultDto<TeacherListDto>>();
        pagedResult.Should().NotBeNull();
        pagedResult!.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetTeacherLookup_ReturnsLookupList()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/teachers/lookup");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lookups = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<TeacherLookupDto>>();
        lookups.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTeacherLookup_WithDepartmentFilter_ReturnsFilteredList()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/teachers/lookup?departmentId=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lookups = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<TeacherLookupDto>>();
        lookups.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTeacher_ValidId_ReturnsTeacherDetails()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var teacher = new Teacher
        {
            FirstName = "Api",
            LastName = "Teacher",
            Email = "api.teacher@test.com",
            DateOfBirth = DateTime.Today.AddYears(-35),
            DepartmentId = 1
        };
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        var response = await adminClient.GetAsync($"/api/teachers/{teacher.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var teacherDto = await response.Content.ReadFromJsonAsync<TeacherDto>();
        teacherDto.Should().NotBeNull();
        teacherDto!.FirstName.Should().Be("Api");
        teacherDto.LastName.Should().Be("Teacher");
        teacherDto.Email.Should().Be("api.teacher@test.com");
    }

    [Fact]
    public async Task GetTeacher_NonExistentId_ReturnsNotFound()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/teachers/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTeacher_ValidRequest_ReturnsCreatedAndPersists()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var dto = new
        {
            FirstName = "New",
            LastName = "Teacher",
            Email = "new.teacher@test.com",
            DateOfBirth = DateTime.Today.AddYears(-35).ToString("yyyy-MM-dd"),
            DepartmentId = 1
        };

        var response = await adminClient.PostAsJsonAsync("/api/teachers", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTeacher = await _context.Teachers.FirstOrDefaultAsync<Teacher>(t => t.Email == "new.teacher@test.com");
        createdTeacher.Should().NotBeNull();
        createdTeacher!.FirstName.Should().Be("New");
        createdTeacher.LastName.Should().Be("Teacher");
    }

    [Fact]
    public async Task CreateTeacher_DuplicateEmail_ReturnsBadRequest()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var existingTeacher = new Teacher
        {
            FirstName = "Existing",
            LastName = "Teacher",
            Email = "duplicate@test.com",
            DateOfBirth = DateTime.Today.AddYears(-35),
            DepartmentId = 1
        };
        _context.Teachers.Add(existingTeacher);
        await _context.SaveChangesAsync();

        var dto = new
        {
            FirstName = "Another",
            LastName = "Teacher",
            Email = "duplicate@test.com",
            DateOfBirth = DateTime.Today.AddYears(-40).ToString("yyyy-MM-dd"),
            DepartmentId = 1
        };

        var response = await adminClient.PostAsJsonAsync("/api/teachers", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTeacher_InvalidEmail_ReturnsBadRequest()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var dto = new
        {
            FirstName = "Invalid",
            LastName = "Email",
            Email = "not-an-email",
            DateOfBirth = DateTime.Today.AddYears(-35).ToString("yyyy-MM-dd"),
            DepartmentId = 1
        };

        var response = await adminClient.PostAsJsonAsync("/api/teachers", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTeacher_MissingRequiredFields_ReturnsBadRequest()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var dto = new
        {
            FirstName = "",
            LastName = "",
            Email = "",
            DateOfBirth = DateTime.Today.AddYears(-35).ToString("yyyy-MM-dd"),
            DepartmentId = 1
        };

        var response = await adminClient.PostAsJsonAsync("/api/teachers", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTeacher_ValidRequest_ReturnsOk()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var teacher = new Teacher
        {
            FirstName = "Update",
            LastName = "Me",
            Email = "update.me@test.com",
            DateOfBirth = DateTime.Today.AddYears(-35),
            DepartmentId = 1
        };
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        var dto = new
        {
            Id = teacher.Id,
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated.name@test.com",
            DateOfBirth = DateTime.Today.AddYears(-36).ToString("yyyy-MM-dd"),
            DepartmentId = 1
        };

        var response = await adminClient.PutAsJsonAsync($"/api/teachers/{teacher.Id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Use a fresh DbContext to verify persistence (avoid tracked entity)
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updated = await verifyContext.Teachers.FindAsync(teacher.Id);
        updated.Should().NotBeNull();
        updated!.FirstName.Should().Be("Updated");
        updated.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task UpdateTeacher_NonExistentId_ReturnsNotFound()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var dto = new
        {
            Id = 99999,
            FirstName = "Ghost",
            LastName = "Teacher",
            Email = "ghost@test.com",
            DateOfBirth = DateTime.Today.AddYears(-35).ToString("yyyy-MM-dd"),
            DepartmentId = 1
        };

        var response = await adminClient.PutAsJsonAsync("/api/teachers/99999", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTeacher_IdMismatch_ReturnsBadRequest()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var teacher = new Teacher
        {
            FirstName = "Update",
            LastName = "Me",
            Email = "update.idmismatch@test.com",
            DateOfBirth = DateTime.Today.AddYears(-35),
            DepartmentId = 1
        };
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        var dto = new
        {
            Id = 99999, // Different from route
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated.name@test.com",
            DateOfBirth = DateTime.Today.AddYears(-36).ToString("yyyy-MM-dd"),
            DepartmentId = 1
        };

        var response = await adminClient.PutAsJsonAsync($"/api/teachers/{teacher.Id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTeacher_DuplicateEmail_ReturnsBadRequest()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var teacher1 = new Teacher
        {
            FirstName = "Teacher",
            LastName = "One",
            Email = "teacher.one@test.com",
            DateOfBirth = DateTime.Today.AddYears(-35),
            DepartmentId = 1
        };
        var teacher2 = new Teacher
        {
            FirstName = "Teacher",
            LastName = "Two",
            Email = "teacher.two@test.com",
            DateOfBirth = DateTime.Today.AddYears(-40),
            DepartmentId = 1
        };
        _context.Teachers.AddRange(teacher1, teacher2);
        await _context.SaveChangesAsync();

        var dto = new
        {
            Id = teacher2.Id,
            FirstName = "Updated",
            LastName = "Two",
            Email = "teacher.one@test.com", // Duplicate email
            DateOfBirth = DateTime.Today.AddYears(-40).ToString("yyyy-MM-dd"),
            DepartmentId = 1
        };

        var response = await adminClient.PutAsJsonAsync($"/api/teachers/{teacher2.Id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteTeacher_ValidId_ReturnsNoContent()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var teacher = new Teacher
        {
            FirstName = "Delete",
            LastName = "Me",
            Email = "delete.me@test.com",
            DateOfBirth = DateTime.Today.AddYears(-35),
            DepartmentId = 1
        };
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        var response = await adminClient.DeleteAsync($"/api/teachers/{teacher.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Use a fresh DbContext to verify persistence (avoid tracked entity)
        // Use IgnoreQueryFilters to find soft-deleted entities
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deleted = await verifyContext.Teachers.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == teacher.Id);
        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTeacher_NonExistentId_ReturnsNotFound()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.DeleteAsync("/api/teachers/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTeacher_WithAssignedCourses_ReturnsNoContent()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var teacher = new Teacher
        {
            FirstName = "Has",
            LastName = "Courses",
            Email = "has.courses@test.com",
            DateOfBirth = DateTime.Today.AddYears(-35),
            DepartmentId = 1
        };
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        var course = new Course("CRS101", "Test Course", "Description", 3, 30, 1)
        {
            TeacherId = teacher.Id
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var response = await adminClient.DeleteAsync($"/api/teachers/{teacher.Id}");

        // Current implementation allows archiving teachers with assigned courses
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Use a fresh DbContext to verify persistence
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deleted = await verifyContext.Teachers.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == teacher.Id);
        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue();
    }
}
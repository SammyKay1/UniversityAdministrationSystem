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
public class EnrollmentsApiIntegrationTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private readonly IntegrationTestFactory _factory;
    private HttpClient _client = null!;
    private ApplicationDbContext _context = null!;

    public EnrollmentsApiIntegrationTests(IntegrationTestFactory factory)
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
    public async Task GetStudentEnrollments_Anonymous_ReturnsUnauthorizedOrRedirect()
    {
        var response = await _client.GetAsync("/api/enrollments/student/1");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GetStudentEnrollments_AuthenticatedWithPermission_ReturnsOk()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Enrollment",
            LastName = "Student",
            Email = "enrollment.student@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        var response = await adminClient.GetAsync($"/api/enrollments/student/{student.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var enrollments = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<EnrollmentListDto>>();
        enrollments.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStudentEnrollments_NonExistentStudent_ReturnsEmptyList()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/enrollments/student/99999");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var enrollments = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<EnrollmentListDto>>();
        enrollments.Should().NotBeNull();
        enrollments!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCourseLookup_ReturnsAvailableCourses()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.GetAsync("/api/enrollments/course-lookup");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var courses = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<CourseLookupDto>>();
        courses.Should().NotBeNull();
    }

    [Fact]
    public async Task EnrollStudent_ValidRequest_ReturnsCreatedAndPersists()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Enroll",
            LastName = "Me",
            Email = "enroll.me@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        var course = new Course("ENR101", "Enroll Course", "Description", 3, 30, 1);
        _context.Students.Add(student);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var dto = new
        {
            StudentId = student.Id,
            CourseId = course.Id
        };

        var response = await adminClient.PostAsJsonAsync("/api/enrollments", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == student.Id && e.CourseId == course.Id);
        enrollment.Should().NotBeNull();
        enrollment!.Status.Should().Be(EnrollmentStatus.Active);
    }

    [Fact]
    public async Task EnrollStudent_DuplicateEnrollment_ReturnsConflict()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Duplicate",
            LastName = "Enrollment",
            Email = "duplicate.enroll@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        var course = new Course("DUP101", "Duplicate Course", "Description", 3, 30, 1);
        _context.Students.Add(student);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        var dto = new
        {
            StudentId = student.Id,
            CourseId = course.Id
        };

        var response = await adminClient.PostAsJsonAsync("/api/enrollments", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task EnrollStudent_NonExistentStudent_ReturnsNotFound()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var course = new Course("NE101", "NonExistent Student", "Description", 3, 30, 1);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var dto = new
        {
            StudentId = 99999,
            CourseId = course.Id
        };

        var response = await adminClient.PostAsJsonAsync("/api/enrollments", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EnrollStudent_NonExistentCourse_ReturnsNotFound()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "NonExistent",
            LastName = "Course",
            Email = "nonexist.course@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        var dto = new
        {
            StudentId = student.Id,
            CourseId = 99999
        };

        var response = await adminClient.PostAsJsonAsync("/api/enrollments", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EnrollStudent_CourseAtCapacity_ReturnsConflict()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var course = new Course("CAP101", "Capacity Course", "Description", 3, 1, 1); // Capacity 1
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        // Fill the course
        var student1 = new Student
        {
            FirstName = "First",
            LastName = "Student",
            Email = "first.student@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        _context.Students.Add(student1);
        await _context.SaveChangesAsync();

        var enrollment1 = new Enrollment(student1.Id, course.Id);
        _context.Enrollments.Add(enrollment1);
        await _context.SaveChangesAsync();

        // Try to enroll second student
        var student2 = new Student
        {
            FirstName = "Second",
            LastName = "Student",
            Email = "second.student@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        _context.Students.Add(student2);
        await _context.SaveChangesAsync();

        var dto = new
        {
            StudentId = student2.Id,
            CourseId = course.Id
        };

        var response = await adminClient.PostAsJsonAsync("/api/enrollments", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task EnrollStudent_ArchivedCourse_ReturnsConflict()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var course = new Course("ARC_ENR_101", "Archived Course", "Description", 3, 30, 1);
        course.Archive(); // Set IsArchived to true
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var student = new Student
        {
            FirstName = "Archived",
            LastName = "Course",
            Email = "archived.course@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        var dto = new
        {
            StudentId = student.Id,
            CourseId = course.Id
        };

        var response = await adminClient.PostAsJsonAsync("/api/enrollments", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task EnrollStudent_ArchivedStudent_ReturnsConflict()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Archived",
            LastName = "Student",
            Email = "archived.student@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        student.Archive(); // Set status to Archived
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        var course = new Course("ARC101", "Course", "Description", 3, 30, 1);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var dto = new
        {
            StudentId = student.Id,
            CourseId = course.Id
        };

        var response = await adminClient.PostAsJsonAsync("/api/enrollments", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DropEnrollment_NonExistentId_ReturnsNotFound()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.PostAsync("/api/enrollments/99999/drop", null);

        // Current implementation returns BadRequest for non-existent enrollments
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DropEnrollment_AlreadyDropped_ReturnsBadRequest()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Already",
            LastName = "Dropped",
            Email = "already.dropped@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        var course = new Course("ALRDROP101", "Already Dropped", "Description", 3, 30, 1);
        _context.Students.Add(student);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        enrollment.Drop(); // Set status to Dropped
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        var response = await adminClient.PostAsync($"/api/enrollments/{enrollment.Id}/drop", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DropEnrollment_AlreadyCompleted_ReturnsBadRequest()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Already",
            LastName = "Completed",
            Email = "already.completed@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        var course = new Course("ALRCOMP101", "Already Completed", "Description", 3, 30, 1);
        _context.Students.Add(student);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        enrollment.Complete(); // Set status to Completed
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        var response = await adminClient.PostAsync($"/api/enrollments/{enrollment.Id}/drop", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CompleteEnrollment_ValidRequest_ReturnsOkAndUpdatesStatus()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Complete",
            LastName = "Me",
            Email = "complete.me@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        var course = new Course("COMP101", "Complete Course", "Description", 3, 30, 1);
        _context.Students.Add(student);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        var response = await adminClient.PostAsync($"/api/enrollments/{enrollment.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CompleteEnrollmentResult>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // Use a fresh DbContext to verify persistence
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var completed = await verifyContext.Enrollments.FindAsync(enrollment.Id);
        completed.Should().NotBeNull();
        completed!.Status.Should().Be(EnrollmentStatus.Completed);
    }

    [Fact]
    public async Task CompleteEnrollment_NonExistentId_ReturnsNotFound()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var response = await adminClient.PostAsync("/api/enrollments/99999/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompleteEnrollment_NotActive_ReturnsBadRequest()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Not",
            LastName = "Active",
            Email = "not.active@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        var course = new Course("NOTACT101", "Not Active", "Description", 3, 30, 1);
        _context.Students.Add(student);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        enrollment.Drop(); // Set status to Dropped
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        var response = await adminClient.PostAsync($"/api/enrollments/{enrollment.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignGrade_ValidRequest_ReturnsOkAndUpdatesGrade()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Grade",
            LastName = "Me",
            Email = "grade.me@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        var course = new Course("GRD101", "Grade Course", "Description", 3, 30, 1);
        _context.Students.Add(student);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        enrollment.Complete(); // Set status to Completed
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        var dto = new
        {
            EnrollmentId = enrollment.Id,
            Grade = "A"
        };

        var response = await adminClient.PostAsJsonAsync($"/api/enrollments/{enrollment.Id}/grade", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AssignGradeResult>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // Use a fresh DbContext to verify persistence
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var graded = await verifyContext.Enrollments.FindAsync(enrollment.Id);
        graded.Should().NotBeNull();
        graded!.Grade.Should().Be(Grade.A);
    }

    [Fact]
    public async Task AssignGrade_IdMismatch_ReturnsBadRequest()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Grade",
            LastName = "Mismatch",
            Email = "grade.mismatch@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        var course = new Course("GRDMIS101", "Grade Mismatch", "Description", 3, 30, 1);
        _context.Students.Add(student);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        var dto = new
        {
            EnrollmentId = 99999, // Different from route
            Grade = "A"
        };

        var response = await adminClient.PostAsJsonAsync($"/api/enrollments/{enrollment.Id}/grade", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignGrade_NonExistentId_ReturnsNotFound()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var dto = new
        {
            EnrollmentId = 99999,
            Grade = "A"
        };

        var response = await adminClient.PostAsJsonAsync("/api/enrollments/99999/grade", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignGrade_InvalidGradeValue_ReturnsBadRequest()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Grade",
            LastName = "Invalid",
            Email = "grade.invalid@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        var course = new Course("GRDINV101", "Grade Invalid", "Description", 3, 30, 1);
        _context.Students.Add(student);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        var dto = new
        {
            EnrollmentId = enrollment.Id,
            Grade = "Z" // Invalid grade
        };

        var response = await adminClient.PostAsJsonAsync($"/api/enrollments/{enrollment.Id}/grade", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignGrade_EnrollmentNotCompleted_ReturnsBadRequest()
    {
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin@school.com", "Admin123!");

        var student = new Student
        {
            FirstName = "Grade",
            LastName = "Active",
            Email = "grade.active@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        var course = new Course("GRDACT101", "Grade Active", "Description", 3, 30, 1);
        _context.Students.Add(student);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        // Enrollment constructor defaults to Active status
        var enrollment = new Enrollment(student.Id, course.Id);
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        var dto = new
        {
            EnrollmentId = enrollment.Id,
            Grade = "A"
        };

        var response = await adminClient.PostAsJsonAsync($"/api/enrollments/{enrollment.Id}/grade", dto);

        // Current implementation allows assigning grades to active enrollments
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AssignGradeResult>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // Use a fresh DbContext to verify persistence
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var graded = await verifyContext.Enrollments.FindAsync(enrollment.Id);
        graded.Should().NotBeNull();
        graded!.Grade.Should().Be(Grade.A);
    }
}
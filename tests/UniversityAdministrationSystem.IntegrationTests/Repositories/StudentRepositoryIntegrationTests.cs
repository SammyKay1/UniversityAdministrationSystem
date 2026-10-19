using System;
using System.Threading.Tasks;
using FluentAssertions;
using UniversityAdministrationSystem;
using UniversityAdministrationSystem.IntegrationTests.Fixtures;
using Xunit;

namespace UniversityAdministrationSystem.IntegrationTests.Repositories;

[Collection("Database")]
public class StudentRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private IStudentRepository _repository = null!;
    private ApplicationDbContext _context = null!;

    public StudentRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        _context = _fixture.CreateContext();
        _repository = new StudentRepository(_context);
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_ValidStudent_PersistsStudent()
    {
        var student = new Student
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };

        await _repository.AddAsync(student);
        await _context.SaveChangesAsync();

        student.Id.Should().BeGreaterThan(0);
        student.ConcurrencyToken.Should().NotBe(Guid.Empty);

        var retrieved = await _context.Students.FindAsync(student.Id);
        retrieved.Should().NotBeNull();
        retrieved!.FirstName.Should().Be("John");
        retrieved.LastName.Should().Be("Doe");
        retrieved.Email.Should().Be("john.doe@test.com");
        retrieved.CreatedDateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetByIdForUpdateAsync_ExistingStudent_ReturnsStudent()
    {
        var student = new Student
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@test.com",
            DateOfBirth = DateTime.Today.AddYears(-21),
            DepartmentId = 1
        };

        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        var retrieved = await _repository.GetByIdForUpdateAsync(student.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(student.Id);
        retrieved.Email.Should().Be("jane.smith@test.com");
    }

    [Fact]
    public async Task GetByIdForUpdateAsync_NonExistentStudent_ReturnsNull()
    {
        var retrieved = await _repository.GetByIdForUpdateAsync(99999);

        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task SoftDeleteAsync_ExistingStudent_MarksAsDeleted()
    {
        var student = new Student
        {
            FirstName = "Delete",
            LastName = "Me",
            Email = "delete.me@test.com",
            DateOfBirth = DateTime.Today.AddYears(-22),
            DepartmentId = 1
        };

        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        await _repository.SoftDeleteAsync(student);
        await _context.SaveChangesAsync();

        var deleted = await _context.Students.FindAsync(student.Id);
        deleted.Should().NotBeNull();
        // Repository sets IsDeleted = true (from AuditableEntity), not Status
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveEnrollmentsAsync_StudentWithActiveEnrollment_ReturnsTrue()
    {
        var student = new Student
        {
            FirstName = "Enrolled",
            LastName = "Student",
            Email = "enrolled.student@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };

        var courseCode = $"CS101_ENR_{Guid.NewGuid():N}";
        var course = new Course(courseCode, "Intro to CS", "Description", 3, 30, 1);

        await _context.Students.AddAsync(student);
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        await _context.Enrollments.AddAsync(enrollment);
        await _context.SaveChangesAsync();

        var result = await _repository.HasActiveEnrollmentsAsync(student.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveEnrollmentsAsync_StudentWithoutEnrollment_ReturnsFalse()
    {
        var student = new Student
        {
            FirstName = "No",
            LastName = "Enrollment",
            Email = "no.enrollment@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };

        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        var result = await _repository.HasActiveEnrollmentsAsync(student.Id);

        result.Should().BeFalse();
    }
}
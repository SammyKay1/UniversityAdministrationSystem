using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using UniversityAdministrationSystem;
using UniversityAdministrationSystem.IntegrationTests.Fixtures;
using Xunit;

namespace UniversityAdministrationSystem.IntegrationTests.Repositories;

[Collection("Database")]
public class EnrollmentRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private IEnrollmentRepository _repository = null!;
    private ApplicationDbContext _context = null!;

    public EnrollmentRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        _context = _fixture.CreateContext();
        _repository = new EnrollmentRepository(_context);
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_ValidEnrollment_PersistsEnrollment()
    {
        var student = new Student
        {
            FirstName = "Enroll",
            LastName = "Student",
            Email = "enroll.student@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };

        var courseCode = $"ENR101_{Guid.NewGuid():N}";
        var course = new Course(courseCode, "Enrollment Course", "Description", 3, 30, 1);

        await _context.Students.AddAsync(student);
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);

        await _repository.AddAsync(enrollment);
        await _context.SaveChangesAsync();

        enrollment.Id.Should().BeGreaterThan(0);
        enrollment.Status.Should().Be(EnrollmentStatus.Active);
        enrollment.Grade.Should().Be(Grade.None);
        enrollment.EnrollmentDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var retrieved = await _context.Enrollments.FindAsync(enrollment.Id);
        retrieved.Should().NotBeNull();
        retrieved!.StudentId.Should().Be(student.Id);
        retrieved.CourseId.Should().Be(course.Id);
    }

    [Fact]
    public async Task ExistsActiveAsync_ActiveEnrollmentExists_ReturnsTrue()
    {
        var student = new Student
        {
            FirstName = "Exists",
            LastName = "Active",
            Email = "exists.active@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };

        var courseCode = $"EXA101_{Guid.NewGuid():N}";
        var course = new Course(courseCode, "Exists Active", "Description", 3, 30, 1);

        await _context.Students.AddAsync(student);
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        await _context.Enrollments.AddAsync(enrollment);
        await _context.SaveChangesAsync();

        var exists = await _repository.ExistsActiveAsync(student.Id, course.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsActiveAsync_NoActiveEnrollment_ReturnsFalse()
    {
        var student = new Student
        {
            FirstName = "No",
            LastName = "Active",
            Email = "no.active@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };

        var courseCode = $"NOA101_{Guid.NewGuid():N}";
        var course = new Course(courseCode, "No Active", "Description", 3, 30, 1);

        await _context.Students.AddAsync(student);
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var exists = await _repository.ExistsActiveAsync(student.Id, course.Id);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsActiveAsync_DroppedEnrollment_ReturnsFalse()
    {
        var student = new Student
        {
            FirstName = "Dropped",
            LastName = "Enrollment",
            Email = "dropped.enrollment@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };

        var courseCode = $"DRP101_{Guid.NewGuid():N}";
        var course = new Course(courseCode, "Dropped Course", "Description", 3, 30, 1);

        await _context.Students.AddAsync(student);
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        enrollment.Drop();
        await _context.Enrollments.AddAsync(enrollment);
        await _context.SaveChangesAsync();

        var exists = await _repository.ExistsActiveAsync(student.Id, course.Id);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CountActiveByCourseAsync_ReturnsCorrectCount()
    {
        var courseCode = $"CNT101_{Guid.NewGuid():N}";
        var course = new Course(courseCode, "Count Course", "Description", 3, 30, 1);

        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        for (int i = 1; i <= 3; i++)
        {
            var student = new Student
            {
                FirstName = $"Student{i}",
                LastName = "Count",
                Email = $"student{i}.count@test.com",
                DateOfBirth = DateTime.Today.AddYears(-20),
                DepartmentId = 1
            };
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();

            var enrollment = new Enrollment(student.Id, course.Id);
            await _context.Enrollments.AddAsync(enrollment);
        }
        await _context.SaveChangesAsync();

        var count = await _repository.CountActiveByCourseAsync(course.Id);

        count.Should().Be(3);
    }

    [Fact]
    public async Task GetByIdForUpdateAsync_ExistingEnrollment_ReturnsEnrollment()
    {
        var student = new Student
        {
            FirstName = "Get",
            LastName = "Enrollment",
            Email = "get.enrollment@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };

        var course = new Course("GET101", "Get Course", "Description", 3, 30, 1);

        await _context.Students.AddAsync(student);
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, course.Id);
        await _context.Enrollments.AddAsync(enrollment);
        await _context.SaveChangesAsync();

        var retrieved = await _repository.GetByIdForUpdateAsync(enrollment.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(enrollment.Id);
        retrieved.StudentId.Should().Be(student.Id);
        retrieved.CourseId.Should().Be(course.Id);
    }
}
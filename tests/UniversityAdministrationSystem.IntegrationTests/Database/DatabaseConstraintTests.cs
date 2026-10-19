using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UniversityAdministrationSystem;
using UniversityAdministrationSystem.IntegrationTests.Fixtures;
using Xunit;

namespace UniversityAdministrationSystem.IntegrationTests.Database;

[Collection("Database")]
public class DatabaseConstraintTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private ApplicationDbContext _context = null!;

    public DatabaseConstraintTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        _context = _fixture.CreateContext();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Course_CourseCode_UniqueConstraint_Enforced()
    {
        var course1 = new Course("UNIQUE101", "First Course", "Description", 3, 30, 1);
        await _context.Courses.AddAsync(course1);
        await _context.SaveChangesAsync();

        var course2 = new Course("UNIQUE101", "Second Course", "Description", 4, 25, 1);
        await _context.Courses.AddAsync(course2);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () => await _context.SaveChangesAsync());
        exception.InnerException.Should().BeOfType<SqliteException>()
            .Which.SqliteErrorCode.Should().Be(19);
    }

    [Fact]
    public async Task Department_Code_UniqueConstraint_Enforced()
    {
        // Use a unique code that won't conflict with seeded data
        var uniqueCode = $"TEST_DEPT_UNIQUE_{Guid.NewGuid():N}";
        
        var dept1 = new Department
        {
            Name = "Test Department 1",
            Code = uniqueCode,
            Email = "test1@test.com",
            Description = "Test Department 1"
        };
        await _context.Departments.AddAsync(dept1);
        await _context.SaveChangesAsync();

        var dept2 = new Department
        {
            Name = "Test Department 2",
            Code = uniqueCode, // Same code - should fail
            Email = "test2@test.com",
            Description = "Test Department 2"
        };
        await _context.Departments.AddAsync(dept2);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () => await _context.SaveChangesAsync());
        exception.InnerException.Should().BeOfType<SqliteException>()
            .Which.SqliteErrorCode.Should().Be(19);
    }

    [Fact]
    public async Task Student_Email_UniqueConstraint_Enforced()
    {
        var student1 = new Student
        {
            FirstName = "Student",
            LastName = "One",
            Email = "unique@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        await _context.Students.AddAsync(student1);
        await _context.SaveChangesAsync();

        var student2 = new Student
        {
            FirstName = "Student",
            LastName = "Two",
            Email = "unique@test.com",
            DateOfBirth = DateTime.Today.AddYears(-21),
            DepartmentId = 1
        };
        await _context.Students.AddAsync(student2);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () => await _context.SaveChangesAsync());
        exception.InnerException.Should().BeOfType<SqliteException>()
            .Which.SqliteErrorCode.Should().Be(19);
    }

    [Fact]
    public async Task Teacher_Email_UniqueConstraint_Enforced()
    {
        var teacher1 = new Teacher
        {
            FirstName = "Teacher",
            LastName = "One",
            Email = "teacher.unique@test.com",
            DateOfBirth = DateTime.Today.AddYears(-40),
            DepartmentId = 1
        };
        await _context.Teachers.AddAsync(teacher1);
        await _context.SaveChangesAsync();

        var teacher2 = new Teacher
        {
            FirstName = "Teacher",
            LastName = "Two",
            Email = "teacher.unique@test.com",
            DateOfBirth = DateTime.Today.AddYears(-35),
            DepartmentId = 1
        };
        await _context.Teachers.AddAsync(teacher2);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () => await _context.SaveChangesAsync());
        exception.InnerException.Should().BeOfType<SqliteException>()
            .Which.SqliteErrorCode.Should().Be(19);
    }

    // Note: User email uniqueness is enforced at the application level by ASP.NET Core Identity's UserManager,
    // not at the database level. The database has a unique index on NormalizedEmail, but that's managed by Identity.
    // Testing this would require using UserManager.CreateAsync which is an integration test concern.

    [Fact]
    public async Task Enrollment_StudentCourse_UniqueConstraint_Enforced()
    {
        var student = new Student
        {
            FirstName = "Enroll",
            LastName = "Unique",
            Email = "enroll.unique@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };

        var course = new Course("ENR_UNIQUE", "Enrollment Unique", "Description", 3, 30, 1);

        await _context.Students.AddAsync(student);
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var enrollment1 = new Enrollment(student.Id, course.Id);
        await _context.Enrollments.AddAsync(enrollment1);
        await _context.SaveChangesAsync();

        var enrollment2 = new Enrollment(student.Id, course.Id);
        await _context.Enrollments.AddAsync(enrollment2);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () => await _context.SaveChangesAsync());
        exception.InnerException.Should().BeOfType<SqliteException>()
            .Which.SqliteErrorCode.Should().Be(19);
    }

    [Fact]
    public async Task Student_DepartmentId_ForeignKey_Enforced()
    {
        var student = new Student
        {
            FirstName = "FK",
            LastName = "Test",
            Email = "fk.test@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 99999 // Non-existent department
        };

        await _context.Students.AddAsync(student);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () => await _context.SaveChangesAsync());
        exception.InnerException.Should().BeOfType<SqliteException>()
            .Which.SqliteErrorCode.Should().Be(19);
    }

    [Fact]
    public async Task Course_DepartmentId_ForeignKey_Enforced()
    {
        var course = new Course("FK101", "FK Course", "Description", 3, 30, 99999);

        await _context.Courses.AddAsync(course);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () => await _context.SaveChangesAsync());
        exception.InnerException.Should().BeOfType<SqliteException>()
            .Which.SqliteErrorCode.Should().Be(19);
    }

    [Fact]
    public async Task Enrollment_StudentId_ForeignKey_Enforced()
    {
        var course = new Course("ENR_FK", "Enrollment FK", "Description", 3, 30, 1);
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(99999, course.Id);
        await _context.Enrollments.AddAsync(enrollment);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () => await _context.SaveChangesAsync());
        exception.InnerException.Should().BeOfType<SqliteException>()
            .Which.SqliteErrorCode.Should().Be(19);
    }

    [Fact]
    public async Task Enrollment_CourseId_ForeignKey_Enforced()
    {
        var student = new Student
        {
            FirstName = "Enroll",
            LastName = "FK",
            Email = "enroll.fk@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        var enrollment = new Enrollment(student.Id, 99999);
        await _context.Enrollments.AddAsync(enrollment);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () => await _context.SaveChangesAsync());
        exception.InnerException.Should().BeOfType<SqliteException>()
            .Which.SqliteErrorCode.Should().Be(19);
    }

    [Fact]
    public async Task SoftDelete_PersistsArchivedStatus()
    {
        var student = new Student
        {
            FirstName = "Soft",
            LastName = "Delete",
            Email = "soft.delete@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        typeof(Student).GetProperty("Status")?.SetValue(student, StudentStatus.Archived);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Students.FindAsync(student.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(StudentStatus.Archived);
    }

    [Fact]
    public async Task ConcurrencyToken_UpdatesOnModification()
    {
        var student = new Student
        {
            FirstName = "Concurrency",
            LastName = "Test",
            Email = "concurrency.test@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        var originalToken = student.ConcurrencyToken;
        originalToken.Should().NotBe(Guid.Empty);

        student.FirstName = "Updated";
        await _context.SaveChangesAsync();

        var retrieved = await _context.Students.FindAsync(student.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ConcurrencyToken.Should().NotBe(originalToken);
    }

    [Fact]
    public async Task AuditFields_PersistOnCreate()
    {
        var student = new Student
        {
            FirstName = "Audit",
            LastName = "Create",
            Email = "audit.create@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Students.FindAsync(student.Id);
        retrieved.Should().NotBeNull();
        retrieved!.CreatedDateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        retrieved.CreatedBy.Should().Be("System");
        retrieved.ModifiedDateUtc.Should().BeNull();
    }

    [Fact]
    public async Task AuditFields_UpdateOnModify()
    {
        var student = new Student
        {
            FirstName = "Audit",
            LastName = "Update",
            Email = "audit.update@test.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        var createdDate = student.CreatedDateUtc;

        student.FirstName = "Modified";
        await _context.SaveChangesAsync();

        var retrieved = await _context.Students.FindAsync(student.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ModifiedDateUtc.Should().NotBeNull();
        retrieved.ModifiedDateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        retrieved.ModifiedBy.Should().Be("System");
        retrieved.CreatedDateUtc.Should().Be(createdDate);
    }
}
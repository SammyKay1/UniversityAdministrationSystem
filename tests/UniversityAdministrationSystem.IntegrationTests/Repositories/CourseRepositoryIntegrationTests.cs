using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using UniversityAdministrationSystem;
using UniversityAdministrationSystem.IntegrationTests.Fixtures;
using Xunit;

namespace UniversityAdministrationSystem.IntegrationTests.Repositories;

[Collection("Database")]
public class CourseRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private ICourseRepository _repository = null!;
    private ApplicationDbContext _context = null!;

    public CourseRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        _context = _fixture.CreateContext();
        _repository = new CourseRepository(_context);
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_ValidCourse_PersistsCourse()
    {
        var courseCode = $"CS201_{Guid.NewGuid():N}";
        var course = new Course(courseCode, "Data Structures", "Advanced programming", 4, 25, 1);

        await _repository.AddAsync(course);
        await _context.SaveChangesAsync();

        course.Id.Should().BeGreaterThan(0);
        course.IsArchived.Should().BeFalse();

        var retrieved = await _context.Courses.FindAsync(course.Id);
        retrieved.Should().NotBeNull();
        retrieved!.CourseCode.Should().Be(courseCode);
        retrieved.Name.Should().Be("Data Structures");
        retrieved.CreditHours.Should().Be(4);
        retrieved.Capacity.Should().Be(25);
    }

    [Fact]
    public async Task ExistsByCourseCodeAsync_ExistingCode_ReturnsTrue()
    {
        var courseCode = $"CS301_{Guid.NewGuid():N}";
        var course = new Course(courseCode, "Algorithms", "Algorithm design", 3, 30, 1);
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var exists = await _repository.ExistsByCourseCodeAsync(courseCode);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByCourseCodeAsync_NonExistentCode_ReturnsFalse()
    {
        var exists = await _repository.ExistsByCourseCodeAsync("NONEXISTENT");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByCourseCodeAsync_ExcludeId_ReturnsFalseForSameCourse()
    {
        var courseCode = $"CS401_{Guid.NewGuid():N}";
        var course = new Course(courseCode, "Operating Systems", "OS concepts", 3, 30, 1);
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var exists = await _repository.ExistsByCourseCodeAsync(courseCode, course.Id);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdForUpdateAsync_ExistingCourse_ReturnsCourse()
    {
        var course = new Course("CS501", "Networks", "Network fundamentals", 3, 30, 1);
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var retrieved = await _repository.GetByIdForUpdateAsync(course.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(course.Id);
        retrieved.CourseCode.Should().Be("CS501");
    }

    [Fact]
    public async Task GetByIdForUpdateAsync_NonExistentCourse_ReturnsNull()
    {
        var retrieved = await _repository.GetByIdForUpdateAsync(99999);

        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task GetLookupAsync_ReturnsCourseLookupDtos()
    {
        var course1 = new Course("LOOKUP1", "Course One", "Desc", 3, 30, 1);
        var course2 = new Course("LOOKUP2", "Course Two", "Desc", 4, 25, 1);
        await _context.Courses.AddRangeAsync(course1, course2);
        await _context.SaveChangesAsync();

        var lookups = (await _repository.GetLookupAsync()).ToList();

        lookups.Should().HaveCountGreaterOrEqualTo(2);
        lookups.Should().Contain(l => l.CourseCode == "LOOKUP1" && l.CourseName == "Course One");
        lookups.Should().Contain(l => l.CourseCode == "LOOKUP2" && l.CourseName == "Course Two");
    }
}
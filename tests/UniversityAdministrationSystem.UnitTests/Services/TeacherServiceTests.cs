using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Services;

public class TeacherServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly TeacherService _service;

    public TeacherServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new TeacherService(_unitOfWorkMock.Object);
    }

    private Teacher CreateTeacher(int id = 1)
    {
        var teacher = new Teacher
        {
            FirstName = "John",
            LastName = "Smith",
            Email = "john.smith@university.edu",
            DateOfBirth = DateTime.Today.AddYears(-40),
            DepartmentId = 1
        };

        typeof(Teacher).GetProperty("Id")?.SetValue(teacher, id);

        return teacher;
    }

    [Fact]
    public async Task CreateAsync_TrimsWhitespace()
    {
        var dto = new TeacherDto
        {
            FirstName = "  John  ",
            LastName = "  Smith  ",
            Email = "  john.smith@university.edu  ",
            DateOfBirth = DateTime.Today.AddYears(-40),
            DepartmentId = 1
        };

        _unitOfWorkMock.Setup(x => x.Teachers.AddAsync(It.IsAny<Teacher>()))
            .Returns<Teacher>(async t => 
            {
                typeof(Teacher).GetProperty("Id")?.SetValue(t, 1);
                return t;
            });

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        await _service.CreateAsync(dto);

        _unitOfWorkMock.Verify(x => x.Teachers.AddAsync(It.Is<Teacher>(t => 
            t.FirstName == "John" && 
            t.LastName == "Smith" &&
            t.Email == "john.smith@university.edu" &&
            t.DepartmentId == 1)), Times.Once);
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_TeacherNotFound_ThrowsInvalidOperationException()
    {
        var dto = new TeacherDto
        {
            Id = 999,
            FirstName = "Updated",
            LastName = "Teacher",
            Email = "updated@university.edu",
            DateOfBirth = DateTime.Today.AddYears(-40),
            DepartmentId = 1
        };

        _unitOfWorkMock.Setup(x => x.Teachers.GetEntityByIdAsync(999))
            .ReturnsAsync((Teacher?)null);

        var act = async () => await _service.UpdateAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Teacher not found.");
    }

    [Fact]
    public async Task UpdateAsync_ValidUpdate_ReturnsSuccess()
    {
        var teacher = CreateTeacher(1);
        
        var dto = new TeacherDto
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@university.edu",
            DateOfBirth = DateTime.Today.AddYears(-35),
            DepartmentId = 2
        };

        _unitOfWorkMock.Setup(x => x.Teachers.GetEntityByIdAsync(1))
            .ReturnsAsync(teacher);

        _unitOfWorkMock.Setup(x => x.Teachers.UpdateAsync(teacher))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        await _service.UpdateAsync(dto);

        teacher.FirstName.Should().Be("Jane");
        teacher.LastName.Should().Be("Doe");
        teacher.Email.Should().Be("jane.doe@university.edu");
        teacher.DateOfBirth.Should().Be(DateTime.Today.AddYears(-35));
        teacher.DepartmentId.Should().Be(2);

        _unitOfWorkMock.Verify(x => x.Teachers.UpdateAsync(teacher), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_TrimsWhitespace()
    {
        var teacher = CreateTeacher(1);
        
        var dto = new TeacherDto
        {
            Id = 1,
            FirstName = "  Jane  ",
            LastName = "  Doe  ",
            Email = "  jane.doe@university.edu  ",
            DateOfBirth = DateTime.Today.AddYears(-35),
            DepartmentId = 2
        };

        _unitOfWorkMock.Setup(x => x.Teachers.GetEntityByIdAsync(1))
            .ReturnsAsync(teacher);

        _unitOfWorkMock.Setup(x => x.Teachers.UpdateAsync(teacher))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        await _service.UpdateAsync(dto);

        teacher.FirstName.Should().Be("Jane");
        teacher.LastName.Should().Be("Doe");
        teacher.Email.Should().Be("jane.doe@university.edu");
    }

    [Fact]
    public async Task ArchiveAsync_TeacherNotFound_ThrowsInvalidOperationException()
    {
        _unitOfWorkMock.Setup(x => x.Teachers.GetEntityByIdAsync(999))
            .ReturnsAsync((Teacher?)null);

        var act = async () => await _service.ArchiveAsync(999);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Teacher not found.");
    }

    [Fact]
    public async Task ArchiveAsync_ValidTeacher_CallsArchive()
    {
        var teacher = CreateTeacher(1);

        _unitOfWorkMock.Setup(x => x.Teachers.GetEntityByIdAsync(1))
            .ReturnsAsync(teacher);

        _unitOfWorkMock.Setup(x => x.Teachers.ArchiveAsync(1))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        await _service.ArchiveAsync(1);

        _unitOfWorkMock.Verify(x => x.Teachers.ArchiveAsync(1), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Services;

public class DepartmentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new DepartmentService(_unitOfWorkMock.Object);
    }

    private Department CreateDepartment(int id = 1, string code = "CS", string name = "Computer Science")
    {
        var department = new Department
        {
            Name = name,
            Code = code,
            Email = "cs@university.edu",
            Description = "Description"
        };

        typeof(Department).GetProperty("Id")?.SetValue(department, id);

        return department;
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ThrowsInvalidOperationException()
    {
        var dto = new DepartmentDto
        {
            Name = "Computer Science",
            Code = "CS",
            Email = "cs@university.edu",
            Description = "Description"
        };

        _unitOfWorkMock.Setup(x => x.Departments.CodeExistsAsync("CS"))
            .ReturnsAsync(true);

        var act = async () => await _service.CreateAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A department with code 'CS' already exists.");
    }

    [Fact]
    public async Task CreateAsync_NormalizesCodeToUppercase()
    {
        var dto = new DepartmentDto
        {
            Name = "Computer Science",
            Code = "cs",
            Email = "cs@university.edu",
            Description = "Description"
        };

        _unitOfWorkMock.Setup(x => x.Departments.CodeExistsAsync("CS"))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Departments.AddAsync(It.IsAny<Department>()))
            .Returns<Department>(async d => 
            {
                typeof(Department).GetProperty("Id")?.SetValue(d, 1);
                return d;
            });

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        await _service.CreateAsync(dto);

        _unitOfWorkMock.Verify(x => x.Departments.AddAsync(It.Is<Department>(d => 
            d.Code == "CS" && 
            d.Name == "Computer Science" &&
            d.Email == "cs@university.edu" &&
            d.Description == "Description")), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_TrimsWhitespace()
    {
        var dto = new DepartmentDto
        {
            Name = "  Computer Science  ",
            Code = "  CS  ",
            Email = "  cs@university.edu  ",
            Description = "  Description  "
        };

        _unitOfWorkMock.Setup(x => x.Departments.CodeExistsAsync("CS"))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Departments.AddAsync(It.IsAny<Department>()))
            .Returns<Department>(async d => 
            {
                typeof(Department).GetProperty("Id")?.SetValue(d, 1);
                return d;
            });

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        await _service.CreateAsync(dto);

        _unitOfWorkMock.Verify(x => x.Departments.AddAsync(It.Is<Department>(d => 
            d.Code == "CS" && 
            d.Name == "Computer Science" &&
            d.Email == "cs@university.edu" &&
            d.Description == "Description")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DepartmentNotFound_ThrowsInvalidOperationException()
    {
        var dto = new DepartmentDto
        {
            Id = 999,
            Name = "Updated",
            Code = "UPD",
            Email = "updated@university.edu",
            Description = "Description"
        };

        _unitOfWorkMock.Setup(x => x.Departments.GetEntityByIdAsync(999))
            .ReturnsAsync((Department?)null);

        var act = async () => await _service.UpdateAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Department not found.");
    }

    [Fact]
    public async Task UpdateAsync_DuplicateCode_ThrowsInvalidOperationException()
    {
        var existingDepartment = CreateDepartment(1, "CS", "Computer Science");
        
        var dto = new DepartmentDto
        {
            Id = 1,
            Name = "Updated",
            Code = "MATH",
            Email = "updated@university.edu",
            Description = "Description"
        };

        _unitOfWorkMock.Setup(x => x.Departments.GetEntityByIdAsync(1))
            .ReturnsAsync(existingDepartment);

        _unitOfWorkMock.Setup(x => x.Departments.CodeExistsAsync("MATH"))
            .ReturnsAsync(true);

        var act = async () => await _service.UpdateAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A department with code 'MATH' already exists.");
    }

    [Fact]
    public async Task UpdateAsync_SameCode_NoDuplicateCheck()
    {
        var existingDepartment = CreateDepartment(1, "CS", "Computer Science");
        
        var dto = new DepartmentDto
        {
            Id = 1,
            Name = "Updated Computer Science",
            Code = "CS",
            Email = "updated@university.edu",
            Description = "Updated Description"
        };

        _unitOfWorkMock.Setup(x => x.Departments.GetEntityByIdAsync(1))
            .ReturnsAsync(existingDepartment);

        _unitOfWorkMock.Setup(x => x.Departments.CodeExistsAsync("CS"))
            .ReturnsAsync(false); // Should not matter since it's the same code

        _unitOfWorkMock.Setup(x => x.Departments.UpdateAsync(It.IsAny<Department>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        await _service.UpdateAsync(dto);

        existingDepartment.Name.Should().Be("Updated Computer Science");
        existingDepartment.Code.Should().Be("CS");
        existingDepartment.Email.Should().Be("updated@university.edu");
        existingDepartment.Description.Should().Be("Updated Description");

        _unitOfWorkMock.Verify(x => x.Departments.UpdateAsync(existingDepartment), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DifferentCodeCaseInsensitive_ChecksDuplicate()
    {
        var existingDepartment = CreateDepartment(1, "CS", "Computer Science");
        
        var dto = new DepartmentDto
        {
            Id = 1,
            Name = "Updated",
            Code = "cs", // Same code, different case
            Email = "updated@university.edu",
            Description = "Description"
        };

        _unitOfWorkMock.Setup(x => x.Departments.GetEntityByIdAsync(1))
            .ReturnsAsync(existingDepartment);

        _unitOfWorkMock.Setup(x => x.Departments.CodeExistsAsync("CS"))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Departments.UpdateAsync(It.IsAny<Department>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        await _service.UpdateAsync(dto);

        existingDepartment.Code.Should().Be("CS");
    }

    [Fact]
    public async Task ArchiveAsync_DepartmentNotFound_ThrowsInvalidOperationException()
    {
        _unitOfWorkMock.Setup(x => x.Departments.GetEntityByIdAsync(999))
            .ReturnsAsync((Department?)null);

        var act = async () => await _service.ArchiveAsync(999);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Department not found.");
    }

    [Fact]
    public async Task ArchiveAsync_ValidDepartment_CallsArchive()
    {
        var department = CreateDepartment(1, "CS", "Computer Science");

        _unitOfWorkMock.Setup(x => x.Departments.GetEntityByIdAsync(1))
            .ReturnsAsync(department);

        _unitOfWorkMock.Setup(x => x.Departments.ArchiveAsync(1))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        await _service.ArchiveAsync(1);

        _unitOfWorkMock.Verify(x => x.Departments.ArchiveAsync(1), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
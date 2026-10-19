using System.Collections.Generic;
using FluentAssertions;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Domain;

public class GradeHelperTests
{
    [Fact]
    public void GetGrades_ReturnsAllGradesExceptNone()
    {
        // Act
        var grades = GradeHelper.GetGrades().ToList();

        // Assert
        grades.Should().HaveCount(5);
        grades.Should().Contain(Grade.A);
        grades.Should().Contain(Grade.B);
        grades.Should().Contain(Grade.C);
        grades.Should().Contain(Grade.D);
        grades.Should().Contain(Grade.F);
        grades.Should().NotContain(Grade.None);
    }

    [Fact]
    public void GetGrades_ReturnsGradesInCorrectOrder()
    {
        // Act
        var grades = GradeHelper.GetGrades().ToList();

        // Assert
        grades.Should().Equal(new[] { Grade.A, Grade.B, Grade.C, Grade.D, Grade.F });
    }

    [Fact]
    public void GetGrades_ExcludesNone()
    {
        // Act
        var grades = GradeHelper.GetGrades().ToList();

        // Assert
        grades.Should().NotContain(Grade.None);
    }

    [Fact]
    public void GetGrades_ReturnsEnumerable()
    {
        // Act
        var result = GradeHelper.GetGrades();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IEnumerable<Grade>>();
    }
}
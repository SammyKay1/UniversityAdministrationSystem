using FluentAssertions;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Services;

public class AcademicStandingServiceTests
{
    private readonly AcademicStandingService _service = new();

    [Fact]
    public void Determine_GPA_3_5_ReturnsDeanList()
    {
        var result = _service.Determine(3.5m);
        result.Should().Be(AcademicStanding.DeanList);
    }

    [Fact]
    public void Determine_GPA_3_6_ReturnsDeanList()
    {
        var result = _service.Determine(3.6m);
        result.Should().Be(AcademicStanding.DeanList);
    }

    [Fact]
    public void Determine_GPA_4_0_ReturnsDeanList()
    {
        var result = _service.Determine(4.0m);
        result.Should().Be(AcademicStanding.DeanList);
    }

    [Fact]
    public void Determine_GPA_2_0_ReturnsGoodStanding()
    {
        var result = _service.Determine(2.0m);
        result.Should().Be(AcademicStanding.GoodStanding);
    }

    [Fact]
    public void Determine_GPA_3_4_ReturnsGoodStanding()
    {
        var result = _service.Determine(3.4m);
        result.Should().Be(AcademicStanding.GoodStanding);
    }

    [Fact]
    public void Determine_GPA_2_5_ReturnsGoodStanding()
    {
        var result = _service.Determine(2.5m);
        result.Should().Be(AcademicStanding.GoodStanding);
    }

    [Fact]
    public void Determine_GPA_1_0_ReturnsProbation()
    {
        var result = _service.Determine(1.0m);
        result.Should().Be(AcademicStanding.Probation);
    }

    [Fact]
    public void Determine_GPA_1_5_ReturnsProbation()
    {
        var result = _service.Determine(1.5m);
        result.Should().Be(AcademicStanding.Probation);
    }

    [Fact]
    public void Determine_GPA_1_9_ReturnsProbation()
    {
        var result = _service.Determine(1.9m);
        result.Should().Be(AcademicStanding.Probation);
    }

    [Fact]
    public void Determine_GPA_0_9_ReturnsSuspension()
    {
        var result = _service.Determine(0.9m);
        result.Should().Be(AcademicStanding.Suspension);
    }

    [Fact]
    public void Determine_GPA_0_ReturnsSuspension()
    {
        var result = _service.Determine(0.0m);
        result.Should().Be(AcademicStanding.Suspension);
    }

    [Fact]
    public void Determine_GPA_Negative_ReturnsSuspension()
    {
        var result = _service.Determine(-1.0m);
        result.Should().Be(AcademicStanding.Suspension);
    }

    [Fact]
    public void Determine_Boundary_Exactly_3_5_ReturnsDeanList()
    {
        var result = _service.Determine(3.5m);
        result.Should().Be(AcademicStanding.DeanList);
    }

    [Fact]
    public void Determine_Boundary_Exactly_2_0_ReturnsGoodStanding()
    {
        var result = _service.Determine(2.0m);
        result.Should().Be(AcademicStanding.GoodStanding);
    }

    [Fact]
    public void Determine_Boundary_Exactly_1_0_ReturnsProbation()
    {
        var result = _service.Determine(1.0m);
        result.Should().Be(AcademicStanding.Probation);
    }
}
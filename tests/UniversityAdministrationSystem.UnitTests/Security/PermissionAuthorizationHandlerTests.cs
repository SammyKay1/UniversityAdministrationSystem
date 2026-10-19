using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Security;

public class PermissionAuthorizationHandlerTests
{
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly PermissionAuthorizationHandler _handler;

    public PermissionAuthorizationHandlerTests()
    {
        _permissionServiceMock = new Mock<IPermissionService>();
        _handler = new PermissionAuthorizationHandler(_permissionServiceMock.Object);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserNotAuthenticated_DoesNotSucceed()
    {
        // Arrange
        var requirement = new PermissionRequirement("students.read");
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // Not authenticated
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        _permissionServiceMock.Verify(x => x.HasPermissionAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasPermission_Succeeds()
    {
        // Arrange
        var requirement = new PermissionRequirement("students.read");
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "test")], "TestAuth"));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        _permissionServiceMock.Setup(x => x.HasPermissionAsync("students.read"))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserLacksPermission_DoesNotSucceed()
    {
        // Arrange
        var requirement = new PermissionRequirement("students.delete");
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "test")], "TestAuth"));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        _permissionServiceMock.Setup(x => x.HasPermissionAsync("students.delete"))
            .ReturnsAsync(false);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
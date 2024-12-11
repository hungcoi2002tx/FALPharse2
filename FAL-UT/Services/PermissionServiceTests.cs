using System.Security.Claims;
using Amazon.DynamoDBv2.DataModel;
using FAL.Services;
using Moq;
using Share.Model;
using Xunit;

public class PermissionServiceTests
{
    private readonly Mock<IDynamoDBContext> _mockDbContext;
    private readonly PermissionService _permissionService;

    public PermissionServiceTests()
    {
        _mockDbContext = new Mock<IDynamoDBContext>();
        _permissionService = new PermissionService(_mockDbContext.Object);
    }

    [Fact]
    public void HasPermission_ResourceIsNull_ReturnsFalse()
    {
        // Arrange
        var user = new ClaimsPrincipal();

        // Act
        var result = _permissionService.HasPermission(user, null, "SomeAction");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasPermission_ActionIsNull_ReturnsFalse()
    {
        // Arrange
        var user = new ClaimsPrincipal();

        // Act
        var result = _permissionService.HasPermission(user, "SomeResource", null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasPermission_InvalidRoleIdClaimValue_ReturnsFalse()
    {
        // Arrange
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "invalid_int")
        });
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = _permissionService.HasPermission(user, "SomeResource", "SomeAction");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasPermission_RoleNotFound_ReturnsFalse()
    {
        // Arrange
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "1")
        });
        var user = new ClaimsPrincipal(identity);

        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role)null);

        // Act
        var result = _permissionService.HasPermission(user, "SomeResource", "SomeAction");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasPermission_RoleDoesNotHavePermission_ReturnsFalse()
    {
        // Arrange
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "1")
        });
        var user = new ClaimsPrincipal(identity);

        var role = new Role
        {
            RoleId = 1,
            RoleName = "TestRole",
            Permissions = new List<Permission>
            {
                new Permission
                {
                    Resource = "SomeResource",
                    Actions = new List<string> { "OtherAction" }
                }
            }
        };

        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // Act
        var result = _permissionService.HasPermission(user, "SomeResource", "SomeAction");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasPermission_RoleHasPermission_ReturnsTrue()
    {
        // Arrange
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "1")
        });
        var user = new ClaimsPrincipal(identity);

        var role = new Role
        {
            RoleId = 1,
            RoleName = "TestRole",
            Permissions = new List<Permission>
            {
                new Permission
                {
                    Resource = "SomeResource",
                    Actions = new List<string> { "SomeAction", "OtherAction" }
                }
            }
        };

        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // Act
        var result = _permissionService.HasPermission(user, "SomeResource", "SomeAction");

        // Assert
        Assert.True(result);
    }
}

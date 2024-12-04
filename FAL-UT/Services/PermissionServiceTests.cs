using System.Security.Claims;
using Amazon.DynamoDBv2.DataModel;
using FAL.Services;
using Moq;
using Share.Model;

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
    public async Task HasPermissionAsync_ResourceIsNull_ReturnsFalse()
    {
        // Arrange
        var user = new ClaimsPrincipal();

        // Act
        var result =  _permissionService.HasPermission(user, null, "SomeAction");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_ActionIsNull_ReturnsFalse()
    {
        // Arrange
        var user = new ClaimsPrincipal();

        // Act
        var result =  _permissionService.HasPermission(user, "SomeResource", null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_RoleIdClaimMissing_ReturnsFalse()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        // Act
        var result =  _permissionService.HasPermission(user, "SomeResource", "SomeAction");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_InvalidRoleIdClaimValue_ReturnsFalse()
    {
        // Arrange
        var identity = new ClaimsIdentity(new Claim[]
        {
        new Claim("RoleId", "invalid_int")
        });
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = _permissionService.HasPermission(user, "SomeResource", "SomeAction");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_RoleNotFound_ReturnsFalse()
    {
        // Arrange
        var identity = new ClaimsIdentity(new Claim[]
        {
            new Claim("RoleId", "1")
        });
        var user = new ClaimsPrincipal(identity);

        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role)null);

        // Act
        var result =  _permissionService.HasPermission(user, "SomeResource", "SomeAction");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_RoleDoesNotHavePermission_ReturnsFalse()
    {
        // Arrange
        var identity = new ClaimsIdentity(new Claim[]
        {
            new Claim("RoleId", "1")
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
        var result =  _permissionService.HasPermission(user, "SomeResource", "SomeAction");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_RoleHasPermission_ReturnsTrue()
    {
        // Arrange
        var identity = new ClaimsIdentity(new Claim[]
        {
            new Claim("RoleId", "1")
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
        var result =  _permissionService.HasPermission(user, "SomeResource", "SomeAction");

        // Assert
        Assert.True(result);
    }
}

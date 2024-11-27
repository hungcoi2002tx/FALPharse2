using Amazon.DynamoDBv2.DataModel;
using FAL.Controllers;
using FAL.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class RolesControllerTests
{
    private readonly Mock<IDynamoDBContext> _mockDbContext;
    private readonly RolesController _controller;

    public RolesControllerTests()
    {
        _mockDbContext = new Mock<IDynamoDBContext>();
        _controller = new RolesController(_mockDbContext.Object);
    }

    [Fact]
    public async Task GetRoles_ReturnsOkResult_WithRoles()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role
            {
                RoleId = 1,
                RoleName = "Admin",
                Permissions = new List<Permission>
                {
                    new Permission { Resource = "Dashboard", Actions = new List<string> { "Read", "Write" } },
                    new Permission { Resource = "Settings", Actions = new List<string> { "Read" } }
                }
            },
            new Role
            {
                RoleId = 2,
                RoleName = "User",
                Permissions = new List<Permission>
                {
                    new Permission { Resource = "Profile", Actions = new List<string> { "Read" } }
                }
            }
        };

        _mockDbContext
    .Setup(ctx => ctx.ScanAsync<Role>(It.IsAny<List<ScanCondition>>(), null)) // Explicitly specify null for the optional argument
    .Returns(MockScanAsync(roles));


        // Act
        var result = await _controller.GetRoles();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(roles, okResult.Value);
    }

    [Fact]
    public async Task GetRoleById_ValidId_ReturnsOkResult_WithRole()
    {
        // Arrange
        var role = new Role
        {
            RoleId = 1,
            RoleName = "Admin",
            Permissions = new List<Permission>
            {
                new Permission { Resource = "Dashboard", Actions = new List<string> { "Read", "Write" } }
            }
        };

        _mockDbContext
            .Setup(ctx => ctx.LoadAsync<Role>(1, default))
            .ReturnsAsync(role);

        // Act
        var result = await _controller.GetRoleById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(role, okResult.Value);
    }

    [Fact]
    public async Task GetRoleById_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        _mockDbContext
            .Setup(ctx => ctx.LoadAsync<Role>(2, default))
            .ReturnsAsync((Role)null);

        // Act
        var result = await _controller.GetRoleById(2);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Role không tìm thấy!", notFoundResult.Value);
    }

    [Fact]
    public async Task CreateRole_ValidRole_ReturnsOkResult()
    {
        // Arrange
        var role = new Role
        {
            RoleId = 1,
            RoleName = "Admin",
            Permissions = new List<Permission>
            {
                new Permission { Resource = "Dashboard", Actions = new List<string> { "Read", "Write" } }
            }
        };

        _mockDbContext
            .Setup(ctx => ctx.LoadAsync<Role>(1, default))
            .ReturnsAsync((Role)null);

        _mockDbContext
            .Setup(ctx => ctx.SaveAsync(role, default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateRole(role);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(role, okResult.Value);
    }

    [Fact]
    public async Task CreateRole_DuplicateRole_ReturnsBadRequestResult()
    {
        // Arrange
        var role = new Role
        {
            RoleId = 1,
            RoleName = "Admin",
            Permissions = new List<Permission>
            {
                new Permission { Resource = "Dashboard", Actions = new List<string> { "Read", "Write" } }
            }
        };

        _mockDbContext
            .Setup(ctx => ctx.LoadAsync<Role>(1, default))
            .ReturnsAsync(role);

        // Act
        var result = await _controller.CreateRole(role);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Role đã tồn tại!", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateRole_ValidRole_ReturnsOkResult()
    {
        // Arrange
        var existingRole = new Role
        {
            RoleId = 1,
            RoleName = "Admin",
            Permissions = new List<Permission>
            {
                new Permission { Resource = "Dashboard", Actions = new List<string> { "Read" } }
            }
        };

        var updatedRole = new Role
        {
            RoleId = 1,
            RoleName = "Admin",
            Permissions = new List<Permission>
            {
                new Permission { Resource = "Dashboard", Actions = new List<string> { "Read", "Write" } }
            }
        };

        _mockDbContext
            .Setup(ctx => ctx.LoadAsync<Role>(1, default))
            .ReturnsAsync(existingRole);

        _mockDbContext
            .Setup(ctx => ctx.SaveAsync(It.IsAny<Role>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateRole(1, updatedRole);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updatedRole.Permissions, ((Role)okResult.Value).Permissions);
    }

    [Fact]
    public async Task UpdateRole_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        var updatedRole = new Role
        {
            RoleId = 1,
            RoleName = "Admin",
            Permissions = new List<Permission>
            {
                new Permission { Resource = "Dashboard", Actions = new List<string> { "Read", "Write" } }
            }
        };

        _mockDbContext
            .Setup(ctx => ctx.LoadAsync<Role>(1, default))
            .ReturnsAsync((Role)null);

        // Act
        var result = await _controller.UpdateRole(1, updatedRole);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Role không tìm thấy để update!", notFoundResult.Value);
    }

    [Fact]
    public async Task DeleteRole_ValidId_ReturnsOkResult()
    {
        // Arrange
        var role = new Role
        {
            RoleId = 1,
            RoleName = "Admin",
            Permissions = new List<Permission>
            {
                new Permission { Resource = "Dashboard", Actions = new List<string> { "Read", "Write" } }
            }
        };

        _mockDbContext
            .Setup(ctx => ctx.LoadAsync<Role>(1, default))
            .ReturnsAsync(role);

        _mockDbContext
            .Setup(ctx => ctx.DeleteAsync(role, default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteRole(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Đã xóa role!", okResult.Value);
    }

    [Fact]
    public async Task DeleteRole_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        _mockDbContext
            .Setup(ctx => ctx.LoadAsync<Role>(1, default))
            .ReturnsAsync((Role)null);

        // Act
        var result = await _controller.DeleteRole(1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Role không tìm thấy để xóa!", notFoundResult.Value);
    }

    private AsyncSearch<T> MockScanAsync<T>(IEnumerable<T> results)
    {
        var mockSearch = new Mock<AsyncSearch<T>>();
        mockSearch
            .Setup(search => search.GetRemainingAsync(default))
            .ReturnsAsync(new List<T>(results));
        return mockSearch.Object;
    }
}

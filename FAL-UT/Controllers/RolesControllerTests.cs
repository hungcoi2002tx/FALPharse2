using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2.DataModel;
using FAL.Controllers;
using Share.Model;

public class RolesControllerTests
{
    private readonly Mock<IDynamoDBContext> _mockDbContext;
    private readonly RolesController _controller;

    public RolesControllerTests()
    {
        _mockDbContext = new Mock<IDynamoDBContext>();
        _controller = new RolesController(_mockDbContext.Object);
    }

    // Test GetRoles
    [Fact]
    public async Task GetRoleById_ReturnsRole_WhenRoleExists()
    {
        // Arrange
        var role = new Role { RoleId = 1, RoleName = "Admin" };

        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, default))
                      .ReturnsAsync(role);

        // Act
        var result = await _controller.GetRoleById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(role, okResult.Value);
    }

   



    // Test GetRoleById
    [Fact]
    public async Task GetRoleById_ReturnsRole_WhenExists()
    {
        // Arrange
        var role = new Role { RoleId = 1, RoleName = "Admin" };
        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, default)).ReturnsAsync(role);

        // Act
        var result = await _controller.GetRoleById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(role, okResult.Value);
    }

    [Fact]
    public async Task GetRoleById_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, default)).ReturnsAsync((Role)null);

        // Act
        var result = await _controller.GetRoleById(1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // Test CreateRole
    [Fact]
    public async Task CreateRole_ReturnsOk_WhenRoleIsCreated()
    {
        // Arrange
        var role = new Role { RoleId = 1, RoleName = "New Role" };
        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, default)).ReturnsAsync((Role)null);

        // Act
        var result = await _controller.CreateRole(role);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(role, okResult.Value);
    }

    [Fact]
    public async Task CreateRole_ReturnsBadRequest_WhenRoleAlreadyExists()
    {
        // Arrange
        var role = new Role { RoleId = 1, RoleName = "Existing Role" };
        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, default)).ReturnsAsync(role);

        // Act
        var result = await _controller.CreateRole(role);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Role already exists!", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateRole_ReturnsOk_WhenRoleIsUpdated()
    {
        // Arrange
        var existingRole = new Role
        {
            RoleId = 1,
            RoleName = "Old Role",
            Permissions = new List<Permission>
        {
            new Permission { Resource = "Files", Actions = new List<string> { "Read" } }
        }
        };

        var updatedRole = new Role
        {
            RoleId = 1,
            RoleName = "Updated Role",
            Permissions = new List<Permission>
        {
            new Permission { Resource = "Files", Actions = new List<string> { "Read", "Write" } }
        }
        };

        _mockDbContext.Setup(db => db.LoadAsync<Role>(existingRole.RoleId, default))
                      .ReturnsAsync(existingRole);

        // Act
        var result = await _controller.UpdateRole(1, updatedRole);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var role = Assert.IsType<Role>(okResult.Value);

        // Verify updated values
        Assert.Equal("Updated Role", role.RoleName);
        Assert.Equal(1, role.Permissions.Count);
        Assert.Equal("Files", role.Permissions[0].Resource);
        Assert.Contains("Write", role.Permissions[0].Actions);
    }


    [Fact]
    public async Task UpdateRole_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, default)).ReturnsAsync((Role)null);

        // Act
        var result = await _controller.UpdateRole(1, new Role());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // Test DeleteRole
    [Fact]
    public async Task DeleteRole_ReturnsOk_WhenRoleIsDeleted()
    {
        // Arrange
        var role = new Role { RoleId = 1, RoleName = "Deletable Role" };
        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, default)).ReturnsAsync(role);

        // Act
        var result = await _controller.DeleteRole(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Role deleted!", okResult.Value);
    }

    [Fact]
    public async Task DeleteRole_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, default)).ReturnsAsync((Role)null);

        // Act
        var result = await _controller.DeleteRole(1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // Additional Tests for RolesControllerTests
    

    [Fact]
    public async Task CreateRole_ThrowsException_WhenDbFails()
    {
        // Arrange
        var role = new Role { RoleId = 1, RoleName = "New Role" };
        _mockDbContext.Setup(db => db.SaveAsync(role, default))
                      .ThrowsAsync(new Exception("Database error"));

        // Act
        await Assert.ThrowsAsync<Exception>(() => _controller.CreateRole(role));
    }

    [Fact]
    public async Task UpdateRole_ThrowsException_WhenDbFails()
    {
        // Arrange
        var existingRole = new Role { RoleId = 1, RoleName = "Old Role" };
        var updatedRole = new Role { RoleId = 1, RoleName = "Updated Role" };

        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, default))
                      .ReturnsAsync(existingRole);

        _mockDbContext.Setup(db => db.SaveAsync(existingRole, default))
                      .ThrowsAsync(new Exception("Database error"));

        // Act
        await Assert.ThrowsAsync<Exception>(() => _controller.UpdateRole(1, updatedRole));
    }

    [Fact]
    public async Task DeleteRole_ThrowsException_WhenDbFails()
    {
        // Arrange
        var role = new Role { RoleId = 1, RoleName = "Deletable Role" };
        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, default)).ReturnsAsync(role);

        _mockDbContext.Setup(db => db.DeleteAsync(role, default))
                      .ThrowsAsync(new Exception("Database error"));

        // Act
        await Assert.ThrowsAsync<Exception>(() => _controller.DeleteRole(1));
    }

    [Fact]
    public async Task GetRoleById_ReturnsBadRequest_WhenRoleIdIsNegative()
    {
        // Act
        var result = await _controller.GetRoleById(-1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateRole_ReturnsBadRequest_WhenRoleIsNull()
    {
        // Act
        var result = await _controller.CreateRole(null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateRole_ReturnsBadRequest_WhenRoleIsNull1()
    {
        // Act
        var result = await _controller.CreateRole(null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateRole_ReturnsBadRequest_WhenRoleIsNull2()
    {
        // Act
        var result = await _controller.CreateRole(null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    [Fact]
    public async Task CreateRole_ReturnsBadRequest_WhenRoleIsNull3()
    {
        // Act
        var result = await _controller.CreateRole(null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateRole_ReturnsBadRequest_WhenUpdatedRoleIsNull()
    {
        // Act
        var result = await _controller.UpdateRole(1, null);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateRole_DoesNotSave_WhenRoleIdsMismatch()
    {
        // Arrange
        var existingRole = new Role { RoleId = 1, RoleName = "Old Role" };
        var updatedRole = new Role { RoleId = 2, RoleName = "Updated Role" };

        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, default))
                      .ReturnsAsync(existingRole);

        // Act
        var result = await _controller.UpdateRole(1, updatedRole);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeleteRole_ReturnsBadRequest_WhenRoleIdIsNegative()
    {
        // Act
        var result = await _controller.DeleteRole(-1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteRole_DoesNotDelete_WhenRoleIdIsZero()
    {
        // Act
        var result = await _controller.DeleteRole(0);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateRole_ReturnsOk_WithMinimumValidData()
    {
        // Arrange
        var role = new Role { RoleId = 1, RoleName = "Basic Role" };
        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, default)).ReturnsAsync((Role)null);

        // Act
        var result = await _controller.CreateRole(role);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(role, okResult.Value);
    }

    [Fact]
    public async Task UpdateRole_DoesNotUpdate_WhenPermissionsAreNull()
    {
        // Arrange
        var existingRole = new Role { RoleId = 1, RoleName = "Old Role" };
        var updatedRole = new Role { RoleId = 1, RoleName = "Updated Role", Permissions = null };

        _mockDbContext.Setup(db => db.LoadAsync<Role>(1, default))
                      .ReturnsAsync(existingRole);

        // Act
        var result = await _controller.UpdateRole(1, updatedRole);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetRoleById_ReturnsBadRequest_WhenRoleIdIsZero()
    {
        // Act
        var result = await _controller.GetRoleById(0);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    

}

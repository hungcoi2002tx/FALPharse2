using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using FAL.Controllers;
using FAL.Models;

public class AccountsControllerTests
{
    private readonly Mock<IDynamoDBContext> _mockDbContext;
    private readonly AccountsController _controller;

    public AccountsControllerTests()
    {
        _mockDbContext = new Mock<IDynamoDBContext>();
        _controller = new AccountsController(_mockDbContext.Object);
    }

    [Fact]
    public async Task GetUsers_ReturnsOkResultWithUsers()
    {
        // Arrange
        var mockDbContext = new Mock<IDynamoDBContext>();

        // Create a sample list of users
        var mockUsers = new List<Account>
    {
        new Account
        {
            Username = "user1",
            Email = "user1@example.com",
            RoleId = 1,
            SystemName = "SystemA",
            WebhookUrl = "https://webhook.example.com",
            WebhookSecretKey = "secret1",
            Status = "Active"
        },
        new Account
        {
            Username = "user2",
            Email = "user2@example.com",
            RoleId = 2,
            SystemName = "SystemB",
            WebhookUrl = "https://webhook2.example.com",
            WebhookSecretKey = "secret2",
            Status = "Inactive"
        }
    };

        // Mock the behavior of ScanAsync
        var mockAsyncSearch = new Mock<AsyncSearch<Account>>();
        mockAsyncSearch
            .Setup(m => m.GetRemainingAsync(default))
            .ReturnsAsync(mockUsers);

        mockDbContext
            .Setup(m => m.ScanAsync<Account>(It.IsAny<IEnumerable<ScanCondition>>(), default))
            .Returns(mockAsyncSearch.Object);

        // Inject the mocked IDynamoDBContext
        var controller = new AccountsController(mockDbContext.Object);

        // Act
        var result = await controller.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUsers = Assert.IsType<List<Account>>(okResult.Value);

        Assert.Equal(2, returnedUsers.Count);
        Assert.Equal("user1", returnedUsers[0].Username);
        Assert.Equal("user2", returnedUsers[1].Username);

        // Verify that ScanAsync was called once
        mockDbContext.Verify(m => m.ScanAsync<Account>(It.IsAny<IEnumerable<ScanCondition>>(), default), Times.Once);
    }


    // Test for GetUserById - Normal Case
    [Fact]
    public async Task GetUserById_ReturnsOkWithUser()
    {
        // Arrange
        var mockUser = new Account { Username = "user1", Email = "user1@example.com" };
        _mockDbContext.Setup(db => db.LoadAsync<Account>("user1", default)).ReturnsAsync(mockUser);

        // Act
        var result = await _controller.GetUserById("user1");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUser = Assert.IsType<Account>(okResult.Value);
        Assert.Equal("user1", returnedUser.Username);
    }

    // Test for GetUserById - User Not Found
    [Fact]
    public async Task GetUserById_ReturnsNotFound()
    {
        // Arrange
        _mockDbContext.Setup(db => db.LoadAsync<Account>("nonexistent", default)).ReturnsAsync((Account)null);

        // Act
        var result = await _controller.GetUserById("nonexistent");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // Test for CreateUser - Normal Case
    [Fact]
    public async Task CreateUser_ReturnsOkWhenCreated()
    {
        // Arrange
        var newUser = new Account { Username = "user1", Email = "user1@example.com" };
        _mockDbContext.Setup(db => db.LoadAsync<Account>("user1", default)).ReturnsAsync((Account)null);

        // Act
        var result = await _controller.CreateUser(newUser);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var createdUser = Assert.IsType<Account>(okResult.Value);
        Assert.Equal("user1", createdUser.Username);

        _mockDbContext.Verify(db => db.SaveAsync(newUser, default), Times.Once);
    }

    // Test for CreateUser - User Already Exists
    [Fact]
    public async Task CreateUser_ReturnsBadRequestWhenUserExists()
    {
        // Arrange
        var existingUser = new Account { Username = "user1", Email = "user1@example.com" };
        _mockDbContext.Setup(db => db.LoadAsync<Account>("user1", default)).ReturnsAsync(existingUser);

        // Act
        var result = await _controller.CreateUser(existingUser);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // Test for UpdateUser - Normal Case
    [Fact]
    public async Task UpdateUser_ReturnsOkWhenUpdated()
    {
        // Arrange
        var existingUser = new Account { Username = "user1", Password = BCrypt.Net.BCrypt.HashPassword("oldpass") };
        var updatedUser = new Account { Username = "user1", Password = "newpass", Email = "user1@example.com" };

        _mockDbContext.Setup(db => db.LoadAsync<Account>("user1", default)).ReturnsAsync(existingUser);

        // Act
        var result = await _controller.UpdateUser("user1", updatedUser);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUser = Assert.IsType<Account>(okResult.Value);

        Assert.True(BCrypt.Net.BCrypt.Verify("newpass", returnedUser.Password));
        _mockDbContext.Verify(db => db.SaveAsync(existingUser, default), Times.Once);
    }

    // Test for UpdateUser - User Not Found
    [Fact]
    public async Task UpdateUser_ReturnsNotFound()
    {
        // Arrange
        _mockDbContext.Setup(db => db.LoadAsync<Account>("nonexistent", default)).ReturnsAsync((Account)null);

        // Act
        var result = await _controller.UpdateUser("nonexistent", new Account());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // Test for DeleteUser - Normal Case
    [Fact]
    public async Task DeleteUser_ReturnsOkWhenDeleted()
    {
        // Arrange
        var existingUser = new Account { Username = "user1" };
        _mockDbContext.Setup(db => db.LoadAsync<Account>("user1", default)).ReturnsAsync(existingUser);

        // Act
        var result = await _controller.DeleteUser("user1");

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _mockDbContext.Verify(db => db.DeleteAsync(existingUser, default), Times.Once);
    }

    // Test for DeleteUser - User Not Found
    [Fact]
    public async Task DeleteUser_ReturnsNotFound()
    {
        // Arrange
        _mockDbContext.Setup(db => db.LoadAsync<Account>("nonexistent", default)).ReturnsAsync((Account)null);

        // Act
        var result = await _controller.DeleteUser("nonexistent");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}

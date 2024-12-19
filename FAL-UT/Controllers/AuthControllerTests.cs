using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Amazon.DynamoDBv2.DataModel;
using FAL.Controllers;
using FAL.Dtos;
using Share.Model;
using FAL.Services.IServices;
using FAL.Utils;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

public class AuthControllerTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IDynamoDBContext> _mockDbContext;
    private readonly Mock<ICollectionService> _mockCollectionService;
    private readonly AuthController _authController;

    public AuthControllerTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockDbContext = new Mock<IDynamoDBContext>();
        _mockCollectionService = new Mock<ICollectionService>();

        _mockConfiguration.Setup(config => config["Jwt:Key"]).Returns("this_is_a_very_long_secret_key_at_least_32_chars!");
        _mockConfiguration.Setup(config => config["Jwt:Issuer"]).Returns("test_issuer");
        _mockConfiguration.Setup(config => config["Jwt:Audience"]).Returns("test_audience");
        _mockConfiguration.Setup(config => config["Jwt:ExpiryMinutes"]).Returns("30");

        _authController = new AuthController(
            _mockConfiguration.Object,
            _mockDbContext.Object,
            _mockCollectionService.Object);
    }

    // --- Login Tests ---
    // Define a DTO class matching the response structure
    private class LoginResponse
    {
        public string Message { get; set; }
        public bool Status { get; set; }
    }

    





    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUsernameDoesNotExist()
    {
        var loginModel = new LoginModel { Username = "nonexistent", Password = "Password123!" };

        _mockDbContext.Setup(db => db.LoadAsync<Account>(loginModel.Username, default))
                      .ReturnsAsync((Account)null);

        var result = await _authController.Login(loginModel);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Contains("Username does not exist.", unauthorized.Value.ToString());
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsIncorrect()
    {
        var loginModel = new LoginModel { Username = "user1", Password = "WrongPassword" };
        var user = new Account { Username = "user1", Password = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"), Status = "Active" };

        _mockDbContext.Setup(db => db.LoadAsync<Account>(loginModel.Username, default))
                      .ReturnsAsync(user);

        var result = await _authController.Login(loginModel);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Contains("Incorrect password.", unauthorized.Value.ToString());
    }

   

    // --- JWT Token Tests ---

    [Fact]
    public void JwtTokenGenerator_ThrowsException_WhenKeyIsMissing()
    {
        var invalidConfig = new Mock<IConfiguration>();
        invalidConfig.Setup(config => config["Jwt:Key"]).Returns((string)null);

        var jwtGenerator = new JwtTokenGenerator(invalidConfig.Object);

        Assert.Throws<InvalidOperationException>(() =>
            jwtGenerator.GenerateJwtToken("user1", "1", "SystemA"));
    }

    // --- Registration Tests ---

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenDtoIsNull()
    {
        AccountRegisterDTO nullDto = null;

        var result = await _authController.Register(nullDto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid registration information.", badRequest.Value);
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenUserAlreadyExists()
    {
        var existingUser = new Account { Username = "existinguser" };
        _mockDbContext.Setup(db => db.LoadAsync<Account>("existinguser", default)).ReturnsAsync(existingUser);

        var registerDto = new AccountRegisterDTO
        {
            Username = "existinguser",
            Password = "password123",
            Email = "user@example.com",
            SystemName = "SystemA",
            WebhookUrl = "https://webhook.example.com",
            WebhookSecretKey = "secret"
        };

        var result = await _authController.Register(registerDto);

        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("Username already exists.", conflictResult.Value);
    }

    
}

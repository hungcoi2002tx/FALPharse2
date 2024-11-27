using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Amazon.DynamoDBv2.DataModel;
using FAL.Controllers;
using FAL.Dtos;
using FAL.Models;
using FAL.Utils;
using System.Text.Json;

public class AuthControllerTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IDynamoDBContext> _mockDbContext;
    private readonly AuthController _authController;

    public AuthControllerTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockDbContext = new Mock<IDynamoDBContext>();

        // Setup configuration values for JwtTokenGenerator
        _mockConfiguration.Setup(config => config["Jwt:Key"]).Returns("test_secret_key_1234567890_32_chars");
        // 16 characters
        _mockConfiguration.Setup(config => config["Jwt:Issuer"]).Returns("test_issuer");
        _mockConfiguration.Setup(config => config["Jwt:Audience"]).Returns("test_audience");
        _mockConfiguration.Setup(config => config["Jwt:ExpiryMinutes"]).Returns("30");

        // Inject mocks into AuthController
        _authController = new AuthController(_mockConfiguration.Object, _mockDbContext.Object);
    }

    [Fact]
    public async Task Login_ReturnsOkWithValidToken_WhenCredentialsAreValid()
    {
        // Arrange
        var loginModel = new LoginModel
        {
            Username = "user1",
            Password = "password123"
        };

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(loginModel.Password);
        var mockUser = new Account
        {
            Username = "user1",
            Password = hashedPassword,
            RoleId = 1,
            SystemName = "SystemA"
        };

        // Ensure the key is at least 32 characters
        _mockConfiguration.Setup(config => config["Jwt:Key"]).Returns("test_secret_key_1234567890_32_chars");
        _mockConfiguration.Setup(config => config["Jwt:Issuer"]).Returns("test_issuer");
        _mockConfiguration.Setup(config => config["Jwt:Audience"]).Returns("test_audience");
        _mockConfiguration.Setup(config => config["Jwt:ExpiryMinutes"]).Returns("30");

        _mockDbContext
            .Setup(db => db.LoadAsync<Account>(loginModel.Username, default))
            .ReturnsAsync(mockUser);

        // Act
        var result = await _authController.Login(loginModel);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Deserialize the Value into a dictionary
        var response = JsonSerializer.Deserialize<Dictionary<string, object>>(
            JsonSerializer.Serialize(okResult.Value)
        );

        // Assert individual properties
        Assert.NotNull(response);
        Assert.True(bool.Parse(response["status"].ToString()));
        Assert.NotNull(response["token"]);
        Assert.Equal(mockUser.RoleId, int.Parse(response["userRole"].ToString()));
        Assert.Equal(mockUser.SystemName, response["systemName"].ToString());
    }


    [Fact]
    public async Task Login_ReturnsBadRequest_WhenLoginModelIsInvalid()
    {
        // Arrange
        LoginModel invalidLoginModel = null;

        // Act
        var result = await _authController.Login(invalidLoginModel);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        // Deserialize the Value into a dictionary
        var response = JsonSerializer.Deserialize<Dictionary<string, object>>(
            JsonSerializer.Serialize(badRequestResult.Value)
        );

        // Assert the individual properties
        Assert.NotNull(response);
        Assert.Equal("Thông tin đăng nhập không hợp lệ.", response["message"].ToString());
        Assert.False(bool.Parse(response["status"].ToString()));
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        var loginModel = new LoginModel
        {
            Username = "nonexistent",
            Password = "password123"
        };

        _mockDbContext
            .Setup(db => db.LoadAsync<Account>(loginModel.Username, default))
            .ReturnsAsync((Account)null);

        // Act
        var result = await _authController.Login(loginModel);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);

        // Deserialize the Value into a dictionary
        var response = JsonSerializer.Deserialize<Dictionary<string, object>>(
            JsonSerializer.Serialize(unauthorizedResult.Value)
        );

        // Validate individual properties
        Assert.NotNull(response);
        Assert.False(bool.Parse(response["status"].ToString()));
        Assert.Equal("Tên người dùng không tồn tại.", response["message"].ToString());
    }


    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsInvalid()
    {
        // Arrange
        var loginModel = new LoginModel
        {
            Username = "user1",
            Password = "wrongpassword"
        };

        var mockUser = new Account
        {
            Username = "user1",
            Password = BCrypt.Net.BCrypt.HashPassword("password123"), // Different password
            RoleId = 1,
            SystemName = "SystemA"
        };

        _mockDbContext
            .Setup(db => db.LoadAsync<Account>(loginModel.Username, default))
            .ReturnsAsync(mockUser);

        // Act
        var result = await _authController.Login(loginModel);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);

        // Deserialize the Value into a dictionary
        var response = JsonSerializer.Deserialize<Dictionary<string, object>>(
            JsonSerializer.Serialize(unauthorizedResult.Value)
        );

        // Validate individual properties
        Assert.NotNull(response);
        Assert.False(bool.Parse(response["status"].ToString()));
        Assert.Equal("Mật khẩu không chính xác.", response["message"].ToString());
    }

    [Fact]
    public void JwtTokenGenerator_ThrowsException_WhenKeyIsMissing()
    {
        // Arrange
        var invalidConfig = new Mock<IConfiguration>();
        invalidConfig.Setup(config => config["Jwt:Key"]).Returns((string)null);

        var jwtGenerator = new JwtTokenGenerator(invalidConfig.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            jwtGenerator.GenerateJwtToken("user1", "1", "SystemA"));
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenDtoIsNull()
    {
        // Arrange
        AccountRegisterDTO nullDto = null;

        // Act
        var result = await _authController.Register(nullDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Thông tin đăng ký không hợp lệ.", badRequestResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var invalidDto = new AccountRegisterDTO
        {
            Username = "ab", // Invalid (less than 3 characters)
            Password = "123", // Invalid (less than 6 characters)
            Email = "invalid-email", // Invalid email format
            SystemName = "", // Empty system name
            WebhookUrl = "", // Empty webhook URL
            WebhookSecretKey = "" // Empty webhook secret key
        };

        // Act
        var result = await _authController.Register(invalidDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Tên người dùng phải có ít nhất 3 ký tự.", badRequestResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenUserAlreadyExists()
    {
        // Arrange
        var existingUser = new Account { Username = "existinguser" };
        _mockDbContext
            .Setup(db => db.LoadAsync<Account>("existinguser", default))
            .ReturnsAsync(existingUser);

        var registerDto = new AccountRegisterDTO
        {
            Username = "existinguser",
            Password = "password123",
            Email = "user@example.com",
            SystemName = "SystemA",
            WebhookUrl = "https://webhook.example.com",
            WebhookSecretKey = "secret"
        };

        // Act
        var result = await _authController.Register(registerDto);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("Tên người dùng đã tồn tại.", conflictResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var registerDto = new AccountRegisterDTO
        {
            Username = "newuser",
            Password = "password123",
            Email = "user@example.com",
            SystemName = "SystemA",
            WebhookUrl = "https://webhook.example.com",
            WebhookSecretKey = "secret"
        };

        Account savedUser = null;

        // Setup the mock to capture the saved user
        _mockDbContext
            .Setup(db => db.LoadAsync<Account>("newuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account)null);

        _mockDbContext
            .Setup(db => db.SaveAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .Callback<Account, CancellationToken>((user, _) => savedUser = user)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authController.Register(registerDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Đăng ký thành công!", okResult.Value);

        // Assert that the user was saved with the correct details
        Assert.NotNull(savedUser);
        Assert.Equal(registerDto.Username, savedUser.Username);
        Assert.True(BCrypt.Net.BCrypt.Verify(registerDto.Password, savedUser.Password));
        Assert.Equal(registerDto.Email, savedUser.Email);
        Assert.Equal(2, savedUser.RoleId);
        Assert.Equal(registerDto.SystemName, savedUser.SystemName);
        Assert.Equal(registerDto.WebhookUrl, savedUser.WebhookUrl);
        Assert.Equal(registerDto.WebhookSecretKey, savedUser.WebhookSecretKey);
        Assert.Equal("Deactive", savedUser.Status);
    }



}

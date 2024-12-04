using FALWebhook.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Share.DTO;
using Share.Model;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class WebhookReceiverControllerTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly WebhookReceiverController _controller;

    public WebhookReceiverControllerTests()
    {
        // Mock configuration
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration
            .Setup(config => config["SecretKey"])
            .Returns("your-secret-key");

        _controller = new WebhookReceiverController(_mockConfiguration.Object);
    }

    [Fact]
    public async Task ReceiveData_ValidSignature_ReturnsOk()
    {
        // Arrange
        var payload = new FaceDetectionResult
        {
            FileName = "test_image.jpg",
            RegisteredFaces = new List<FaceRecognitionResponse>
        {
            new FaceRecognitionResponse
            {
                FaceId = "12345",
                UserId = "user1",
                TimeAppearances = "10:00:00",
                BoundingBox = new BoundingBox { Top = 0.1f, Left = 0.1f, Width = 0.5f, Height = 0.5f }
            }
        },
            UnregisteredFaces = new List<FaceRecognitionResponse>
        {
            new FaceRecognitionResponse
            {
                FaceId = "54321",
                UserId = null,
                TimeAppearances = "10:01:00",
                BoundingBox = new BoundingBox { Top = 0.2f, Left = 0.2f, Width = 0.4f, Height = 0.4f }
            }
        },
            Width = 1920,
            Height = 1080,
            Key = "image_key"
        };

        var payloadString = JsonSerializer.Serialize(payload);
        var secretKey = "your-secret-key";
        var signature = GenerateHMAC(payloadString, secretKey);

        // Act
        var result = await _controller.ReceiveData(signature, payload);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        var message = response?.GetType().GetProperty("Message")?.GetValue(response, null) as string;
        Assert.Equal("Webhook đã nhận dữ liệu thành công.", message);
    }


    [Fact]
    public async Task ReceiveData_InvalidSignature_ReturnsUnauthorized()
    {
        // Arrange
        var payload = new FaceDetectionResult
        {
            FileName = "test_image.jpg",
            RegisteredFaces = new List<FaceRecognitionResponse>
            {
                new FaceRecognitionResponse
                {
                    FaceId = "12345",
                    UserId = "user1",
                    TimeAppearances = "10:00:00",
                    BoundingBox = new BoundingBox { Top = 0.1f, Left = 0.1f, Width = 0.5f, Height = 0.5f }
                }
            },
            UnregisteredFaces = null,
            Width = 1920,
            Height = 1080,
            Key = "image_key"
        };

        var invalidSignature = "invalid-signature";

        // Act
        var result = await _controller.ReceiveData(invalidSignature, payload);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Chữ ký không hợp lệ.", unauthorizedResult.Value);
    }

    private static string GenerateHMAC(string payload, string secret)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(hashBytes);
        }
    }
}

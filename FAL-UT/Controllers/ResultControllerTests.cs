using Amazon.DynamoDBv2.Model;
using FAL.Controllers;
using FAL.Services.IServices;
using FAL.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Share.Constant;
using Share.DTO;
using System.Security.Claims;

public class ResultControllerTests
{
    private readonly Mock<IDynamoDBService> _mockDynamoService;
    private readonly ResultController _controller;

    public ResultControllerTests()
    {
        _mockDynamoService = new Mock<IDynamoDBService>();

        // Pass null for the logger
        _controller = new ResultController(_mockDynamoService.Object, null);
    }

    [Fact]
    public async Task GetWebhookResult_ValidInput_ReturnsOkResult()
    {
        // Arrange
        var mediaId = "12345";
        var systermId = "testSystem";
        var expectedResult = new FaceDetectionResult();
        var claims = new List<Claim>
        {
            new Claim(GlobalVarians.SystermId, systermId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mockDynamoService
            .Setup(service => service.GetWebhookResult(It.IsAny<string>(), mediaId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetWebhookResult(mediaId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedResult, okResult.Value);
    }

    [Fact]
    public async Task GetWebhookResult_ExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var mediaId = "12345";
        var systermId = "testSystem";
        var claims = new List<Claim>
        {
            new Claim(GlobalVarians.SystermId, systermId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mockDynamoService
            .Setup(service => service.GetWebhookResult(It.IsAny<string>(), mediaId))
            .ThrowsAsync(new Exception("An error occurred"));

        // Act
        var result = await _controller.GetWebhookResult(mediaId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("An error occurred", badRequestResult.Value);
    }

    [Fact]
    public async Task GetResult_ValidInput_ReturnsOkResult()
    {
        // Arrange
        var fileName = "testFile";
        var systermId = "testSystem";
        var expectedResult = "resultString";
        var claims = new List<Claim>
        {
            new Claim(GlobalVarians.SystermId, systermId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var queryResult = new Dictionary<string, AttributeValue>
        {
            { ":v_fileName", new AttributeValue { S = fileName } }
        };

        _mockDynamoService
            .Setup(service => service.GetRecordByKeyConditionExpressionAsync(
                StringExtention.GetTableNameResult(systermId),
                "FileName = :v_fileName",
                It.IsAny<Dictionary<string, AttributeValue>>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetResult(fileName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedResult, okResult.Value);
    }

    [Fact]
    public async Task GetResult_ExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var fileName = "testFile";
        var systermId = "testSystem";
        var claims = new List<Claim>
        {
            new Claim(GlobalVarians.SystermId, systermId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mockDynamoService
            .Setup(service => service.GetRecordByKeyConditionExpressionAsync(
                StringExtention.GetTableNameResult(systermId),
                "FileName = :v_fileName",
                It.IsAny<Dictionary<string, AttributeValue>>()))
            .ThrowsAsync(new Exception("An error occurred"));

        // Act
        var result = await _controller.GetResult(fileName);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("An error occurred", badRequestResult.Value);
    }
}

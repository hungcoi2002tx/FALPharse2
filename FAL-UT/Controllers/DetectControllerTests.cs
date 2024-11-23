using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using FAL.Controllers;
using FAL.Services.IServices;
using Share.SystemModel;

public class DetectControllerTests
{
    private readonly Mock<IS3Service> _mockS3Service;
    private readonly Mock<ICollectionService> _mockCollectionService;
    private readonly DetectController _controller;

    public DetectControllerTests()
    {
        _mockS3Service = new Mock<IS3Service>();
        _mockCollectionService = new Mock<ICollectionService>();

        // Pass null for the logger
        _controller = new DetectController( // s3Client is not used in the DetectAsync method
            null, // Ignore logger
            _mockCollectionService.Object,
            _mockS3Service.Object
        );

        // Mock user claims
        var claims = new List<Claim>
    {
        new Claim(GlobalVarians.SystermId, "test-system-id")
    };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };
    }


    [Fact]
    public async Task DetectAsync_ValidInput_ReturnsOk()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var content = "Fake file content";
        var fileName = "test.jpg";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;
        mockFile.Setup(_ => _.OpenReadStream()).Returns(ms);
        mockFile.Setup(_ => _.FileName).Returns(fileName);
        mockFile.Setup(_ => _.Length).Returns(ms.Length);

        _mockS3Service.Setup(x => x.AddBudgetAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockS3Service.Setup(x => x.AddFileToS3Async(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TypeOfRequest>(), It.IsAny<string>(), null))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DetectAsync(mockFile.Object, "test-media-id");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResultResponse>(okResult.Value);
        Assert.True(response.Status);
        Assert.Equal("The system has received the file.", response.Message);
    }

    [Fact]
    public async Task DetectAsync_InvalidFile_ReturnsBadRequest()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0); // Invalid file
        mockFile.Setup(f => f.FileName).Returns("invalid.jpg"); // Mock FileName

        // Act
        var result = await _controller.DetectAsync(mockFile.Object, "test-media-id");

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result); // Expecting ObjectResult for 400
        Assert.Equal(400, badRequestResult.StatusCode);

        var response = Assert.IsType<ResultResponse>(badRequestResult.Value);
        Assert.False(response.Status);
        Assert.Equal("Bad Request. Invalid value.", response.Message);
    }




    [Fact]
    public async Task DetectAsync_BucketDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var content = "Fake file content";
        var fileName = "test.jpg";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;
        mockFile.Setup(_ => _.OpenReadStream()).Returns(ms);
        mockFile.Setup(_ => _.FileName).Returns(fileName);
        mockFile.Setup(_ => _.Length).Returns(ms.Length);

        _mockS3Service.Setup(x => x.AddBudgetAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        var result = await _controller.DetectAsync(mockFile.Object, "test-media-id");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Bucket test-system-id does not exist.", notFoundResult.Value);
    }

    [Fact]
    public async Task DetectAsync_UnexpectedError_ReturnsInternalServerError()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var content = "Fake file content";
        var fileName = "test.jpg";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;
        mockFile.Setup(_ => _.OpenReadStream()).Returns(ms);
        mockFile.Setup(_ => _.FileName).Returns(fileName);
        mockFile.Setup(_ => _.Length).Returns(ms.Length);

        _mockS3Service.Setup(x => x.AddBudgetAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.DetectAsync(mockFile.Object, "test-media-id");

        // Assert
        var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, internalServerErrorResult.StatusCode);

        var response = Assert.IsType<ResultResponse>(internalServerErrorResult.Value);
        Assert.False(response.Status);
        Assert.Equal("Internal Server Error", response.Message);
    }
}

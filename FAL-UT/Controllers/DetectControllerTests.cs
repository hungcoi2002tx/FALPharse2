using FAL.Controllers;
using FAL.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Share.Constant;
using Share.DTO;
using Share.Model;
using Share.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

public class DetectControllerTests
{
    private readonly Mock<IS3Service> _mockS3Service;
    private readonly Mock<ICollectionService> _mockCollectionService;
    private readonly Mock<IDynamoDBService> _mockDynamoService;
    private readonly CustomLog _logger;
    private readonly DetectController _controller;

    public DetectControllerTests()
    {
        _mockS3Service = new Mock<IS3Service>();
        _mockCollectionService = new Mock<ICollectionService>();
        _mockDynamoService = new Mock<IDynamoDBService>();

        // Use a temporary file path for the CustomLog instance
        var tempLogFilePath = Path.GetTempFileName();
        _logger = new CustomLog(tempLogFilePath);

        // Initialize the controller with the dummy logger
        _controller = new DetectController(_logger, _mockCollectionService.Object, _mockS3Service.Object, _mockDynamoService.Object);

        // Mock user claims
        var claims = new List<Claim>
        {
            new Claim(GlobalVarians.SystermId, "test-system-id")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task DetectAsync_ValidFile_ReturnsOk()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var mediaId = "test-media-id";

        fileMock.Setup(f => f.FileName).Returns("test_image.jpg");
        fileMock.Setup(f => f.Length).Returns(4 * 1024 * 1024); // 4MB, valid for an image

        _mockS3Service.Setup(s => s.IsAddBudgetAsync(It.IsAny<string>()))
                      .ReturnsAsync(true);

        _mockS3Service.Setup(s => s.AddFileToS3Async(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TypeOfRequest>(), mediaId, null))
                      .ReturnsAsync(true);

        // Act
        var result = await _controller.DetectAsync(fileMock.Object, mediaId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResultResponse>(okResult.Value);
        Assert.True(response.Status);
        Assert.Equal("The system has received the file.", response.Message);
    }

    [Fact]
    public async Task DetectAsync_FileTooLarge_ThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var mediaId = "test-media-id";

        fileMock.Setup(f => f.FileName).Returns("test_image.jpg");
        fileMock.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6MB, exceeds max image size

        // Act
        var result = await _controller.DetectAsync(fileMock.Object, mediaId);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<ResultResponse>(badRequestResult.Value);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.False(response.Status);
        Assert.Equal("Bad Request. Invalid value.", response.Message);
    }

    [Fact]
    public async Task DetectAsync_InvalidFileExtension_ThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var mediaId = "test-media-id";

        // Mock file properties with an unsupported file extension
        fileMock.Setup(f => f.FileName).Returns("test_file.txt"); // Unsupported extension
        fileMock.Setup(f => f.Length).Returns(1 * 1024 * 1024); // 1MB
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("Test file content")));

        // Act
        var result = await _controller.DetectAsync(fileMock.Object, mediaId);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<ResultResponse>(badRequestResult.Value);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.False(response.Status);
        Assert.Equal("Bad Request. Invalid value.", response.Message);
    }

    [Fact]
    public async Task DetectAsync_NullFile_ThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        IFormFile? file = null;
        var mediaId = "test-media-id";

        // Act
        var result = await _controller.DetectAsync(file, mediaId);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<ResultResponse>(badRequestResult.Value);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.False(response.Status);
        Assert.Equal("Bad Request. Invalid value.", response.Message);
    }

    [Fact]
    public async Task UploadMultipleImages_ValidFiles_ReturnsOk()
    {
        // Arrange
        var files = new FormFileCollection
        {
            CreateMockFile("test_image1.jpg", 1024 * 1024), // 1MB
            CreateMockFile("test_image2.jpg", 1024 * 1024)  // 1MB
        };

        _mockS3Service.Setup(s => s.IsAddBudgetAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockS3Service.Setup(s => s.AddFileToS3Async(
    It.IsAny<IFormFile>(),
    It.IsAny<string>(),
    It.IsAny<string>(),
    It.IsAny<TypeOfRequest>(),
    It.IsAny<string>(),
    It.IsAny<string>() // Optional parameter, defaults to null
)).ReturnsAsync(true);


        // Act
        var result = await _controller.UploadMultipleImages(files);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Files uploaded successfully.", okResult.Value);
    }

    [Fact]
    public async Task UploadMultipleImages_NoFiles_ReturnsBadRequest()
    {
        // Arrange
        var files = new FormFileCollection(); // No files

        // Act
        var result = await _controller.UploadMultipleImages(files);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No files received from the upload.", badRequestResult.Value);
    }

    [Fact]
    public async Task UploadMultipleImages_BucketDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var files = new FormFileCollection
        {
            CreateMockFile("test_image1.jpg", 1024 * 1024) // 1MB
        };

        _mockS3Service.Setup(s => s.IsAddBudgetAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        var result = await _controller.UploadMultipleImages(files);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Bucket test-system-id does not exist.", notFoundResult.Value);
    }

    [Fact]
    public async Task UploadMultipleImages_InvalidFile_ThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var files = new FormFileCollection
    {
        CreateMockFile("test_file.txt", 1024) // Invalid extension
    };

        // Ensure bucket exists
        _mockS3Service.Setup(s => s.IsAddBudgetAsync(It.IsAny<string>())).ReturnsAsync(true);

        // Act
        var result = await _controller.UploadMultipleImages(files);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var responseMessage = badRequestResult.Value.ToString();
        Assert.NotNull(responseMessage);
        Assert.Contains("Chỉ chấp nhận file ảnh", responseMessage); // Match a part of the expected error message
    }



    private IFormFile CreateMockFile(string fileName, long size)
    {
        var content = new MemoryStream(Encoding.UTF8.GetBytes("Dummy content"));
        return new FormFile(content, 0, size, fileName, fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }
}

using Amazon.Rekognition.Model;
using FAL.Controllers;
using FAL.Services.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Share.Constant;
using Share.Data;
using Share.DTO;
using Share.Model;
using Share.Utils;
using System;
using System.IO.Compression;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

public class TrainControllerTests
{
    private readonly Mock<ICollectionService> _mockCollectionService;
    private readonly Mock<IS3Service> _mockS3Service;
    private readonly Mock<IDynamoDBService> _mockDynamoService;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly CustomLog _logger;
    private readonly TrainController _controller;

    public TrainControllerTests()
    {
        _mockCollectionService = new Mock<ICollectionService>();
        _mockS3Service = new Mock<IS3Service>();
        _mockDynamoService = new Mock<IDynamoDBService>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();

        // Use a temporary file path for the CustomLog instance
        var tempLogFilePath = Path.GetTempFileName();
        _logger = new CustomLog(tempLogFilePath);

        _controller = new TrainController(null, _logger, _mockCollectionService.Object, _mockS3Service.Object, _mockDynamoService.Object, _mockEnvironment.Object);

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
    public async Task DeleteByUserIdAsync_ValidUserId_ReturnsOk()
    {
        // Arrange
        var userId = "test-user-id";

        _mockCollectionService.Setup(s => s.DeleteByUserIdAsync(userId, It.IsAny<string>()))
                              .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteByUserIdAsync(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Extract the anonymous object and validate the Status property
        var response = okResult.Value;
        Assert.NotNull(response);
        var statusProperty = response.GetType().GetProperty("Status");
        Assert.NotNull(statusProperty);
        var status = (bool)statusProperty.GetValue(response);
        Assert.True(status);
    }


    [Fact]
    public async Task DeleteByUserIdAsync_InvalidUserId_ReturnsBadRequest()
    {
        // Arrange
        var userId = "invalid-user-id";

        _mockCollectionService.Setup(s => s.DeleteByUserIdAsync(userId, It.IsAny<string>()))
                              .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteByUserIdAsync(userId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        // Extract the anonymous object and validate the Status property
        var response = badRequestResult.Value;
        Assert.NotNull(response);
        Assert.True(response.GetType().GetProperty("Status")?.GetValue(response) is bool status && !status);
    }


    [Fact]
    public async Task DeleteByUserIdAsync_ExceptionThrown_ReturnsServerError()
    {
        // Arrange
        var userId = "test-user-id";

        _mockCollectionService.Setup(s => s.DeleteByUserIdAsync(userId, It.IsAny<string>()))
                              .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.DeleteByUserIdAsync(userId);

        // Assert
        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<ResultResponse>(serverErrorResult.Value);
        Assert.Equal(500, serverErrorResult.StatusCode);
        Assert.False(response.Status);
        Assert.Equal("Internal Server Error", response.Message);
    }

    [Fact]
    public async Task ResetByUserId_SuccessfulReset_ReturnsOk()
    {
        // Arrange
        var userIdRequest = new UserIdRequest { UserId = "test-user-id" };
        _mockCollectionService.Setup(s => s.DeleteFromCollectionAsync(userIdRequest.UserId, It.IsAny<string>()))
                              .ReturnsAsync(true);

        // Act
        var result = await _controller.ResetByUserId(userIdRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
        var statusProperty = response.GetType().GetProperty("Status");
        Assert.NotNull(statusProperty);
        var status = (bool)statusProperty.GetValue(response);
        Assert.True(status);
    }

    [Fact]
    public async Task ResetByUserId_UnsuccessfulReset_ReturnsBadRequest()
    {
        // Arrange
        var userIdRequest = new UserIdRequest { UserId = "test-user-id" };
        _mockCollectionService.Setup(s => s.DeleteFromCollectionAsync(userIdRequest.UserId, It.IsAny<string>()))
                              .ReturnsAsync(false);

        // Act
        var result = await _controller.ResetByUserId(userIdRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = badRequestResult.Value;
        Assert.NotNull(response);
        var statusProperty = response.GetType().GetProperty("Status");
        Assert.NotNull(statusProperty);
        var status = (bool)statusProperty.GetValue(response);
        Assert.False(status);
    }

    [Fact]
    public async Task ResetByUserId_ExceptionThrown_ReturnsServerError()
    {
        // Arrange
        var userIdRequest = new UserIdRequest { UserId = "test-user-id" };
        _mockCollectionService.Setup(s => s.DeleteFromCollectionAsync(userIdRequest.UserId, It.IsAny<string>()))
                              .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.ResetByUserId(userIdRequest);

        // Assert
        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);
        var response = Assert.IsType<ResultResponse>(serverErrorResult.Value);
        Assert.False(response.Status);
        Assert.Equal("Internal Server Error", response.Message);
    }

    [Fact]
    public async Task CheckIsTrained_UserTrained_ReturnsOk()
    {
        // Arrange
        _mockDynamoService.Setup(s => s.GetFaceIdsByUserIdAsync("user1", "test-system-id"))
                          .ReturnsAsync(new List<string> { "face1" });

        // Act
        var result = await _controller.CheckIsTrained("user1");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResultIsTrainedModel>(okResult.Value);
        Assert.True(response.Status);
        Assert.True(response.IsTrained);
        Assert.Equal("The system training was successful.", response.Message);
    }

    [Fact]
    public async Task CheckIsTrained_UserNotTrained_ReturnsOk()
    {
        // Arrange
        _mockDynamoService.Setup(s => s.GetFaceIdsByUserIdAsync("user1", "test-system-id"))
                          .ReturnsAsync(new List<string>());

        // Act
        var result = await _controller.CheckIsTrained("user1");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResultIsTrainedModel>(okResult.Value);
        Assert.True(response.Status);
        Assert.False(response.IsTrained);
    }

    [Fact]
    public async Task CheckIsTrained_DynamoServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockDynamoService.Setup(s => s.GetFaceIdsByUserIdAsync("user1", "test-system-id"))
                           .ThrowsAsync(new Exception("Some error"));

        // Act
        var result = await _controller.CheckIsTrained("user1");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<ResultIsTrainedModel>(objectResult.Value);
        Assert.False(response.Status);
        Assert.False(response.IsTrained);
        Assert.Equal("Internal Server Error", response.Message);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task CheckIsTrained_InvalidUserId_ReturnsInternalServerError()
    {
        // Arrange
        var invalidUserId = string.Empty;  // Simulate invalid user ID (empty string or null)
        _mockDynamoService.Setup(s => s.GetFaceIdsByUserIdAsync(invalidUserId, "test-system-id"))
                           .ThrowsAsync(new ArgumentException("Invalid UserId"));

        // Act
        var result = await _controller.CheckIsTrained(invalidUserId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<ResultIsTrainedModel>(objectResult.Value);
        Assert.False(response.Status);
        Assert.False(response.IsTrained);
        Assert.Equal("Internal Server Error", response.Message);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task CheckIsTrained_MissingSystemId_ReturnsInternalServerError()
    {
        // Arrange
        var missingSystermId = "nonexistent-system-id";  // Simulate missing or invalid system ID
        _mockDynamoService.Setup(s => s.GetFaceIdsByUserIdAsync("user1", missingSystermId))
                           .ThrowsAsync(new ArgumentException("System ID is missing"));

        // Act
        var result = await _controller.CheckIsTrained("user1");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<ResultIsTrainedModel>(objectResult.Value);
        Assert.False(response.Status);
        Assert.False(response.IsTrained);
        Assert.Equal("Internal Server Error", response.Message);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task CheckIsTrained_EmptyResponseList_ReturnsOkWithIsTrainedFalse()
    {
        // Arrange
        _mockDynamoService.Setup(s => s.GetFaceIdsByUserIdAsync("user1", "test-system-id"))
                           .ReturnsAsync(new List<string>());  // Empty list as response

        // Act
        var result = await _controller.CheckIsTrained("user1");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResultIsTrainedModel>(okResult.Value);
        Assert.True(response.Status);
        Assert.False(response.IsTrained);
        Assert.Equal("The system training was successful.", response.Message);
    }

    [Fact]
    public async Task TrainByImageAsync_ValidFile_ReturnsOk()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("image.jpg");
        fileMock.Setup(f => f.Length).Returns(1024);

        _mockCollectionService.Setup(s => s.DetectFaceByFileAsync(It.IsAny<IFormFile>()))
                              .ReturnsAsync(new DetectFacesResponse { FaceDetails = new List<FaceDetail> { new FaceDetail() } });

        _mockCollectionService.Setup(s => s.IndexFaceByFileAsync(It.IsAny<Image>(), "test-system-id", "user1"))
                              .ReturnsAsync(new IndexFacesResponse
                              {
                                  FaceRecords = new List<FaceRecord>
                                  {
                                  new FaceRecord { Face = new Face { FaceId = "face-id" } }
                                  }
                              });

        // Act
        var result = await _controller.TrainByImageAsync(fileMock.Object, "user1");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResultResponse>(okResult.Value);
        Assert.True(response.Status);
        Assert.Equal("The system training was successful.", response.Message);
    }

    [Fact]
    public async Task TrainByImageAsync_InvalidFile_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("invalid.docx");
        fileMock.Setup(f => f.Length).Returns(1024);

        // Act
        var result = await _controller.TrainByImageAsync(fileMock.Object, "user1");

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task TrainByFaceIdAsync_ValidFaceId_ReturnsOk()
    {
        // Arrange
        var faceTrainModel = new FaceTrainModel
        {
            FaceId = "face-id",
            UserId = "user1"
        };

        _mockDynamoService.Setup(s => s.IsExistFaceIdAsync("test-system-id", "face-id")).ReturnsAsync(false);

        // Act
        var result = await _controller.TrainByFaceIdAsync(faceTrainModel);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResultResponse>(okResult.Value);
        Assert.True(response.Status);
        Assert.Equal("The system training was successful.", response.Message);
    }

    [Fact]
    public async Task DisassociateByFaceIdAsync_ValidFaceId_ReturnsOk()
    {
        // Arrange
        var faceTrainModel = new FaceTrainModel
        {
            FaceId = "face-id",
            UserId = "user1"
        };

        // Mock dependencies to simulate valid scenarios
        _mockDynamoService.Setup(s => s.IsExistFaceIdAsync("test-system-id", "face-id")).ReturnsAsync(true);
        _mockDynamoService.Setup(s => s.IsExistUserAsync("test-system-id", "user1")).ReturnsAsync(true); // Mock user exists
        _mockCollectionService.Setup(s => s.DisassociatedFaceAsync("test-system-id", "face-id", "user1"))
                      .ReturnsAsync(true); // Return Task<bool> with true

        _mockDynamoService.Setup(s => s.DeleteUserInformationAsync("test-system-id", "user1", "face-id"))
                          .ReturnsAsync(true); // Return Task<bool> with true

        _mockDynamoService.Setup(s => s.GetFaceIdsByUserIdAsync("user1", "test-system-id"))
                          .ReturnsAsync(new List<string>()); // No faces remain
        _mockCollectionService.Setup(s => s.DeleteUserFromRekognitionCollectionAsync("test-system-id", "user1"))
                              .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DisassociateByFaceIdAsync(faceTrainModel);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResultResponse>(okResult.Value);
        Assert.True(response.Status);
        Assert.Equal("The disassociate was successful.", response.Message);
    }

    [Fact]
    public async Task DisassociateByFaceIdAsync_FaceIdNotFound_ReturnsBadRequest()
    {
        // Arrange
        var faceTrainModel = new FaceTrainModel
        {
            FaceId = "face-id",
            UserId = "user1"
        };

        // Mock dependencies to simulate the scenario where the FaceId does not exist
        _mockDynamoService.Setup(s => s.IsExistFaceIdAsync("test-system-id", "face-id")).ReturnsAsync(false); // FaceId doesn't exist

        // Act
        var result = await _controller.DisassociateByFaceIdAsync(faceTrainModel);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ResultResponse>(badRequestResult.Value);
        Assert.False(response.Status);
        Assert.Equal("FaceId is not existed in systerm", response.Message);
    }
    [Fact]
    public async Task DisassociateByFaceIdAsync_UserNotFound_ReturnsBadRequest()
    {
        // Arrange
        var faceTrainModel = new FaceTrainModel
        {
            FaceId = "face-id",
            UserId = "user1"
        };

        // Mock dependencies to simulate the scenario where the user doesn't exist
        _mockDynamoService.Setup(s => s.IsExistFaceIdAsync("test-system-id", "face-id")).ReturnsAsync(true); // FaceId exists
        _mockDynamoService.Setup(s => s.IsExistUserAsync("test-system-id", "user1")).ReturnsAsync(false); // User doesn't exist

        // Act
        var result = await _controller.DisassociateByFaceIdAsync(faceTrainModel);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result); // Check for BadRequestObjectResult
        Assert.Equal(400, badRequestResult.StatusCode); // Check for 400 Bad Request
        var response = Assert.IsType<ResultResponse>(badRequestResult.Value); // Check the response type
        Assert.False(response.Status); // Check the status is false
        Assert.Equal("User does not exist, so face cannot be disassociated.", response.Message); // Check the error message
    }



    [Fact]
    public async Task DisassociateByFaceIdAsync_DisassociationFails_ReturnsServerError()
    {
        // Arrange
        var faceTrainModel = new FaceTrainModel
        {
            FaceId = "face-id",
            UserId = "user1"
        };

        // Mock dependencies to simulate the scenario where the FaceId exists and user exists
        _mockDynamoService.Setup(s => s.IsExistFaceIdAsync("test-system-id", "face-id")).ReturnsAsync(true);
        _mockDynamoService.Setup(s => s.IsExistUserAsync("test-system-id", "user1")).ReturnsAsync(true);
        _mockCollectionService.Setup(s => s.DisassociatedFaceAsync("test-system-id", "face-id", "user1"))
                               .ThrowsAsync(new Exception("Disassociation failed")); // Simulate failure

        // Act
        var result = await _controller.DisassociateByFaceIdAsync(faceTrainModel);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var response = Assert.IsType<ResultResponse>(objectResult.Value);
        Assert.False(response.Status);
        Assert.Equal("Internal Server Error", response.Message);
    }



    [Fact]
    public async Task UploadAndProcessZipFile_ValidZip_ReturnsOk()
    {
        // Arrange
        var zipFileMock = CreateMockZipFile(new List<string> { "image1.jpg", "image2.png" });

        _mockCollectionService.Setup(s => s.DetectFaceByFileAsync(It.IsAny<IFormFile>()))
            .ReturnsAsync(new DetectFacesResponse { FaceDetails = new List<FaceDetail> { new FaceDetail() } });

        _mockCollectionService.Setup(s => s.IndexFaceByFileAsync(It.IsAny<Image>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new IndexFacesResponse
            {
                FaceRecords = new List<FaceRecord>
                {
                new FaceRecord { Face = new Face { FaceId = "face-id-123" } }
                }
            });

        // Act
        var result = await _controller.UploadAndProcessZipFile(zipFileMock);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result); // Expecting ObjectResult
        Assert.Equal(200, objectResult.StatusCode); // Validate the status code is 200

        // Validate the ResultResponse object
        var response = Assert.IsType<ResultResponse>(objectResult.Value);
        Assert.True(response.Status);
        Assert.Contains("Training completed", response.Message);
    }


    [Fact]
    public async Task UploadAndProcessZipFile_NoImagesInZip_ReturnsBadRequest()
    {
        // Arrange
        var zipFileMock = CreateMockZipFile(new List<string> { "document.txt", "test.pdf" });

        // Act
        var result = await _controller.UploadAndProcessZipFile(zipFileMock);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result); // Check for ObjectResult
        Assert.Equal(400, objectResult.StatusCode); // Validate the status code is 400

        // Verify the ResultResponse object
        var response = Assert.IsType<ResultResponse>(objectResult.Value); // Ensure the returned value is a ResultResponse
        Assert.False(response.Status); // Status should be false
        Assert.Equal("Bad Request. Invalid value.", response.Message); // Check the message
    }


    [Fact]
    public async Task UploadAndProcessZipFile_InvalidZipFile_ReturnsBadRequest()
    {
        // Arrange
        var zipFileMock = CreateMockFile("invalid_file.txt");

        // Act
        var result = await _controller.UploadAndProcessZipFile(zipFileMock);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Only ZIP files are supported.", badRequestResult.Value);
    }

    private IFormFile CreateMockZipFile(List<string> fileNames)
    {
        // Path for the temporary ZIP file
        var tempZipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");

        using (var zipStream = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            foreach (var fileName in fileNames)
            {
                var entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                using (var entryStream = entry.Open())
                {
                    var content = Encoding.UTF8.GetBytes("Mock file content");
                    entryStream.Write(content, 0, content.Length);
                }
            }
        }

        // Mock the IFormFile
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(Path.GetFileName(tempZipPath));
        fileMock.Setup(f => f.Length).Returns(new FileInfo(tempZipPath).Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new FileStream(tempZipPath, FileMode.Open, FileAccess.Read));
        fileMock.Setup(f => f.ContentType).Returns("application/zip");

        return fileMock.Object;
    }





    private IFormFile CreateMockFile(string fileName)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(1024);
        return fileMock.Object;
    }



    private IFormFile CreateMockImageFile()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("Test image content"));

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test_image.jpg");
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        return fileMock.Object;
    }
}

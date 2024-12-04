using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FAL.Controllers;
using FAL.Services.IServices;
using Microsoft.AspNetCore.Http;
using Amazon.Rekognition.Model;
using Share.Utils;
using Share.Constant;
using Share.Model;
using Share.DTO;

public class CollectionControllerTests
{
    private readonly Mock<ICollectionService> _mockCollectionService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<CustomLog> _mockLogger;
    private readonly CollectionController _controller;

    public CollectionControllerTests()
    {
        _mockCollectionService = new Mock<ICollectionService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<CustomLog>("test-log-path");

        _controller = new CollectionController(
            Mock.Of<IS3Service>(), // S3Service is not used in this method
            _mockCollectionService.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );

        // Setup mock user claims
        var userClaims = new List<Claim>
        {
            new Claim(GlobalVarians.SystermId, "testSystemId")
        };
        var identity = new ClaimsIdentity(userClaims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetListFaceIdsAsync_ReturnsOkWithMappedFaces_WhenServiceSucceeds()
    {
        // Arrange
        var mockFaces = new List<Face>
    {
        new Face { FaceId = "face1", UserId = "user1" },
        new Face { FaceId = "face2", UserId = "user2" }
    };

        var mappedFaces = new List<FaceTrainModel>
    {
        new FaceTrainModel { FaceId = "face1", UserId = "user1" },
        new FaceTrainModel { FaceId = "face2", UserId = "user2" }
    };

        // Mock the ICollectionService to return the list of Faces
        _mockCollectionService
            .Setup(service => service.GetFacesAsync("testSystemId"))
            .ReturnsAsync(mockFaces);

        // Mock the IMapper to map from List<Face> to List<FaceTrainModel>
        _mockMapper
            .Setup(mapper => mapper.Map<List<FaceTrainModel>>(mockFaces))
            .Returns(mappedFaces);

        // Act
        var result = await _controller.GetListFaceIdsAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedFaces = Assert.IsType<List<FaceTrainModel>>(okResult.Value);
        Assert.Equal(mappedFaces.Count, returnedFaces.Count);
        Assert.Equal(mappedFaces[0].FaceId, returnedFaces[0].FaceId);
        Assert.Equal(mappedFaces[1].FaceId, returnedFaces[1].FaceId);
        Assert.Equal(mappedFaces[0].UserId, returnedFaces[0].UserId);
        Assert.Equal(mappedFaces[1].UserId, returnedFaces[1].UserId);
    }



    [Fact]
    public async Task GetListFaceIdsAsync_LogsExceptionAndReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mockCollectionService
            .Setup(service => service.GetFacesAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Test Exception"));

        // Act
        var result = await _controller.GetListFaceIdsAsync();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);

        var response = Assert.IsType<ResultResponse>(statusCodeResult.Value);
        Assert.False(response.Status);
        Assert.Equal("Internal Server Error", response.Message);

        // Removed logger verification
    }

    [Fact]
    public async Task DeleteFaceAsync_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var faceId = "testFaceId";
        var systermId = "testSystemId";
        _mockCollectionService
            .Setup(service => service.DeleteByFaceIdAsync(faceId, systermId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteFaceAsync(faceId, systermId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value);
    }

    [Fact]
    public async Task GetListCollectionAsync_ReturnsOk_WithCollections()
    {
        // Arrange
        var collections = new List<string> { "collection1", "collection2" };
        _mockCollectionService
            .Setup(service => service.GetCollectionAsync(GlobalVarians.SystermId))
            .ReturnsAsync(collections);

        // Act
        var result = await _controller.GetListCollectionAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCollections = Assert.IsType<List<string>>(okResult.Value); // Expecting List<string>
        Assert.Equal(collections.Count, returnedCollections.Count);
        Assert.Equal(collections, returnedCollections);
    }


    [Fact]
    public async Task CreateCollectionAsync_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var collectionId = "testCollectionId";
        _mockCollectionService
            .Setup(service => service.CreateCollectionByIdAsync(collectionId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreateCollectionAsync(collectionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value);
    }

    [Fact]
    public async Task DeleteFaceAsync_ThrowsException_WhenServiceFails()
    {
        // Arrange
        var faceId = "testFaceId";
        var systermId = "testSystemId";
        _mockCollectionService
            .Setup(service => service.DeleteByFaceIdAsync(faceId, systermId))
            .ThrowsAsync(new Exception("Service failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.DeleteFaceAsync(faceId, systermId));
    }

    [Fact]
    public async Task GetListCollectionAsync_ThrowsException_WhenServiceFails()
    {
        // Arrange
        _mockCollectionService
            .Setup(service => service.GetCollectionAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Service failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.GetListCollectionAsync());
    }

    [Fact]
    public async Task CreateCollectionAsync_ThrowsException_WhenServiceFails()
    {
        // Arrange
        var collectionId = "testCollectionId";
        _mockCollectionService
            .Setup(service => service.CreateCollectionByIdAsync(collectionId))
            .ThrowsAsync(new Exception("Service failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.CreateCollectionAsync(collectionId));
    }

}

using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Xunit;
using FAL.Services;
using FAL.Services.IServices;
using Share.SystemModel;
using Microsoft.AspNetCore.Http;

public class CollectionServiceTests
{
    private readonly Mock<IAmazonRekognition> _mockRekognitionClient;
    private readonly Mock<IDynamoDBService> _mockDynamoDBService;
    private readonly Mock<CustomLog> _mockLogger;
    private readonly CollectionService _service;
    private readonly Mock<CollectionService> _mockService;

    public CollectionServiceTests()
    {
        _mockRekognitionClient = new Mock<IAmazonRekognition>();
        _mockDynamoDBService = new Mock<IDynamoDBService>();
        _mockLogger = new Mock<CustomLog>("dummy-path");
        _service = new CollectionService(_mockRekognitionClient.Object, _mockLogger.Object, _mockDynamoDBService.Object);
        _mockService = new Mock<CollectionService>(_mockRekognitionClient.Object, _mockLogger.Object, _mockDynamoDBService.Object);
    }

    // Test for CreateCollectionAsync
    [Fact]
    public async Task CreateCollectionAsync_Success_ReturnsTrue()
    {
        // Arrange
        string systermId = "test-system-id";
        var createCollectionResponse = new CreateCollectionResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };

        _mockRekognitionClient.Setup(x => x.CreateCollectionAsync(It.IsAny<CreateCollectionRequest>(), default))
            .ReturnsAsync(createCollectionResponse);

        // Act
        var result = await _service.CreateCollectionAsync(systermId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CreateCollectionAsync_CollectionAlreadyExists_ThrowsException()
    {
        // Arrange
        string systermId = "test-system-id";
        var createCollectionResponse = new CreateCollectionResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };

        _mockRekognitionClient.Setup(x => x.CreateCollectionAsync(It.IsAny<CreateCollectionRequest>(), default))
            .ThrowsAsync(new ResourceAlreadyExistsException("Collection already exists"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.CreateCollectionAsync(systermId));
        Assert.Equal("Collection already exists.", ex.Message);
    }

    [Fact]
    public async Task CreateCollectionAsync_InvalidParameter_ThrowsException()
    {
        // Arrange
        string systermId = "test-system-id";

        _mockRekognitionClient.Setup(x => x.CreateCollectionAsync(It.IsAny<CreateCollectionRequest>(), default))
            .ThrowsAsync(new InvalidParameterException("Invalid parameter"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.CreateCollectionAsync(systermId));
        Assert.Equal("Collection id invalid.", ex.Message);
    }

    [Fact]
    public async Task CreateCollectionAsync_GeneralException_ThrowsException()
    {
        // Arrange
        string systermId = "test-system-id";

        _mockRekognitionClient.Setup(x => x.CreateCollectionAsync(It.IsAny<CreateCollectionRequest>(), default))
            .ThrowsAsync(new Exception("An error occurred"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.CreateCollectionAsync(systermId));
        Assert.Equal("An error occurred", ex.Message);
    }

    // Test for DeleteByFaceIdAsync
    [Fact]
    public async Task DeleteByFaceIdAsync_Success_ReturnsTrue()
    {
        // Arrange
        string systermId = "test-system-id";
        string faceId = "face-id";

        var deleteFacesResponse = new DeleteFacesResponse
        {
            HttpStatusCode = HttpStatusCode.OK
        };

        _mockRekognitionClient.Setup(x => x.DeleteFacesAsync(It.IsAny<DeleteFacesRequest>(), default))
            .ReturnsAsync(deleteFacesResponse);

        // Act
        var result = await _service.DeleteByFaceIdAsync(faceId, systermId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteByFaceIdAsync_BadRequest_ThrowsException()
    {
        // Arrange
        string systermId = "test-system-id";
        string faceId = "face-id";

        var deleteFacesResponse = new DeleteFacesResponse
        {
            HttpStatusCode = HttpStatusCode.BadRequest
        };

        _mockRekognitionClient.Setup(x => x.DeleteFacesAsync(It.IsAny<DeleteFacesRequest>(), default))
            .ReturnsAsync(deleteFacesResponse);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteByFaceIdAsync(faceId, systermId));
        Assert.Equal("Lỗi request xóa faceid trong collection", ex.Message);
    }

    [Fact]
    public async Task DeleteByFaceIdAsync_GeneralException_ThrowsException()
    {
        // Arrange
        string systermId = "test-system-id";
        string faceId = "face-id";

        _mockRekognitionClient.Setup(x => x.DeleteFacesAsync(It.IsAny<DeleteFacesRequest>(), default))
            .ThrowsAsync(new Exception("An error occurred"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteByFaceIdAsync(faceId, systermId));
        Assert.Equal("An error occurred", ex.Message);
    }

    [Fact]
    public async Task DeleteByUserIdAsync_ValidInput_ReturnsTrue()
    {
        // Arrange
        string userId = "user1";
        string systermId = "test-system-id";
        var faces = new List<Face>
        {
            new Face { UserId = userId, FaceId = "face1" },
            new Face { UserId = userId, FaceId = "face2" }
        };
        _mockRekognitionClient.Setup(x => x.ListFacesAsync(It.IsAny<ListFacesRequest>(), default))
            .ReturnsAsync(new ListFacesResponse { Faces = faces });

        _mockRekognitionClient.Setup(x => x.DeleteFacesAsync(It.IsAny<DeleteFacesRequest>(), default))
            .ReturnsAsync(new DeleteFacesResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        // Act
        var result = await _service.DeleteByUserIdAsync(userId, systermId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteByUserIdAsync_InvalidInput_ThrowsException()
    {
        // Arrange
        string userId = "";
        string systermId = "test-system-id";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteByUserIdAsync(userId, systermId));
        Assert.Equal("Invalid parameter", ex.Message);
    }

    [Fact]
    public async Task DeleteByUserIdAsync_NoFacesForUser_ReturnsTrue()
    {
        // Arrange
        string userId = "user1";
        string systermId = "test-system-id";
        _mockRekognitionClient.Setup(x => x.ListFacesAsync(It.IsAny<ListFacesRequest>(), default))
            .ReturnsAsync(new ListFacesResponse { Faces = new List<Face>() });

        // Act
        var result = await _service.DeleteByUserIdAsync(userId, systermId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteByUserIdAsync_ErrorInListFaces_ThrowsException()
    {
        // Arrange
        string userId = "user1";
        string systermId = "test-system-id";
        _mockRekognitionClient.Setup(x => x.ListFacesAsync(It.IsAny<ListFacesRequest>(), default))
            .ThrowsAsync(new Exception("ListFaces error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteByUserIdAsync(userId, systermId));
        Assert.Equal("ListFaces error", ex.Message);
    }

    [Fact]
    public async Task DeleteByUserIdAsync_ErrorInDeleteOrDisassociate_ThrowsException()
    {
        // Arrange
        string userId = "user1";
        string systermId = "test-system-id";
        var faces = new List<Face> { new Face { UserId = userId, FaceId = "face1" } };
        _mockRekognitionClient.Setup(x => x.ListFacesAsync(It.IsAny<ListFacesRequest>(), default))
            .ReturnsAsync(new ListFacesResponse { Faces = faces });

        _mockRekognitionClient.Setup(x => x.DeleteFacesAsync(It.IsAny<DeleteFacesRequest>(), default))
            .ThrowsAsync(new Exception("Delete face error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteByUserIdAsync(userId, systermId));
        Assert.Equal("Delete face error", ex.Message);
    }

    // Test for DisassociatedFaceAsync
    [Fact]
    public async Task DisassociatedFaceAsync_Success_ReturnsTrue()
    {
        // Arrange
        string systermId = "test-system-id";
        string faceId = "face-id";
        string userId = "user1";
        _mockRekognitionClient.Setup(x => x.DisassociateFacesAsync(It.IsAny<DisassociateFacesRequest>(), default))
            .ReturnsAsync(new DisassociateFacesResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        // Act
        var result = await _service.DisassociatedFaceAsync(systermId, faceId, userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DisassociatedFaceAsync_BadRequest_ThrowsException()
    {
        // Arrange
        string systermId = "test-system-id";
        string faceId = "face-id";
        string userId = "user1";
        _mockRekognitionClient.Setup(x => x.DisassociateFacesAsync(It.IsAny<DisassociateFacesRequest>(), default))
            .ReturnsAsync(new DisassociateFacesResponse { HttpStatusCode = System.Net.HttpStatusCode.BadRequest });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.DisassociatedFaceAsync(systermId, faceId, userId));
        Assert.Equal($"Disassociated Face with faceId {faceId} - UserId {userId} - Collection {systermId} fail", ex.Message);
    }

    // Test for IndexFaceAsync
    [Fact]
    public async Task IndexFaceAsync_Success_ReturnsResponse()
    {
        // Arrange
        string systermId = "test-system-id";
        string bucketName = "test-bucket";
        string imageName = "image.jpg";
        _mockRekognitionClient.Setup(x => x.IndexFacesAsync(It.IsAny<IndexFacesRequest>(), default))
            .ReturnsAsync(new IndexFacesResponse { FaceRecords = new List<FaceRecord> { new FaceRecord() } });

        // Act
        var result = await _service.IndexFaceAsync(systermId, bucketName, imageName);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task IndexFaceAsync_InvalidS3Object_ThrowsException()
    {
        // Arrange
        string systermId = "test-system-id";
        string bucketName = "test-bucket";
        string imageName = "image.jpg";
        _mockRekognitionClient.Setup(x => x.IndexFacesAsync(It.IsAny<IndexFacesRequest>(), default))
            .ThrowsAsync(new InvalidS3ObjectException("S3 object does not exist"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.IndexFaceAsync(systermId, bucketName, imageName));
        Assert.Equal("S3 object does not exist.", ex.Message);
    }

    [Fact]
    public async Task IndexFaceAsync_ResourceNotFound_ThrowsException()
    {
        // Arrange
        string systermId = "test-system-id";
        string bucketName = "test-bucket";
        string imageName = "image.jpg";
        _mockRekognitionClient.Setup(x => x.IndexFacesAsync(It.IsAny<IndexFacesRequest>(), default))
            .ThrowsAsync(new ResourceNotFoundException("The resource cannot be found"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.IndexFaceAsync(systermId, bucketName, imageName));
        Assert.Equal("The resource specified in the request cannot be found.", ex.Message);
    }

    [Fact]
    public async Task IndexFaceAsync_GeneralError_ThrowsException()
    {
        // Arrange
        string systermId = "test-system-id";
        string bucketName = "test-bucket";
        string imageName = "image.jpg";
        _mockRekognitionClient.Setup(x => x.IndexFacesAsync(It.IsAny<IndexFacesRequest>(), default))
            .ThrowsAsync(new Exception("General error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.IndexFaceAsync(systermId, bucketName, imageName));
        Assert.Equal("General error", ex.Message);
    }

    [Fact]
    public async Task IsCollectionExistAsync_CollectionExists_ReturnsTrue()
    {
        // Arrange
        string systermId = "test-system-id";
        var response = new ListCollectionsResponse
        {
            CollectionIds = new List<string> { systermId }
        };

        _mockRekognitionClient.Setup(x => x.ListCollectionsAsync(It.IsAny<ListCollectionsRequest>(), default))
            .ReturnsAsync(response);

        // Act
        var result = await _service.IsCollectionExistAsync(systermId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsCollectionExistAsync_CollectionDoesNotExist_ReturnsFalse()
    {
        // Arrange
        string systermId = "test-system-id";
        var response = new ListCollectionsResponse
        {
            CollectionIds = new List<string>()
        };

        _mockRekognitionClient.Setup(x => x.ListCollectionsAsync(It.IsAny<ListCollectionsRequest>(), default))
            .ReturnsAsync(response);

        // Act
        var result = await _service.IsCollectionExistAsync(systermId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsCollectionExistAsync_ErrorDuringListCollections_ThrowsException()
    {
        // Arrange
        string systermId = "test-system-id";
        _mockRekognitionClient.Setup(x => x.ListCollectionsAsync(It.IsAny<ListCollectionsRequest>(), default))
            .ThrowsAsync(new Exception("ListCollections error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.IsCollectionExistAsync(systermId));
        Assert.Equal("ListCollections error", ex.Message);
    }

    // Test for DetectFaceByFileAsync
    [Fact]
    public async Task DetectFaceByFileAsync_ValidFile_ReturnsDetectFacesResponse()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var imageBytes = new byte[] { 1, 2, 3 }; // Mock image content
        var memStream = new MemoryStream(imageBytes);

        fileMock.Setup(f => f.OpenReadStream()).Returns(memStream);
        var detectFacesResponse = new DetectFacesResponse
        {
            FaceDetails = new List<FaceDetail> { new FaceDetail() }
        };

        _mockRekognitionClient.Setup(x => x.DetectFacesAsync(It.IsAny<DetectFacesRequest>(), default))
            .ReturnsAsync(detectFacesResponse);

        // Act
        var result = await _service.DetectFaceByFileAsync(fileMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.FaceDetails); // Verify that faces were detected
    }

    [Fact]
    public async Task DetectFaceByFileAsync_FileProcessingError_ThrowsException()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.OpenReadStream()).Throws(new Exception("Error during file processing"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.DetectFaceByFileAsync(fileMock.Object));
        Assert.Equal("Error during file processing", ex.Message);
    }

    // Test for AssociateFacesAsync
    [Fact]
    public async Task AssociateFacesAsync_Success_ReturnsTrue()
    {
        // Arrange
        var faceIds = new List<string> { "face1", "face2" };
        string systermId = "test-system-id";
        string key = "image-key";

        _mockRekognitionClient.Setup(x => x.AssociateFacesAsync(It.IsAny<AssociateFacesRequest>(), default))
            .ReturnsAsync(new AssociateFacesResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        // Act
        var result = await _service.AssociateFacesAsync(systermId, faceIds, key);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AssociateFacesAsync_FailureInAssociate_ThrowsException()
    {
        // Arrange
        var faceIds = new List<string> { "face1", "face2" };
        string systermId = "test-system-id";
        string key = "image-key";

        _mockRekognitionClient.Setup(x => x.AssociateFacesAsync(It.IsAny<AssociateFacesRequest>(), default))
            .ReturnsAsync(new AssociateFacesResponse { HttpStatusCode = System.Net.HttpStatusCode.BadRequest });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.AssociateFacesAsync(systermId, faceIds, key));
        Assert.Equal("Associate request failed for faceId: face1", ex.Message);
    }

    [Fact]
    public async Task AssociateFacesAsync_ErrorInAssociation_ThrowsException()
    {
        // Arrange
        var faceIds = new List<string> { "face1", "face2" };
        string systermId = "test-system-id";
        string key = "image-key";

        _mockRekognitionClient.Setup(x => x.AssociateFacesAsync(It.IsAny<AssociateFacesRequest>(), default))
            .ThrowsAsync(new Exception("Error during face association"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.AssociateFacesAsync(systermId, faceIds, key));
        Assert.Equal("An error occurred while associating faces.", ex.Message);
    }

    [Fact]
    public async Task IsUserExistByUserIdAsync_UserExists_ReturnsTrue()
    {
        // Arrange
        var systermId = "test-system-id";
        var userId = "user1";
        var response = new SearchUsersResponse
        {
            UserMatches = new List<UserMatch>
    {
        new UserMatch
        {
            Similarity = 99.5f,
            User = new MatchedUser { UserId = "user123" } // Assuming MatchedUser has UserId
        }
    }
        };

        _mockRekognitionClient.Setup(x => x.SearchUsersAsync(It.IsAny<SearchUsersRequest>(), default))
            .ReturnsAsync(response);

        // Act
        var result = await _service.IsUserExistByUserIdAsync(systermId, userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsUserExistByUserIdAsync_UserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var systermId = "test-system-id";
        var userId = "user1";
        var response = new SearchUsersResponse
        {
            UserMatches = new List<UserMatch>()
        };

        _mockRekognitionClient.Setup(x => x.SearchUsersAsync(It.IsAny<SearchUsersRequest>(), default))
            .ReturnsAsync(response);

        // Act
        var result = await _service.IsUserExistByUserIdAsync(systermId, userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsUserExistByUserIdAsync_ErrorDuringSearch_ThrowsException()
    {
        // Arrange
        var systermId = "test-system-id";
        var userId = "user1";

        _mockRekognitionClient.Setup(x => x.SearchUsersAsync(It.IsAny<SearchUsersRequest>(), default))
            .ThrowsAsync(new Exception("Search error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.IsUserExistByUserIdAsync(systermId, userId));
        Assert.Equal("Search error", ex.Message);
    }

    // Test for CreateNewUserAsync
    [Fact]
    public async Task CreateNewUserAsync_UserCreationSuccess_ReturnsTrue()
    {
        // Arrange
        var systermId = "test-system-id";
        var userId = "user1";

        _mockRekognitionClient.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserRequest>(), default))
            .ReturnsAsync(new CreateUserResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK
            });

        // Act
        var result = await _service.CreateNewUserAsync(systermId, userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CreateNewUserAsync_UserCreationFailure_ThrowsException()
    {
        // Arrange
        var systermId = "test-system-id";
        var userId = "user1";

        _mockRekognitionClient.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserRequest>(), default))
            .ReturnsAsync(new CreateUserResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.BadRequest
            });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.CreateNewUserAsync(systermId, userId));
        Assert.Equal("Create User fail", ex.Message);
    }

    // Test for SearchUserByFaceIdsAsync
    [Fact]
    public async Task SearchUserByFaceIdsAsync_Success_ReturnsSearchUsersResponse()
    {
        // Arrange
        var systermId = "test-system-id";
        var faceId = "face-id";
        var response = new SearchUsersResponse
        {
            UserMatches = new List<UserMatch>
    {
        new UserMatch
        {
            Similarity = 99.5f,
            User = new MatchedUser { UserId = "user123" } // Assuming MatchedUser has UserId
        }
    }
        };

        _mockRekognitionClient.Setup(x => x.SearchUsersAsync(It.IsAny<SearchUsersRequest>(), default))
            .ReturnsAsync(response);

        // Act
        var result = await _service.SearchUserByFaceIdsAsync(systermId, faceId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.UserMatches);
    }

    [Fact]
    public async Task SearchUserByFaceIdsAsync_ErrorDuringSearch_ThrowsException()
    {
        // Arrange
        var systermId = "test-system-id";
        var faceId = "face-id";

        _mockRekognitionClient.Setup(x => x.SearchUsersAsync(It.IsAny<SearchUsersRequest>(), default))
            .ThrowsAsync(new Exception("Search error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.SearchUserByFaceIdsAsync(systermId, faceId));
        Assert.Equal("Search error", ex.Message);
    }

    // Test for GetFacesAsync
    [Fact]
    public async Task GetFacesAsync_Success_ReturnsListOfFaces()
    {
        // Arrange
        var systermId = "test-system-id";
        var response = new ListFacesResponse
        {
            Faces = new List<Face> { new Face { FaceId = "face1" }, new Face { FaceId = "face2" } }
        };

        _mockRekognitionClient.Setup(x => x.ListFacesAsync(It.IsAny<ListFacesRequest>(), default))
            .ReturnsAsync(response);

        // Act
        var result = await _service.GetFacesAsync(systermId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetFacesAsync_ErrorDuringListFaces_ThrowsException()
    {
        // Arrange
        var systermId = "test-system-id";

        _mockRekognitionClient.Setup(x => x.ListFacesAsync(It.IsAny<ListFacesRequest>(), default))
            .ThrowsAsync(new Exception("List faces error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetFacesAsync(systermId));
        Assert.Equal("List faces error", ex.Message);
    }

    [Fact]
    public async Task GetCollectionAsync_ReturnsCollectionIds()
    {
        // Arrange
        var systermId = "test-system-id";
        var expectedCollectionIds = new List<string> { "collection-1", "collection-2" };

        var listCollectionsResponse = new ListCollectionsResponse
        {
            CollectionIds = expectedCollectionIds,
            NextToken = null
        };

        _mockRekognitionClient
            .Setup(client => client.ListCollectionsAsync(It.IsAny<ListCollectionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(listCollectionsResponse);

        // Act
        var result = await _service.GetCollectionAsync(systermId);

        // Assert
        Assert.Equal(expectedCollectionIds, result);
    }

    [Fact]
    public async Task GetCollectionAsync_ThrowsException_ReturnsEmptyList()
    {
        // Arrange
        var systermId = "test-system-id";

        _mockRekognitionClient
            .Setup(client => client.ListCollectionsAsync(It.IsAny<ListCollectionsRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("An error occurred"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.GetCollectionAsync(systermId));
    }

    [Fact]
    public async Task CreateCollectionByIdAsync_Success_ReturnsTrue()
    {
        // Arrange
        var collectionId = "new-collection";
        var response = new CreateCollectionResponse
        {
            HttpStatusCode = HttpStatusCode.OK
        };

        _mockRekognitionClient
            .Setup(client => client.CreateCollectionAsync(It.IsAny<CreateCollectionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _service.CreateCollectionByIdAsync(collectionId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CreateCollectionByIdAsync_Failure_ThrowsException()
    {
        // Arrange
        var collectionId = "new-collection";
        var response = new CreateCollectionResponse
        {
            HttpStatusCode = HttpStatusCode.BadRequest
        };

        _mockRekognitionClient
            .Setup(client => client.CreateCollectionAsync(It.IsAny<CreateCollectionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.CreateCollectionByIdAsync(collectionId));
    }

    [Fact]
    public async Task IndexFaceByFileAsync_Success_ReturnsIndexFacesResponse()
    {
        // Arrange
        var systermId = "test-system-id";
        var image = new Image(); // Assuming Image object is correctly created
        var key = "some-key";
        var response = new IndexFacesResponse
        {
            FaceRecords = new List<FaceRecord>
            {
                new FaceRecord
                {
                    Face = new Face { FaceId = "test-face-id" }
                }
            },
            HttpStatusCode = HttpStatusCode.OK
        };

        _mockRekognitionClient
            .Setup(client => client.IndexFacesAsync(It.IsAny<IndexFacesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _service.IndexFaceByFileAsync(image, systermId, key);

        // Assert
        Assert.Equal(response, result);
    }

    [Fact]
    public async Task IndexFaceByFileAsync_Failure_ThrowsException()
    {
        // Arrange
        var systermId = "test-system-id";
        var image = new Image(); // Assuming Image object is correctly created
        var key = "some-key";

        _mockRekognitionClient
            .Setup(client => client.IndexFacesAsync(It.IsAny<IndexFacesRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error during indexing"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.IndexFaceByFileAsync(image, systermId, key));
    }

    [Fact]
    public async Task IndexFaceByFileAsync_S3ObjectNotFound_ThrowsException()
    {
        // Arrange
        var systermId = "test-system-id";
        var image = new Image(); // Assuming Image object is correctly created
        var key = "some-key";

        _mockRekognitionClient
            .Setup(client => client.IndexFacesAsync(It.IsAny<IndexFacesRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidS3ObjectException("S3 object does not exist"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.IndexFaceByFileAsync(image, systermId, key));
    }

    [Fact]
    public async Task DeleteFromCollectionAsync_NoFaceIds_DeletesUserAndReturnsTrue()
    {
        // Arrange
        string userId = "testUserId";
        string systemId = "testSystemId";

        _mockDynamoDBService.Setup(s => s.GetFaceIdsByUserIdAsync(userId, systemId))
            .ReturnsAsync((List<string>)null);

        _mockDynamoDBService.Setup(s => s.DeleteUserFromDynamoDbAsync(userId, systemId))
            .Returns(Task.CompletedTask);

        _mockService.Setup(s => s.DeleteUserFromRekognitionCollectionAsync(systemId, userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _mockService.Object.DeleteFromCollectionAsync(userId, systemId);

        // Assert
        Assert.True(result);
        _mockDynamoDBService.Verify(s => s.DeleteUserFromDynamoDbAsync(userId, systemId), Times.Once);
        _mockService.Verify(s => s.DeleteUserFromRekognitionCollectionAsync(systemId, userId), Times.Once);
    }

    [Fact]
    public async Task DeleteFromCollectionAsync_FaceIdsExist_FacesDeleted_UserDeleted_ReturnsTrue()
    {
        // Arrange
        string userId = "testUserId";
        string systemId = "testSystemId";
        var faceIds = new List<string> { "faceId1", "faceId2" };

        _mockDynamoDBService.Setup(s => s.GetFaceIdsByUserIdAsync(userId, systemId))
            .ReturnsAsync(faceIds);

        _mockService.Setup(s => s.DisassociatedFaceAsync(systemId, It.IsAny<string>(), userId))
            .ReturnsAsync(true);

        _mockRekognitionClient.Setup(s => s.DeleteFacesAsync(It.IsAny<DeleteFacesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteFacesResponse { DeletedFaces = faceIds });

        _mockDynamoDBService.Setup(s => s.DeleteUserFromDynamoDbAsync(userId, systemId))
            .Returns(Task.CompletedTask);

        _mockService.Setup(s => s.DeleteUserFromRekognitionCollectionAsync(systemId, userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _mockService.Object.DeleteFromCollectionAsync(userId, systemId);

        // Assert
        Assert.True(result);
        _mockRekognitionClient.Verify(s => s.DeleteFacesAsync(
            It.Is<DeleteFacesRequest>(r => r.CollectionId == systemId && r.FaceIds == faceIds),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockDynamoDBService.Verify(s => s.DeleteUserFromDynamoDbAsync(userId, systemId), Times.Once);
        _mockService.Verify(s => s.DeleteUserFromRekognitionCollectionAsync(systemId, userId), Times.Once);
    }

    [Fact]
    public async Task DeleteFromCollectionAsync_FaceIdsExist_NoFacesDeleted_ReturnsFalse()
    {
        // Arrange
        string userId = "testUserId";
        string systemId = "testSystemId";
        var faceIds = new List<string> { "faceId1", "faceId2" };

        _mockDynamoDBService.Setup(s => s.GetFaceIdsByUserIdAsync(userId, systemId))
            .ReturnsAsync(faceIds);

        _mockService.Setup(s => s.DisassociatedFaceAsync(systemId, It.IsAny<string>(), userId))
            .ReturnsAsync(true);

        // Include CancellationToken in Setup
        _mockRekognitionClient.Setup(s => s.DeleteFacesAsync(It.IsAny<DeleteFacesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteFacesResponse { DeletedFaces = new List<string>() });

        // Act
        var result = await _mockService.Object.DeleteFromCollectionAsync(userId, systemId);

        // Assert
        Assert.False(result);

        // Include CancellationToken in Verify
        _mockRekognitionClient.Verify(
            s => s.DeleteFacesAsync(
                It.Is<DeleteFacesRequest>(r => r.CollectionId == systemId && r.FaceIds == faceIds),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _mockDynamoDBService.Verify(s => s.DeleteUserFromDynamoDbAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockService.Verify(s => s.DeleteUserFromRekognitionCollectionAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteFromCollectionAsync_ExceptionThrown_ReturnsFalse()
    {
        // Arrange
        string userId = "testUserId";
        string systemId = "testSystemId";

        _mockDynamoDBService.Setup(s => s.GetFaceIdsByUserIdAsync(userId, systemId))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _mockService.Object.DeleteFromCollectionAsync(userId, systemId);

        // Assert
        Assert.False(result);
    }
}

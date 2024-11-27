using Xunit;
using Moq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Threading;
using System.Threading.Tasks;
using FAL.Services;
using Share.Data;
using System.Text.Json;

public class DynamoDBServiceTests
{
    private readonly Mock<IAmazonDynamoDB> _mockDynamoDBClient;
    private readonly DynamoDBService _service;

    public DynamoDBServiceTests()
    {
        _mockDynamoDBClient = new Mock<IAmazonDynamoDB>();
        _service = new DynamoDBService(_mockDynamoDBClient.Object);
    }

    [Fact]
    public async Task CreateUserInformationAsync_ValidInput_ReturnsTrue()
    {
        // Arrange
        string tableName = "TestTable";
        string userId = "testuser";
        string faceId = "testfaceid";

        _mockDynamoDBClient.Setup(client => client.PutItemAsync(
            It.IsAny<PutItemRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new PutItemResponse());

        // Act
        var result = await _service.CreateUserInformationAsync(tableName, userId, faceId);

        // Assert
        Assert.True(result);
        _mockDynamoDBClient.Verify(client => client.PutItemAsync(
            It.Is<PutItemRequest>(request =>
                request.TableName == tableName &&
                request.Item["UserId"].S == userId.ToLower() &&
                request.Item["FaceId"].S == faceId
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task CreateUserInformationAsync_PutItemThrowsException_ThrowsException()
    {
        // Arrange
        string tableName = "TestTable";
        string userId = "testuser";
        string faceId = "testfaceid";

        _mockDynamoDBClient.Setup(client => client.PutItemAsync(
            It.IsAny<PutItemRequest>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new Exception("DynamoDB error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.CreateUserInformationAsync(tableName, userId, faceId));
    }

    [Fact]
    public async Task IsExistFaceIdAsync_FaceIdExists_ReturnsTrue()
    {
        // Arrange
        string systemId = "TestTable";
        string faceId = "existingFaceId";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Count = 1,
            Items = new List<Dictionary<string, AttributeValue>>()
            {
            new Dictionary<string, AttributeValue>
            {
                { "FaceId", new AttributeValue { S = faceId } }
            }
            }
        });

        // Act
        var result = await _service.IsExistFaceIdAsync(systemId, faceId);

        // Assert
        Assert.True(result);
        _mockDynamoDBClient.Verify(client => client.QueryAsync(
            It.Is<QueryRequest>(request =>
                request.TableName == systemId &&
                request.IndexName == "FaceIdIndex" &&
                request.KeyConditionExpression == "FaceId = :faceId" &&
                request.ExpressionAttributeValues[":faceId"].S == faceId
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task IsExistFaceIdAsync_FaceIdDoesNotExist_ReturnsFalse()
    {
        // Arrange
        string systemId = "TestTable";
        string faceId = "nonExistingFaceId";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Count = 0,
            Items = new List<Dictionary<string, AttributeValue>>()
        });

        // Act
        var result = await _service.IsExistFaceIdAsync(systemId, faceId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsExistFaceIdAsync_QueryThrowsException_ThrowsException()
    {
        // Arrange
        string systemId = "TestTable";
        string faceId = "faceId";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new Exception("DynamoDB error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.IsExistFaceIdAsync(systemId, faceId));
    }

    [Fact]
    public async Task IsExistUserAsync_UserExists_ReturnsTrue()
    {
        // Arrange
        string systemId = "TestTable";
        string userId = "existingUser";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Count = 1,
            Items = new List<Dictionary<string, AttributeValue>>()
            {
            new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { S = userId.ToLower() } }
            }
            }
        });

        // Act
        var result = await _service.IsExistUserAsync(systemId, userId);

        // Assert
        Assert.True(result);
        _mockDynamoDBClient.Verify(client => client.QueryAsync(
            It.Is<QueryRequest>(request =>
                request.TableName == systemId &&
                request.KeyConditionExpression == "UserId = :v_userId" &&
                request.ExpressionAttributeValues[":v_userId"].S == userId.ToLower() &&
                request.Limit == 1
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task IsExistUserAsync_UserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        string systemId = "TestTable";
        string userId = "nonExistingUser";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Count = 0,
            Items = new List<Dictionary<string, AttributeValue>>()
        });

        // Act
        var result = await _service.IsExistUserAsync(systemId, userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsExistUserAsync_QueryThrowsException_ThrowsException()
    {
        // Arrange
        string systemId = "TestTable";
        string userId = "userId";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new Exception("DynamoDB error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.IsExistUserAsync(systemId, userId));
    }

    [Fact]
    public async Task GetRecordByKeyConditionExpressionAsync_RecordExists_ReturnsData()
    {
        // Arrange
        string systemId = "TestTable";
        string keyConditionExpression = "UserId = :userId";
        var expressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":userId", new AttributeValue { S = "testUser" } }
        };
        string expectedData = "some data";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>
            {
                new Dictionary<string, AttributeValue>
                {
                    { "Data", new AttributeValue { S = expectedData } }
                }
            }
        });

        // Act
        var result = await _service.GetRecordByKeyConditionExpressionAsync(systemId, keyConditionExpression, expressionAttributeValues);

        // Assert
        Assert.Equal(expectedData, result);
        _mockDynamoDBClient.Verify(client => client.QueryAsync(
            It.Is<QueryRequest>(request =>
                request.TableName == systemId &&
                request.KeyConditionExpression == keyConditionExpression &&
                request.ExpressionAttributeValues == expressionAttributeValues
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task GetRecordByKeyConditionExpressionAsync_RecordDoesNotExist_ReturnsNull()
    {
        // Arrange
        string systemId = "TestTable";
        string keyConditionExpression = "UserId = :userId";
        var expressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":userId", new AttributeValue { S = "nonExistingUser" } }
        };

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>()
        });

        // Act
        var result = await _service.GetRecordByKeyConditionExpressionAsync(systemId, keyConditionExpression, expressionAttributeValues);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetRecordByKeyConditionExpressionAsync_QueryThrowsException_ThrowsException()
    {
        // Arrange
        string systemId = "TestTable";
        string keyConditionExpression = "UserId = :userId";
        var expressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":userId", new AttributeValue { S = "testUser" } }
        };

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new Exception("DynamoDB error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.GetRecordByKeyConditionExpressionAsync(systemId, keyConditionExpression, expressionAttributeValues));
    }

    [Fact]
    public async Task DeleteUserInformationAsync_DeletionSuccessful_ReturnsTrue()
    {
        // Arrange
        string tableName = "TestTable";
        string userId = "testUser";
        string faceId = "testFaceId";

        _mockDynamoDBClient.Setup(client => client.DeleteItemAsync(
            It.IsAny<DeleteItemRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new DeleteItemResponse
        {
            HttpStatusCode = System.Net.HttpStatusCode.OK
        });

        // Act
        var result = await _service.DeleteUserInformationAsync(tableName, userId, faceId);

        // Assert
        Assert.True(result);
        _mockDynamoDBClient.Verify(client => client.DeleteItemAsync(
            It.Is<DeleteItemRequest>(request =>
                request.TableName == tableName &&
                request.Key["UserId"].S == userId &&
                request.Key["FaceId"].S == faceId
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task DeleteUserInformationAsync_DeletionFails_ReturnsFalse()
    {
        // Arrange
        string tableName = "TestTable";
        string userId = "testUser";
        string faceId = "testFaceId";

        _mockDynamoDBClient.Setup(client => client.DeleteItemAsync(
            It.IsAny<DeleteItemRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new DeleteItemResponse
        {
            HttpStatusCode = System.Net.HttpStatusCode.InternalServerError
        });

        // Act
        var result = await _service.DeleteUserInformationAsync(tableName, userId, faceId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteUserInformationAsync_DeleteItemThrowsException_ThrowsException()
    {
        // Arrange
        string tableName = "TestTable";
        string userId = "testUser";
        string faceId = "testFaceId";

        _mockDynamoDBClient.Setup(client => client.DeleteItemAsync(
            It.IsAny<DeleteItemRequest>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new Exception("DynamoDB error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.DeleteUserInformationAsync(tableName, userId, faceId));
    }

    [Fact]
    public async Task GetFaceIdsByUserIdAsync_UserHasFaces_ReturnsFaceIds()
    {
        // Arrange
        string userId = "testUser";
        string systemId = "TestTable";
        var expectedFaceIds = new List<string> { "faceId1", "faceId2" };

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Items = expectedFaceIds.Select(faceId => new Dictionary<string, AttributeValue>
            {
            { "FaceId", new AttributeValue { S = faceId } }
            }).ToList()
        });

        // Act
        var result = await _service.GetFaceIdsByUserIdAsync(userId, systemId);

        // Assert
        Assert.Equal(expectedFaceIds, result);
        _mockDynamoDBClient.Verify(client => client.QueryAsync(
            It.Is<QueryRequest>(request =>
                request.TableName == systemId &&
                request.KeyConditionExpression == "UserId = :userId" &&
                request.ExpressionAttributeValues[":userId"].S == userId
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task GetFaceIdsByUserIdAsync_UserHasNoFaces_ReturnsEmptyList()
    {
        // Arrange
        string userId = "testUser";
        string systemId = "TestTable";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>()
        });

        // Act
        var result = await _service.GetFaceIdsByUserIdAsync(userId, systemId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFaceIdsByUserIdAsync_QueryThrowsException_ThrowsException()
    {
        // Arrange
        string userId = "testUser";
        string systemId = "TestTable";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new Exception("DynamoDB error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.GetFaceIdsByUserIdAsync(userId, systemId));
    }

    [Fact]
    public async Task DeleteUserFromDynamoDbAsync_UserExists_DeletesItems()
    {
        // Arrange
        string userId = "testUser";
        string systemId = "TestTable";
        var itemsToDelete = new List<Dictionary<string, AttributeValue>>
        {
            new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { S = userId } },
                { "FaceId", new AttributeValue { S = "faceId1" } }
            },
            new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { S = userId } },
                { "FaceId", new AttributeValue { S = "faceId2" } }
            }
        };

        _mockDynamoDBClient.Setup(client => client.ScanAsync(
            It.IsAny<ScanRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new ScanResponse
        {
            Items = itemsToDelete
        });

        _mockDynamoDBClient.Setup(client => client.DeleteItemAsync(
            It.IsAny<DeleteItemRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new DeleteItemResponse());

        // Act
        await _service.DeleteUserFromDynamoDbAsync(userId, systemId);

        // Assert
        _mockDynamoDBClient.Verify(client => client.ScanAsync(
            It.Is<ScanRequest>(request =>
                request.TableName == systemId &&
                request.FilterExpression == "UserId = :userId" &&
                request.ExpressionAttributeValues[":userId"].S == userId
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        _mockDynamoDBClient.Verify(client => client.DeleteItemAsync(
            It.IsAny<DeleteItemRequest>(),
            It.IsAny<CancellationToken>()
        ), Times.Exactly(itemsToDelete.Count));
    }

    [Fact]
    public async Task DeleteUserFromDynamoDbAsync_UserDoesNotExist_NoDeletionOccurs()
    {
        // Arrange
        string userId = "nonExistingUser";
        string systemId = "TestTable";

        _mockDynamoDBClient.Setup(client => client.ScanAsync(
            It.IsAny<ScanRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new ScanResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>()
        });

        // Act
        await _service.DeleteUserFromDynamoDbAsync(userId, systemId);

        // Assert
        _mockDynamoDBClient.Verify(client => client.DeleteItemAsync(
            It.IsAny<DeleteItemRequest>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Fact]
    public async Task DeleteUserFromDynamoDbAsync_ScanThrowsException_HandlesException()
    {
        // Arrange
        string userId = "testUser";
        string systemId = "TestTable";

        _mockDynamoDBClient.Setup(client => client.ScanAsync(
            It.IsAny<ScanRequest>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new AmazonDynamoDBException("DynamoDB error"));

        // Act
        await _service.DeleteUserFromDynamoDbAsync(userId, systemId);

        // Assert
        // Since the method handles exceptions internally and doesn't throw, we expect no exception here.
        _mockDynamoDBClient.Verify(client => client.DeleteItemAsync(
            It.IsAny<DeleteItemRequest>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Fact]
    public async Task GetFaceIdForUserAndFaceAsync_FaceExists_ReturnsFaceId()
    {
        // Arrange
        string userId = "testUser";
        string faceId = "testFaceId";
        string tableName = "TestTable";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>
            {
                new Dictionary<string, AttributeValue>
                {
                    { "FaceId", new AttributeValue { S = faceId } }
                }
            }
        });

        // Act
        var result = await _service.GetFaceIdForUserAndFaceAsync(userId, faceId, tableName);

        // Assert
        Assert.Equal(faceId, result);
        _mockDynamoDBClient.Verify(client => client.QueryAsync(
            It.Is<QueryRequest>(request =>
                request.TableName == tableName &&
                request.KeyConditionExpression == "UserId = :userId and FaceId = :faceId" &&
                request.ExpressionAttributeValues[":userId"].S == userId &&
                request.ExpressionAttributeValues[":faceId"].S == faceId
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task GetFaceIdForUserAndFaceAsync_FaceDoesNotExist_ReturnsNull()
    {
        // Arrange
        string userId = "testUser";
        string faceId = "nonExistingFaceId";
        string tableName = "TestTable";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>()
        });

        // Act
        var result = await _service.GetFaceIdForUserAndFaceAsync(userId, faceId, tableName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetFaceIdForUserAndFaceAsync_QueryThrowsException_ThrowsException()
    {
        // Arrange
        string userId = "testUser";
        string faceId = "testFaceId";
        string tableName = "TestTable";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new Exception("DynamoDB error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.GetFaceIdForUserAndFaceAsync(userId, faceId, tableName));
    }

    [Fact]
    public async Task GetOldestFaceIdForUserAsync_UserHasFaces_ReturnsOldestFaceId()
    {
        // Arrange
        string userId = "testUser";
        string tableName = "TestTable";
        var faceItems = new List<Dictionary<string, AttributeValue>>
        {
            new Dictionary<string, AttributeValue>
            {
                { "FaceId", new AttributeValue { S = "faceId1" } },
                { "CreateDate", new AttributeValue { S = DateTime.UtcNow.AddDays(-2).ToString("o") } }
            },
            new Dictionary<string, AttributeValue>
            {
                { "FaceId", new AttributeValue { S = "faceId2" } },
                { "CreateDate", new AttributeValue { S = DateTime.UtcNow.AddDays(-1).ToString("o") } }
            }
        };

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Items = faceItems
        });

        // Act
        var result = await _service.GetOldestFaceIdForUserAsync(userId, tableName);

        // Assert
        Assert.Equal("faceId1", result);
        _mockDynamoDBClient.Verify(client => client.QueryAsync(
            It.Is<QueryRequest>(request =>
                request.TableName == tableName &&
                request.KeyConditionExpression == "UserId = :userId" &&
                request.ExpressionAttributeValues[":userId"].S == userId
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task GetOldestFaceIdForUserAsync_UserHasNoFaces_ReturnsNull()
    {
        // Arrange
        string userId = "testUser";
        string tableName = "TestTable";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>()
        });

        // Act
        var result = await _service.GetOldestFaceIdForUserAsync(userId, tableName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOldestFaceIdForUserAsync_QueryThrowsException_ThrowsException()
    {
        // Arrange
        string userId = "testUser";
        string tableName = "TestTable";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new Exception("DynamoDB error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.GetOldestFaceIdForUserAsync(userId, tableName));
    }

    [Fact]
    public async Task GetWebhookResult_ItemExists_ReturnsFaceDetectionResult()
    {
        // Arrange
        string systemId = "TestTable";
        string mediaId = "testMediaId";

        var expectedResult = new FaceDetectionResult
        {
            FileName = mediaId,
            RegisteredFaces = new List<FaceRecognitionResponse>
        {
            new FaceRecognitionResponse
            {
                FaceId = "faceId1",
                UserId = "userId1",
                TimeAppearances = "00:00:05",
                BoundingBox = new BoundingBox
                {
                    Left = 0.1f,
                    Top = 0.2f,
                    Width = 0.3f,
                    Height = 0.4f
                }
            }
        },
            UnregisteredFaces = new List<FaceRecognitionResponse>
        {
            new FaceRecognitionResponse
            {
                FaceId = "faceId2",
                UserId = null,
                TimeAppearances = "00:00:10",
                BoundingBox = new BoundingBox
                {
                    Left = 0.5f,
                    Top = 0.6f,
                    Width = 0.7f,
                    Height = 0.8f
                }
            }
        },
            Width = 1024,
            Height = 768,
            Key = "someKey"
        };

        string serializedResult = JsonSerializer.Serialize(expectedResult);

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>
            {
            new Dictionary<string, AttributeValue>
            {
                { "Data", new AttributeValue { S = serializedResult } }
            }
            }
        });

        // Act
        var result = await _service.GetWebhookResult(systemId, mediaId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult.FileName, result.FileName);
        Assert.Equal(expectedResult.Width, result.Width);
        Assert.Equal(expectedResult.Height, result.Height);
        Assert.Equal(expectedResult.Key, result.Key);

        Assert.Equal(expectedResult.RegisteredFaces.Count, result.RegisteredFaces.Count);
        Assert.Equal(expectedResult.UnregisteredFaces.Count, result.UnregisteredFaces.Count);

        // Compare individual RegisteredFaces
        for (int i = 0; i < expectedResult.RegisteredFaces.Count; i++)
        {
            var expectedFace = expectedResult.RegisteredFaces[i];
            var actualFace = result.RegisteredFaces[i];

            Assert.Equal(expectedFace.FaceId, actualFace.FaceId);
            Assert.Equal(expectedFace.UserId, actualFace.UserId);
            Assert.Equal(expectedFace.TimeAppearances, actualFace.TimeAppearances);

            if (expectedFace.BoundingBox != null && actualFace.BoundingBox != null)
            {
                Assert.Equal(expectedFace.BoundingBox.Left, actualFace.BoundingBox.Left);
                Assert.Equal(expectedFace.BoundingBox.Top, actualFace.BoundingBox.Top);
                Assert.Equal(expectedFace.BoundingBox.Width, actualFace.BoundingBox.Width);
                Assert.Equal(expectedFace.BoundingBox.Height, actualFace.BoundingBox.Height);
            }
            else
            {
                Assert.Null(actualFace.BoundingBox);
            }
        }

        // Compare individual UnregisteredFaces
        for (int i = 0; i < expectedResult.UnregisteredFaces.Count; i++)
        {
            var expectedFace = expectedResult.UnregisteredFaces[i];
            var actualFace = result.UnregisteredFaces[i];

            Assert.Equal(expectedFace.FaceId, actualFace.FaceId);
            Assert.Equal(expectedFace.UserId, actualFace.UserId);
            Assert.Equal(expectedFace.TimeAppearances, actualFace.TimeAppearances);

            if (expectedFace.BoundingBox != null && actualFace.BoundingBox != null)
            {
                Assert.Equal(expectedFace.BoundingBox.Left, actualFace.BoundingBox.Left);
                Assert.Equal(expectedFace.BoundingBox.Top, actualFace.BoundingBox.Top);
                Assert.Equal(expectedFace.BoundingBox.Width, actualFace.BoundingBox.Width);
                Assert.Equal(expectedFace.BoundingBox.Height, actualFace.BoundingBox.Height);
            }
            else
            {
                Assert.Null(actualFace.BoundingBox);
            }
        }

        _mockDynamoDBClient.Verify(client => client.QueryAsync(
            It.Is<QueryRequest>(request =>
                request.TableName == systemId &&
                request.KeyConditionExpression == "FileName = :fileName" &&
                request.ExpressionAttributeValues[":fileName"].S == mediaId.ToLower()
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }


    [Fact]
    public async Task GetWebhookResult_ItemDoesNotExist_ReturnsNull()
    {
        // Arrange
        string systemId = "TestTable";
        string mediaId = "nonExistingMediaId";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>()
        });

        // Act
        var result = await _service.GetWebhookResult(systemId, mediaId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetWebhookResult_QueryThrowsException_ReturnsNull()
    {
        // Arrange
        string systemId = "TestTable";
        string mediaId = "testMediaId";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new Exception("DynamoDB error"));

        // Act
        var result = await _service.GetWebhookResult(systemId, mediaId);

        // Assert
        Assert.Null(result);
        // You can also check if an error message was logged if your method logs errors.
    }

    [Fact]
    public async Task GetWebhookResult_InvalidJson_ReturnsNull()
    {
        // Arrange
        string systemId = "TestTable";
        string mediaId = "testMediaId";
        string invalidJson = "Invalid JSON string";

        _mockDynamoDBClient.Setup(client => client.QueryAsync(
            It.IsAny<QueryRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>
            {
                new Dictionary<string, AttributeValue>
                {
                    { "Data", new AttributeValue { S = invalidJson } }
                }
            }
        });

        // Act
        var result = await _service.GetWebhookResult(systemId, mediaId);

        // Assert
        Assert.Null(result);
        // You can also check if an error message was logged if your method logs errors.
    }
}

using Xunit;
using Moq;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FAL.Services;
using Share.SystemModel;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

public class S3ServiceTests
{
    // Helper method to create a mock IFormFile
    private IFormFile CreateMockFormFile(string fileName, string contentType, byte[] content)
    {
        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream(content);
        ms.Position = 0;

        fileMock.Setup(_ => _.FileName).Returns(fileName);
        fileMock.Setup(_ => _.Length).Returns(content.Length);
        fileMock.Setup(_ => _.ContentType).Returns(contentType);
        fileMock.Setup(_ => _.Headers).Returns(new HeaderDictionary());
        fileMock.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream, token));

        return fileMock.Object;
    }

    private IFormFile CreateMockImageFormFile(string fileName, string contentType, int width, int height)
    {
        // Create an image in memory
        using var image = new Image<Rgba32>(width, height);

        // Save the image to a memory stream
        var ms = new MemoryStream();
        image.SaveAsJpeg(ms);
        ms.Position = 0;

        // Create the mock IFormFile
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(_ => _.FileName).Returns(fileName);
        fileMock.Setup(_ => _.Length).Returns(ms.Length);
        fileMock.Setup(_ => _.ContentType).Returns(contentType);
        fileMock.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream, token));
        fileMock.Setup(_ => _.CopyTo(It.IsAny<Stream>()))
            .Callback<Stream>(stream => ms.CopyTo(stream));
        return fileMock.Object;
    }

    [Fact]
    public async Task AddFileToS3Async_SmallImageFile_UploadsSuccessfully()
    {
        // Arrange
        var mockAmazonS3 = new Mock<IAmazonS3>();
        var s3Service = new S3Service(mockAmazonS3.Object);

        var fileName = "test.jpg";
        var bucketName = "test-bucket";
        var mediaId = "media123";
        var userId = "user123";

        // Create a small image file with valid image data
        var file = CreateMockImageFormFile(fileName, "image/jpeg", 100, 100);

        // Mock the PutObjectAsync method
        mockAmazonS3.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());

        // Act
        var result = await s3Service.AddFileToS3Async(file, fileName, bucketName, TypeOfRequest.Training, mediaId, userId);

        // Assert
        Assert.True(result);

        // Verify that PutObjectAsync was called
        mockAmazonS3.Verify(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddFileToS3Async_WhenExceptionThrown_ThrowsException()
    {
        // Arrange
        var mockAmazonS3 = new Mock<IAmazonS3>();
        var s3Service = new S3Service(mockAmazonS3.Object);

        var fileName = "test.jpg";
        var bucketName = "test-bucket";
        var mediaId = "media123";
        var userId = "user123";

        // Create a small valid image file
        var file = CreateMockImageFormFile(fileName, "image/jpeg", 100, 100);

        // Mock the PutObjectAsync method to throw an exception
        mockAmazonS3.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("Test exception"));

        // Act & Assert
        await Assert.ThrowsAsync<AmazonS3Exception>(() => s3Service.AddFileToS3Async(file, fileName, bucketName, TypeOfRequest.Training, mediaId, userId));
    }


    [Fact]
    public async Task IsExistBudgetAsync_BucketExists_ReturnsTrue()
    {
        // Arrange
        var mockAmazonS3 = new Mock<IAmazonS3>();
        var s3Service = new S3Service(mockAmazonS3.Object);
        var bucketName = "existing-bucket";

        mockAmazonS3.Setup(s3 => s3.DoesS3BucketExistAsync(bucketName)).ReturnsAsync(true);

        // Act
        var result = await s3Service.IsExistBudgetAsync(bucketName);

        // Assert
        Assert.True(result);
        mockAmazonS3.Verify(s3 => s3.DoesS3BucketExistAsync(bucketName), Times.Once);
    }

    [Fact]
    public async Task IsExistBudgetAsync_BucketDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var mockAmazonS3 = new Mock<IAmazonS3>();
        var s3Service = new S3Service(mockAmazonS3.Object);
        var bucketName = "nonexistent-bucket";

        mockAmazonS3.Setup(s3 => s3.DoesS3BucketExistAsync(bucketName)).ReturnsAsync(false);

        // Act
        var result = await s3Service.IsExistBudgetAsync(bucketName);

        // Assert
        Assert.False(result);
        mockAmazonS3.Verify(s3 => s3.DoesS3BucketExistAsync(bucketName), Times.Once);
    }

    [Fact]
    public async Task IsExistBudgetAsync_WhenExceptionThrown_ThrowsException()
    {
        // Arrange
        var mockAmazonS3 = new Mock<IAmazonS3>();
        var s3Service = new S3Service(mockAmazonS3.Object);
        var bucketName = "test-bucket";

        mockAmazonS3.Setup(s3 => s3.DoesS3BucketExistAsync(bucketName))
            .ThrowsAsync(new AmazonS3Exception("Test exception"));

        // Act & Assert
        await Assert.ThrowsAsync<AmazonS3Exception>(() => s3Service.IsExistBudgetAsync(bucketName));
    }

    [Fact]
    public async Task AddBudgetAsync_ReturnsTrue()
    {
        // Arrange
        var mockAmazonS3 = new Mock<IAmazonS3>();
        var s3Service = new S3Service(mockAmazonS3.Object);
        var bucketName = "new-bucket";

        // Since the method currently does nothing and returns true, we can test that

        // Act
        var result = await s3Service.AddBudgetAsync(bucketName);

        // Assert
        Assert.True(result);
    }
}

// Supporting enums and classes used in the S3Service and tests



public enum ContentType
{
    Image,
    Video
}

public class FaceInformation
{
    public string UserId { get; set; }
}

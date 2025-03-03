﻿using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.S3;
using Amazon.SQS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Amazon.S3.Model;
using Amazon.SQS.Model;
using FAL.Controllers;
using Share.Utils;
using Amazon.DynamoDBv2;
using FAL.Services.IServices;

public class CompareControllerTests
{
    private readonly Mock<IAmazonRekognition> _mockRekognitionClient;
    private readonly Mock<IAmazonSQS> _mockSqsClient;
    private readonly Mock<IAmazonS3> _mockS3Client;
    private readonly Mock<IDynamoDBService> _mockDynamo;
    private readonly CompareController _controller;

    public CompareControllerTests()
    {
        _mockRekognitionClient = new Mock<IAmazonRekognition>();
        _mockSqsClient = new Mock<IAmazonSQS>();
        _mockS3Client = new Mock<IAmazonS3>();

        _mockDynamo = new Mock<IDynamoDBService>();
        _controller = new CompareController(
            new CustomLog("test-log-path"), // Provide a dummy logger
            _mockRekognitionClient.Object,
            _mockSqsClient.Object,
            _mockS3Client.Object,
            _mockDynamo.Object
        );
    }

    private IFormFile CreateMockFormFile(string fileName, string contentType, byte[] content)
    {
        var fileMock = new Mock<IFormFile>();
        var stream = new MemoryStream(content);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Callback<Stream, object>((s, _) => stream.CopyTo(s)).Returns(Task.CompletedTask);
        return fileMock.Object;
    }

    [Fact]
    public async Task CompareFaces_ReturnsBadRequest_WhenImagesAreMissing()
    {
        // Arrange
        var request = new CompareFaceRequest
        {
            SourceImage = null,
            TargetImage = null
        };

        // Act
        var result = await _controller.CompareFaces(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Both images must be provided.", badRequestResult.Value);
    }

    [Fact]
    public async Task CompareFaces_ReturnsOk_WhenMessageSentToQueue()
    {
        // Arrange
        var sourceImage = CreateMockFormFile("source.jpg", "image/jpeg", new byte[] { 1, 2, 3 });
        var targetImage = CreateMockFormFile("target.jpg", "image/jpeg", new byte[] { 4, 5, 6 });

        var request = new CompareFaceRequest
        {
            SourceImage = sourceImage,
            TargetImage = targetImage
        };

        _mockS3Client.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
            .ReturnsAsync(new Amazon.S3.Model.PutObjectResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK
            });

        _mockSqsClient.Setup(sqs => sqs.SendMessageAsync(It.IsAny<SendMessageRequest>(), default))
            .ReturnsAsync(new Amazon.SQS.Model.SendMessageResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK
            });

        // Act
        var result = await _controller.CompareFaces(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var anonymousObject = okResult.Value;

        // Use reflection to access the properties of the anonymous object
        var messageProperty = anonymousObject.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);

        var messageValue = messageProperty.GetValue(anonymousObject)?.ToString();
        Assert.Equal("Request sent to queue successfully.", messageValue);
    }



}

using Alumniphase2.Lambda;
using Alumniphase2.Lambda.Models;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alumniphase2.UnitTest.Lambda
{
    public class FunctionTests : Alumniphase2.Lambda.Function
    {
        private readonly Mock<IAmazonRekognition> _mockRekognitionClient;
        private readonly Mock<Function> _mockFunction;

        public FunctionTests()
        {
            _mockRekognitionClient = new Mock<IAmazonRekognition>();
            _mockFunction = new Mock<Function> { CallBase = true };
        }


        [Fact]
        public async Task DetectVideoProcess_ShouldCall_StartFaceSearch_And_GetFaceSearchResults()
        {
            // Arrange
            string bucketName = "test-bucket";
            string videoKey = "test-video.mp4";
            string expectedJobId = "test-job-id";

            var expectedResponse = new List<ResponseObj>
        {
            new ResponseObj { UserId = "user-1" },
            new ResponseObj { UserId = "user-2" }
        };

            _mockFunction
                .Setup(x => x.StartFaceSearch(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedJobId);

            // Mock GetFaceSearchResults to return a list of userIds
            _mockFunction
                .Setup(x => x.GetFaceSearchResults(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedResponse);

            // Act
            await _mockFunction.Object.DetectVideoProcess(bucketName, videoKey);

            // Assert
            _mockFunction.Verify(x => x.StartFaceSearch(bucketName, videoKey), Times.Once);
            _mockFunction.Verify(x => x.GetFaceSearchResults(expectedJobId, bucketName), Times.Once);
        }

        [Fact]
        public async Task StartFaceSearch_ShouldCall_StartFaceSearchAsync_And_Return_JobId()
        {
            // Arrange
            string bucketName = "fualumni";
            string videoFileName = "93dcc1f5-d306-4667-a9fe-9c47c5eec03a";
            string expectedJobId = "test-job-id";

            var startFaceSearchResponse = new StartFaceSearchResponse
            {
                JobId = expectedJobId
            };

            var mockRekognitionClient = new Mock<IAmazonRekognition>();
            mockRekognitionClient
                .Setup(x => x.StartFaceSearchAsync(It.IsAny<StartFaceSearchRequest>(), default))
                .ReturnsAsync(startFaceSearchResponse);

            var function = new Function
            {
                _rekognitionClient = mockRekognitionClient.Object // Inject mock RekognitionClient here
            };

            // Act
            var result = await function.StartFaceSearch(bucketName, videoFileName);

            // Assert
            mockRekognitionClient.Verify(x => x.StartFaceSearchAsync(It.Is<StartFaceSearchRequest>(req =>
                req.CollectionId == bucketName &&
                req.Video.S3Object.Bucket == bucketName &&
                req.Video.S3Object.Name == videoFileName &&
                req.FaceMatchThreshold == 80
            ), default), Times.Once);

            Assert.Equal(expectedJobId, result);
        }


        //[Fact]
        //public async Task GetFaceSearchResults_ShouldReturn_ListOfResponseObjs_WhenJobSucceeded()
        //{
        //    // Arrange
        //    string jobId = "test-job-id";
        //    string collectionId = "test-collection-id";

        //    // Mocking Rekognition API response
        //    var faceSearchResponse = new GetFaceSearchResponse
        //    {
        //        JobStatus = VideoJobStatus.SUCCEEDED,
        //        Persons = new List<PersonMatch>
        //    {
        //        new PersonMatch
        //        {
        //            Timestamp = 12345,
        //            FaceMatches = new List<FaceMatch>
        //            {
        //                new FaceMatch
        //                {
        //                    Face = new Face { FaceId = "faceId1" },
        //                    Similarity = 80
        //                }
        //            }
        //        }
        //    }
        //    };

        //    // Set up GetFaceSearchAsync to return the mocked response
        //    _mockRekognitionClient
        //        .Setup(x => x.GetFaceSearchAsync(It.IsAny<GetFaceSearchRequest>(), default))
        //        .ReturnsAsync(faceSearchResponse);

        //    // Mock the internal FindListUserIdInVideo method
        //    _mockFunction.Setup(x => x.FindListUserIdInVideo(It.IsAny<Dictionary<string, (double Confidence, long Timestamp)>>(), collectionId))
        //                 .ReturnsAsync(new List<ResponseObj>
        //                 {
        //                 new ResponseObj { UserId = "userId1" }
        //                 });

        //    // Act
        //    var result = await _mockFunction.Object.GetFaceSearchResults(jobId, collectionId);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Single(result); // We expect only one ResponseObj in the list
        //    Assert.Equal("userId1", result[0].UserId);

        //    _mockRekognitionClient.Verify(x => x.GetFaceSearchAsync(It.IsAny<GetFaceSearchRequest>(), default), Times.AtLeastOnce);
        //    _mockFunction.Verify(x => x.FindListUserIdInVideo(It.IsAny<Dictionary<string, (double Confidence, long Timestamp)>>(), collectionId), Times.Once);
        //}

        //[Fact]
        //public async Task GetFaceSearchResults_ShouldReturn_EmptyList_WhenJobFailed()
        //{
        //    // Arrange
        //    string jobId = "test-job-id";
        //    string collectionId = "test-collection-id";

        //    // Mocking Rekognition API response to simulate job failure
        //    var faceSearchResponse = new GetFaceSearchResponse
        //    {
        //        JobStatus = VideoJobStatus.FAILED
        //    };

        //    _mockRekognitionClient
        //        .Setup(x => x.GetFaceSearchAsync(It.IsAny<GetFaceSearchRequest>(), default))
        //        .ReturnsAsync(faceSearchResponse);

        //    // Act
        //    var result = await _function.GetFaceSearchResults(jobId, collectionId);

        //    // Assert
        //    Assert.Empty(result);
        //    _mockRekognitionClient.Verify(x => x.GetFaceSearchAsync(It.IsAny<GetFaceSearchRequest>(), default), Times.Once);
        //}

        //[Fact]
        //public async Task GetFaceSearchResults_ShouldHandle_Exception_AndReturn_EmptyList()
        //{
        //    // Arrange
        //    string jobId = "test-job-id";
        //    string collectionId = "test-collection-id";

        //    // Simulate an exception being thrown by Rekognition API
        //    _mockRekognitionClient
        //        .Setup(x => x.GetFaceSearchAsync(It.IsAny<GetFaceSearchRequest>(), default))
        //        .ThrowsAsync(new Exception("Rekognition API error"));

        //    // Act
        //    var result = await _function.GetFaceSearchResults(jobId, collectionId);

        //    // Assert
        //    Assert.Empty(result);
        //    _mockRekognitionClient.Verify(x => x.GetFaceSearchAsync(It.IsAny<GetFaceSearchRequest>(), default), Times.Once);
        //}
    }
}

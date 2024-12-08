using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using AuthenExamCompareFaceExam.ExternalService;
using AuthenExamCompareFaceExam;

namespace AuthenExam.UnitTests
{
    [TestFixture]
    public class FaceCompareServiceTests
    {
        private FaceCompareService _faceCompareService;
        string sourceImage;
        string targetImage;

        [SetUp]
        public void Setup()
        {
            var authService = new AuthService(
                "https://dev.demorecognition.click/api/Auth/login",
                "string", // Username
                "123456"  // Password
            );

            _faceCompareService = new FaceCompareService(
                authService,
                "https://dev.demorecognition.click/api/Compare/compare/result"
            );
            sourceImage = @"C:\Users\Kha\Documents\source.jpg";
            targetImage = @"C:\Users\Kha\Documents\target.jpg";
        }

        [Test]
        public async Task CompareFacesAsync_ValidImages_ReturnsSuccessResponse()
        {
            // Arrange

            Assert.IsTrue(File.Exists(sourceImage), $"Source image not found at {sourceImage}");
            Assert.IsTrue(File.Exists(targetImage), $"Target image not found at {targetImage}");

            // Act
            var result = await _faceCompareService.CompareFacesAsync(sourceImage, targetImage);

            // Assert
            Assert.NotNull(result, "Result should not be null.");
            Assert.AreEqual(200, result.Status, "Status should be 200 for successful comparison.");
            Assert.NotNull(result.Data, "Data should not be null for successful comparison.");
            Console.WriteLine($"Comparison Result: {result.Data}");
        }

        [Test]
        public async Task CompareFacesAsync_InvalidImages_ReturnsErrorResponse()
        {
            // Arrange

            Assert.IsTrue(File.Exists(sourceImage), $"Source image not found at {sourceImage}");
            Assert.IsTrue(File.Exists(targetImage), $"Target image not found at {targetImage}");

            // Act
            var result = await _faceCompareService.CompareFacesAsync(sourceImage, targetImage);

            // Assert
            Assert.NotNull(result, "Result should not be null.");
            Assert.AreNotEqual(200, result.Status, "Status should not be 200 for invalid comparison.");
            Assert.IsNull(result.Data, "Data should be null for invalid comparison.");
            Console.WriteLine($"Error Message: {result.Message}");
        }
    }
}

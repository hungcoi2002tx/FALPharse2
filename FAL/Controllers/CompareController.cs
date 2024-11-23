using Amazon.Rekognition.Model;
using Amazon.Rekognition;
using Amazon.S3;
using FAL.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Share.SystemModel;
using System.IO.Compression;
using Amazon.SQS.Model;
using Amazon.SQS;
using Newtonsoft.Json;
using Amazon.S3.Model;

namespace FAL.Controllers
{

    public class CompareFaceRequest
    {
        public IFormFile SourceImage { get; set; }
        public IFormFile TargetImage { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class CompareController : ControllerBase
    {
        private readonly IAmazonRekognition _rekognitionClient;
        private readonly IAmazonS3 _s3Client;
        private readonly CustomLog _logger;
        private readonly string SystermId = GlobalVarians.SystermId;
        private readonly IAmazonSQS _sqsClient;

        public CompareController(CustomLog logger, IAmazonRekognition rekognitionClient, IAmazonSQS sqsClient, IAmazonS3 s3Service)
        {
            _logger = logger;
            _rekognitionClient = rekognitionClient;
            _sqsClient = sqsClient;
            _s3Client = s3Service;
        }
        [HttpPost("compare")]
        public async Task<IActionResult> CompareFaces([FromForm] CompareFaceRequest request)
        {
            if (request.SourceImage == null || request.TargetImage == null)
            {
                return BadRequest("Both images must be provided.");
            }

            if (!IsImageFile(request.SourceImage) || !IsImageFile(request.TargetImage))
            {
                return BadRequest("Both files must be valid images.");
            }

            // Convert images to byte arrays
            var sourceImageBytes = await ConvertToBytes(request.SourceImage);
            var targetImageBytes = await ConvertToBytes(request.TargetImage);

            // Extract the file name without extension
            var sourceFileName = Path.GetFileNameWithoutExtension(request.SourceImage.FileName);
            var targetFileName = Path.GetFileNameWithoutExtension(request.TargetImage.FileName);

            // Get content type from the uploaded files
            var sourceContentType = request.SourceImage.ContentType;
            var targetContentType = request.TargetImage.ContentType;

            // Upload images to S3 with "si" and "ti" suffixes
            var sourceImageUrl = await UploadImageToS3(sourceImageBytes, sourceFileName, "si", sourceContentType);
            var targetImageUrl = await UploadImageToS3(targetImageBytes, targetFileName, "ti", targetContentType);

            // Prepare the message for SQS with the S3 URLs
            var message = new
            {
                SourceImageUrl = sourceImageUrl,
                TargetImageUrl = targetImageUrl,
                SimilarityThreshold = 80F
            };

            var sqsMessage = new SendMessageRequest
            {
                QueueUrl = "https://sqs.ap-southeast-1.amazonaws.com/257394496117/CompareFaces",  // Update with your SQS queue URL
                MessageBody = JsonConvert.SerializeObject(message)
            };

            try
            {
                // Send the message to SQS
                var sendMessageResponse = await _sqsClient.SendMessageAsync(sqsMessage);

                if (sendMessageResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(new { message = "Request sent to queue successfully." });
                }
                else
                {
                    return StatusCode(500, "Failed to send message to queue.");
                }
            }
            catch (AmazonSQSException ex)
            {
                return StatusCode(500, $"Error sending message to SQS: {ex.Message}");
            }
        }

        [HttpPost("compare/result")]
        public async Task<IActionResult> CompareFacesReturnResult([FromForm] CompareFaceRequest request)
        {
            // Validate if both images are provided
            if (request.SourceImage == null || request.TargetImage == null)
            {
                return BadRequest("Both images must be provided.");
            }

            // Validate file types
            if (!IsImageFile(request.SourceImage) || !IsImageFile(request.TargetImage))
            {
                return BadRequest("Both files must be valid images.");
            }

            try
            {
                // Read IFormFile into byte arrays
                byte[] sourceImageBytes;
                byte[] targetImageBytes;

                using (var sourceStream = new MemoryStream())
                {
                    await request.SourceImage.CopyToAsync(sourceStream);
                    sourceImageBytes = sourceStream.ToArray();
                }

                using (var targetStream = new MemoryStream())
                {
                    await request.TargetImage.CopyToAsync(targetStream);
                    targetImageBytes = targetStream.ToArray();
                }

                // Create CompareFacesRequest object
                var compareFacesRequest = new Amazon.Rekognition.Model.CompareFacesRequest
                {
                    SourceImage = new Amazon.Rekognition.Model.Image
                    {
                        Bytes = new MemoryStream(sourceImageBytes)
                    },
                    TargetImage = new Amazon.Rekognition.Model.Image
                    {
                        Bytes = new MemoryStream(targetImageBytes)
                    },
                    SimilarityThreshold = 80 // Default to 80% similarity if not provided
                };

                // Call AWS Rekognition CompareFaces API
                var response = await _rekognitionClient.CompareFacesAsync(compareFacesRequest);

                // Prepare the result
                var results = response.FaceMatches.Select(match => new
                {
                    Similarity = match.Similarity,
                    BoundingBox = match.Face.BoundingBox
                });

                return Ok(new
                {
                    Status = response.FaceMatches.Any() && response.FaceMatches.Max(match => match.Similarity) >= 80,
                    Percentage = response.FaceMatches.Any()
         ? response.FaceMatches.Max(match => match.Similarity)
         : (float?)null // Return null if no matches
                });
            }
            catch (AmazonRekognitionException ex)
            {
                // Handle AWS Rekognition exceptions
                return StatusCode(500, $"AWS Rekognition error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle generic exceptions
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private bool IsImageFile(IFormFile file)
        {
            return file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }

        // Helper method to upload images to S3 with a unique key
        private async Task<string> UploadImageToS3(byte[] imageBytes, string fileName, string suffix, string contentType)
        {
            // Generate a unique key using GUID
            var uniqueFileName = $"{suffix}_{Guid.NewGuid()}";
  // You can also use contentType's extension if preferred

            // Create a PutObjectRequest with the unique key
            var putRequest = new PutObjectRequest
            {
                BucketName = "comparefacestorage",  // Replace with your bucket name
                Key = uniqueFileName,
                InputStream = new MemoryStream(imageBytes),
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(putRequest);

            // Construct and return the S3 URL
            return $"https://{putRequest.BucketName}.s3.amazonaws.com/{uniqueFileName}";
        }

        private async Task<byte[]> ConvertToBytes(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public class UserTrainDTO
        {
            public string UserId { get; set; }
            public string ImageName { get; set; }
        }
    }
}

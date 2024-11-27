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
using System.Net;

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
            var response = new CompareResponseResult();

            try
            {
                // Validate input
                if (!IsValidRequest(request, out var validationMessage))
                {
                    return CreateResponse(null, validationMessage, HttpStatusCode.BadRequest);
                }

                // Read image data
                byte[] sourceImageBytes = await GetImageBytesAsync(request.SourceImage);
                byte[] targetImageBytes = await GetImageBytesAsync(request.TargetImage);

                // Create and send CompareFacesRequest
                var rekognitionResponse = await _rekognitionClient.CompareFacesAsync(new Amazon.Rekognition.Model.CompareFacesRequest
                {
                    SourceImage = new Amazon.Rekognition.Model.Image { Bytes = new MemoryStream(sourceImageBytes) },
                    TargetImage = new Amazon.Rekognition.Model.Image { Bytes = new MemoryStream(targetImageBytes) },
                    SimilarityThreshold = 80
                });

                // Process response
                var maxSimilarity = rekognitionResponse.FaceMatches.Any()
                    ? rekognitionResponse.FaceMatches.Max(match => match.Similarity)
                    : (float?)null;

                var message = maxSimilarity.HasValue && maxSimilarity >= 80
                    ? "Faces matched successfully."
                    : "No matching faces found.";

                return CreateResponse(maxSimilarity, message, HttpStatusCode.OK);
            }
            catch (AmazonRekognitionException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests || ex.Message.Contains("Limit Exceeded"))
            {
                return CreateResponse(null, "Rate limit exceeded or request limit exceeded. Please try again later.", HttpStatusCode.TooManyRequests);
            }
            catch (AmazonRekognitionException ex)
            {
                var statusCode = ex.StatusCode != 0 ? ex.StatusCode : HttpStatusCode.InternalServerError;

                return CreateResponse(
                    null,
                    $"AWS Rekognition error: {ex.Message}",
                    statusCode
                );
            }
            catch (Exception ex)
            {
                return CreateResponse(null, $"An error occurred: {ex.Message}", HttpStatusCode.InternalServerError);
            }
        }

        private bool IsValidRequest(CompareFaceRequest request, out string message)
        {
            if (request.SourceImage == null || request.TargetImage == null)
            {
                message = "Both images must be provided.";
                return false;
            }

            if (!IsImageFile(request.SourceImage) || !IsImageFile(request.TargetImage))
            {
                message = "Both files must be valid images.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private async Task<byte[]> GetImageBytesAsync(IFormFile image)
        {
            using var stream = new MemoryStream();
            await image.CopyToAsync(stream);
            return stream.ToArray();
        }

        private IActionResult CreateResponse(float? percentage, string message, HttpStatusCode statusCode)
        {
            var response = new CompareResponseResult
            {
                Percentage = percentage,
                Message = message,
            };

            return StatusCode((int)statusCode, response);
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

using Amazon.Rekognition.Model;
using Amazon.Rekognition;
using Amazon.S3;
using FAL.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Share.SystemModel;

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
        private readonly CustomLog _logger;
        private readonly string SystermId = GlobalVarians.SystermId;

        public CompareController(CustomLog logger, IAmazonRekognition rekognitionClient)
        {
            _logger = logger;
            _rekognitionClient = rekognitionClient;
        }
        [HttpPost("compare")]
        public async Task<IActionResult> CompareFaces([FromForm] CompareFaceRequest request)
        {
            if (request.SourceImage == null || request.TargetImage == null)
            {
                return BadRequest("Both images must be provided.");
            }

            var sourceImageBytes = await ConvertToBytes(request.SourceImage);
            var targetImageBytes = await ConvertToBytes(request.TargetImage);

            var compareFacesRequest = new CompareFacesRequest
            {
                SourceImage = new Image
                {
                    Bytes = new MemoryStream(sourceImageBytes)
                },
                TargetImage = new Image
                {
                    Bytes = new MemoryStream(targetImageBytes)
                },
                SimilarityThreshold = 80F
            };

            try
            {
                var compareFacesResponse = await _rekognitionClient.CompareFacesAsync(compareFacesRequest);

                if (compareFacesResponse.FaceMatches.Count > 0)
                {
                    var similarity = compareFacesResponse.FaceMatches[0].Similarity;
                    return Ok(new { message = "Faces matched", similarity });
                }
                else
                {
                    return Ok(new { message = "No match found" });
                }
            }
            catch (AmazonRekognitionException ex)
            {
                return StatusCode(500, $"Error calling AWS Rekognition: {ex.Message}");
            }
        }

        private async Task<byte[]> ConvertToBytes(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}

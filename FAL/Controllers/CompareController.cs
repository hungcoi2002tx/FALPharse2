using Amazon.Rekognition.Model;
using Amazon.Rekognition;
using Amazon.S3;
using FAL.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Share.SystemModel;
using System.IO.Compression;

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

        [HttpPost("upload-zip")]
        public async Task<IActionResult> UploadAndProcessZipFile(IFormFile zipFile)
        {
            if (zipFile == null || zipFile.Length == 0)
            {
                return BadRequest("No ZIP file uploaded.");
            }

            if (!Path.GetExtension(zipFile.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only ZIP files are supported.");
            }

            try
            {
                var tempZipFilePath = Path.GetTempFileName();
                using (var stream = new FileStream(tempZipFilePath, FileMode.Create))
                {
                    await zipFile.CopyToAsync(stream);
                }

                var extractPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(extractPath);
                ZipFile.ExtractToDirectory(tempZipFilePath, extractPath);

                var userTrainList = new List<UserTrainDTO>();
                var imageFiles = Directory.GetFiles(extractPath)
                    .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase));

                foreach (var imageFile in imageFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(imageFile);
                    var imageName = Path.GetFileName(imageFile);

                    userTrainList.Add(new UserTrainDTO
                    {
                        UserId = fileName,
                        ImageName = imageName
                    });
                }

                System.IO.File.Delete(tempZipFilePath);
                Directory.Delete(extractPath, true);

                return Ok(userTrainList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing the ZIP file: {ex.Message}");
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

        public class UserTrainDTO
        {
            public string UserId { get; set; }
            public string ImageName { get; set; }
        }
    }
}

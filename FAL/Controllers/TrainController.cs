using Amazon.Rekognition.Model;
using Amazon.Runtime;
using Amazon.S3;
using FAL.Services;
using FAL.Services.IServices;
using FAL.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Share.Data;
using Share.DTO;
using Share.SystemModel;
using System.IO.Compression;
using System.Net;
using System.Reflection;

namespace FAL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainController : ControllerBase
    {
        private readonly IS3Service _s3Service;
        private readonly ICollectionService _collectionService;
        private readonly IDynamoDBService _dynamoService;
        private readonly IWebHostEnvironment _environment;
        private readonly CustomLog _logger;
        private readonly string SystermId = GlobalVarians.SystermId;

        public TrainController(IAmazonS3 s3Client,
            CustomLog logger,
            ICollectionService collectionService,
            IS3Service s3Service, IDynamoDBService
            dynamoService, IWebHostEnvironment environment)
        {
            _logger = logger;
            _collectionService = collectionService;
            _s3Service = s3Service;
            _dynamoService = dynamoService;
            _environment = environment;
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteByUserIdAsync([FromBody] string userId)
        {
            try
            {
                var result = await _collectionService.DeleteByUserIdAsync(userId, SystermId);
                if (result) return Ok(new { Status = true });
                return BadRequest(new { Status = false });
            }
            catch (Exception ex)
            {
                _logger.LogException($"{MethodBase.GetCurrentMethod().Name} - {GetType().Name}", ex);
                return StatusCode(500, new ResultResponse
                {
                    Status = false,
                    Message = "Internal Server Error"
                });
            }
        }

        [Authorize]
        [HttpDelete("Reset")]
        public async Task<IActionResult> ResetByUserId([FromBody] UserIdRequest userId)
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == SystermId).Value;
                // Step 1: Delete user from SQL collection
                var collectionDeleted = await _collectionService.DeleteFromCollectionAsync(userId.UserId, systermId);

                // Check if both deletions were successful
                if (collectionDeleted)
                    return Ok(new { Status = true });

                return BadRequest(new { Status = false });
            }
            catch (Exception ex)
            {
                _logger.LogException($"{MethodBase.GetCurrentMethod().Name} - {GetType().Name}", ex);
                return StatusCode(500, new ResultResponse
                {
                    Status = false,
                    Message = "Internal Server Error"
                });
            }
        }

        [Authorize]
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
                // Save the ZIP file to a temporary location
                var tempZipFilePath = Path.GetTempFileName();
                using (var stream = new FileStream(tempZipFilePath, FileMode.Create))
                {
                    await zipFile.CopyToAsync(stream);
                }

                // Extract ZIP file to a temporary folder
                var extractPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(extractPath);
                ZipFile.ExtractToDirectory(tempZipFilePath, extractPath);

                // Find image files in the extracted folder
                var imageFiles = Directory.GetFiles(extractPath)
                    .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var systemId = User.Claims.FirstOrDefault(c => c.Type == SystermId)?.Value;

                int successCount = 0;
                int failureCount = 0;

                foreach (var imageFile in imageFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(imageFile);
                    var imageName = Path.GetFileName(imageFile);

                    // Read the image file and validate it
                    using (var imageStream = new FileStream(imageFile, FileMode.Open, FileAccess.Read))
                    {
                        var formFile = new FormFile(imageStream, 0, imageStream.Length, null, imageName)
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = GetContentType(imageFile)
                        };

                        try
                        {
                            // Validate the file with Rekognition
                            await ValidateFileWithRekognitionAsync(formFile);

                            // Train the system for each user with the image
                            var image = await GetImageAsync(formFile);
                            await TrainAsync(image, fileName, systemId);
                            successCount++; // Increment success count
                        }
                        catch (Exception ex)
                        {
                            // Log the exception for this specific image
                            _logger.LogException($"{MethodBase.GetCurrentMethod().Name} - {GetType().Name}", ex);
                            failureCount++; // Increment failure count
                        }
                    }
                }

                // Cleanup
                System.IO.File.Delete(tempZipFilePath);
                Directory.Delete(extractPath, true);

                return Ok(new ResultResponse
                {
                    Status = true,
                    Message = $"Training completed. Success: {successCount}, Failed: {failureCount}."
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogException($"{MethodBase.GetCurrentMethod().Name} - {GetType().Name}", ex);
                return StatusCode(400, new ResultResponse
                {
                    Status = false,
                    Message = "Bad Request. Invalid value."
                });
            }
            catch (Exception ex)
            {
                _logger.LogException($"{MethodBase.GetCurrentMethod().Name} - {GetType().Name}", ex);
                return StatusCode(500, new ResultResponse
                {
                    Status = false,
                    Message = "Internal Server Error"
                });
            }
        }


        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
    {
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" }
    };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        [HttpPost("/Check/IsTrained")]
        public async Task<IActionResult> CheckIsTrained(string userId)
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == SystermId).Value;
                var response = await _dynamoService.GetFaceIdsByUserIdAsync(userId, systermId);
                //return 
                return Ok(new ResultIsTrainedModel
                {
                    Status = true,
                    IsTrained = response.Count > 0,
                    Message = "The system training was successful."
                });
            }
            catch (Exception ex)
            {
                _logger.LogException($"{MethodBase.GetCurrentMethod().Name} - {GetType().Name}", ex);
                return StatusCode(500, new ResultIsTrainedModel
                {
                    Status = false,
                    IsTrained= false,
                    Message = "Internal Server Error"
                });
            }
        }

            [HttpPost("file")]
        public async Task<IActionResult> TrainByImageAsync(IFormFile file, string userId)
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == SystermId).Value;
                await ValidateFileWithRekognitionAsync(file);
                var image = await GetImageAsync(file);
                await TrainAsync(image, userId, systermId);
                //return 
                return Ok(new ResultResponse
                {
                    Status = true,
                    Message = "The system training was successful."
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogException($"{MethodBase.GetCurrentMethod().Name} - {GetType().Name}", ex);
                return StatusCode(400, new ResultResponse
                {
                    Status = false,
                    Message = "Bad Request. Invalid value."
                });
            }
            catch (Exception ex)
            {
                _logger.LogException($"{MethodBase.GetCurrentMethod().Name} - {GetType().Name}", ex);
                return StatusCode(500, new ResultResponse
                {
                    Status = false,
                    Message = "Internal Server Error"
                });
            }
        }

        [HttpPost("url")]
        public async Task<IActionResult> TrainByUrlAsync([FromBody] TrainModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Data) || string.IsNullOrEmpty(model.UserId))
                {
                    throw new ArgumentException(message: "Image URL and User ID cannot be null or empty.");
                }
                var systermId = User.Claims.FirstOrDefault(c => c.Type == SystermId)?.Value;
                if(systermId == null)
                {
                    throw new ArgumentException(message: "Not exist system");
                }
                byte[] imageBytes = null;
                try
                {
                    imageBytes =  DownloadImageAsync(model.Data); // Await the asynchronous method
                    if (imageBytes == null || imageBytes.Length == 0)
                    {
                        throw new ArgumentException("Downloaded image is empty or null.");
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    // Check for specific HTTP status code (Forbidden 403)
                    if (httpEx.Message.Contains("403"))
                    {
                        return BadRequest(new ResultResponse
                        {
                            Status = false,
                            Message = $"Access to the image URL is forbidden (HTTP 403). Please check the URL permissions : {httpEx.Message}"
                        });
                    }
                    // Log the HttpRequestException in case of network issues
                    return BadRequest(new ResultResponse
                    {
                        Status = false,
                        Message = $"Failed to download image. Please check the image URL and try again: {httpEx.Message}"
                    });
                }
                catch (Exception ex)
                {
                    // Catch other exceptions
                    return BadRequest(new ResultResponse
                    {
                        Status = false,
                        Message = $"An error occurred while processing the image: {ex.Message}"
                    });
                }
                if (! await CheckValidImageByByte(imageBytes))
                {
                    throw new ArgumentException(message: "Invalid image format or size or many face.");
                }

                using (var imageStream = new MemoryStream(imageBytes))
                {
                    await TrainAsync(new Image { Bytes = imageStream }, model.UserId, systermId);
                }

                return Ok(new ResultResponse
                {
                    Status = true,
                    Message = "The system training was successful."
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogException($"{MethodBase.GetCurrentMethod().Name} - {GetType().Name}", ex);
                return BadRequest(new ResultResponse
                {
                    Status = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogException($"{MethodBase.GetCurrentMethod().Name} - {GetType().Name}", ex);
                return StatusCode(500, new ResultResponse
                {
                    Status = false,
                    Message = "Internal Server Error"
                });
            }
        }

        async Task<bool> CheckValidImageByByte(byte[] imageBytes)
        {
            try
            {
                return imageBytes.Length != 0 && imageBytes.Length > 15 * 1024 * 1024 && (imageBytes.IsJpeg() || imageBytes.IsPng()) && await CheckValidImageByByte(imageBytes);
            }
            catch (Exception)
            {

                throw;
            }
        }


        async Task<bool> CheckValidImageByRecognition(byte[] imageBytes)
        {
            try
            {
                Image image;
                using (var imageStream = new MemoryStream(imageBytes))
                {
                    image = new Image() { Bytes = imageStream };
                }
                var response = await _collectionService.DetectFaceByFileAsync(image);
                if (response.FaceDetails.Count != 1)
                {
                    throw new ArgumentException(message: "File ảnh yêu cầu duy nhất 1 mặt");
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

         byte[] DownloadImageAsync(string urlAvatar)
        {
            try
            {

                if (!string.IsNullOrEmpty(urlAvatar))
                {
                    int lastIndImg = urlAvatar.LastIndexOf(".");
                    string extImg = urlAvatar.Substring(lastIndImg);
                    var localAvatar = "HL" + "." + "HE170642" + extImg;

                    //var dir = Directory.GetCurrentDirectory();
                    //string fullFolderPath = Path.Combine(_environment.ContentRootPath, "UploadedFiles");
                    //string fullFilePath = Path.Combine(fullFolderPath, localAvatar);
                    //if (!System.IO.Directory.Exists(fullFolderPath))
                    //{
                    //    System.IO.Directory.CreateDirectory(fullFolderPath);
                    //}
                    //if (!System.IO.File.Exists(fullFilePath))
                    //{
                       
                    //}
                    return GetFileFromUrl("", urlAvatar);

                }
                //using (var httpClient = new HttpClient())
                //{
                //    return await httpClient.GetByteArrayAsync(url);
                //}
                return Array.Empty<byte>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        byte[] GetFileFromUrl(string fileName, string url)
        {
            byte[] content;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            WebResponse response = request.GetResponse();

            Stream stream = response.GetResponseStream();
            using (BinaryReader reader = new BinaryReader(stream))
            {
                content = reader.ReadBytes(500000);
                reader.Close();
            }
            response.Close();
            FileStream fs = new FileStream(fileName, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            try
            {
                bw.Write(content);
                return content;
            }
            finally
            {
                bw.Close();
                fs.Close();
            }
        }



        [HttpPost("faceId")]
        public async Task<IActionResult> TrainByFaceIdAsync([FromBody] FaceTrainModel info)
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == SystermId).Value;
                //check faceId in dynamodb
                var result = await _dynamoService.IsExistFaceIdAsync(systermId, info.FaceId);
                if (result)
                {
                    return BadRequest(new ResultResponse
                    {
                        Status = false,
                        Message = "FaceId is existed in systerm"
                    });
                }

                //train
                await TrainFaceIdAsync(info.UserId, info.FaceId, systermId);

                //return 
                return Ok(new ResultResponse
                {
                    Status = true,
                    Message = "The system training was successful."
                });
            }
            catch (Exception ex)
            {
                _logger.LogException($"{MethodBase.GetCurrentMethod().Name} - {GetType().Name}", ex);
                return StatusCode(500, new ResultResponse
                {
                    Status = false,
                    Message = "Internal Server Error"
                });
            }
        }

        [HttpPost("disassociate")]
        public async Task<IActionResult> DisassociateByFaceIdAsync([FromBody] FaceTrainModel info)
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == SystermId).Value;
                //check faceId in dynamodb
                var result = await _dynamoService.IsExistFaceIdAsync(systermId, info.FaceId);
                if (!result)
                {
                    return BadRequest(new ResultResponse
                    {
                        Status = false,
                        Message = "FaceId is not existed in systerm"
                    });
                }

                //train
                await DisassociateFaceIdAsync(info.UserId, info.FaceId, systermId);

                //return 
                return Ok(new ResultResponse
                {
                    Status = true,
                    Message = "The disassociate was successful."
                });
            }
            catch (Exception ex)
            {
                _logger.LogException($"{MethodBase.GetCurrentMethod().Name} - {GetType().Name}", ex);
                return StatusCode(500, new ResultResponse
                {
                    Status = false,
                    Message = "Internal Server Error"
                });
            }
        }

        private async Task<bool> ValidateFileWithRekognitionAsync(IFormFile file)
        {
            try
            {
                file.ValidImage();
                var response = await _collectionService.DetectFaceByFileAsync(file);
                if (response.FaceDetails.Count != 1)
                {
                    throw new ArgumentException(message: "File ảnh yêu cầu duy nhất 1 mặt");
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task TrainAsync(Image file, string userId, string systermId)
        {
            try
            {
                var collectionExits = await _collectionService.IsCollectionExistAsync(systermId);
                if (!collectionExits)
                {
                    await _collectionService.CreateCollectionAsync(systermId);
                }

                #region index face
                var indexResponse = await _collectionService.IndexFaceByFileAsync(file, systermId, userId);
                #endregion
                await TrainFaceIdAsync(userId, indexResponse.FaceRecords[0].Face.FaceId, systermId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task TrainFaceIdAsync(string userId, string faceId, string systermId)
        {
            try
            {
                #region check exit UserId
                var isExitUser = await _dynamoService.IsExistUserAsync(systermId, userId);
                #endregion
                if (!isExitUser)
                {
                    await _collectionService.CreateNewUserAsync(systermId, userId);
                }
                #region Add user 
                await _collectionService.AssociateFacesAsync(systermId, new List<string>() { faceId }, userId);
                await _dynamoService.CreateUserInformationAsync(systermId, userId, faceId);
                #endregion
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task DisassociateFaceIdAsync(string userId, string faceId, string systermId)
        {
            try
            {
                #region check exit UserId
                var isUserExist = await _dynamoService.IsExistUserAsync(systermId, userId);
                #endregion
                if (!isUserExist)
                {
                    throw new InvalidOperationException("User does not exist, so face cannot be disassociated.");
                }
                #region Add user 
                await _collectionService.DisassociatedFaceAsync(systermId, faceId, userId);
                await _collectionService.DeleteUserFromRekognitionCollectionAsync(systermId, userId);
                await _dynamoService.DeleteUserInformationAsync(systermId, userId, faceId);
                #endregion
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<Image> GetImageAsync(IFormFile file)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                return new Image
                {
                    Bytes = memoryStream
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

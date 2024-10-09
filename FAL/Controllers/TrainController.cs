using Amazon.Rekognition.Model;
using Amazon.S3;
using FAL.Services.IServices;
using FAL.Utils;
using Microsoft.AspNetCore.Mvc;
using Share.SystemModel;
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
        private readonly CustomLog _logger;
        private readonly string SystermId = GlobalVarians.SystermId;

        public TrainController(IAmazonS3 s3Client,
            CustomLog logger,
            ICollectionService collectionService,
            IS3Service s3Service, IDynamoDBService
            dynamoService)
        {
            _logger = logger;
            _collectionService = collectionService;
            _s3Service = s3Service;
            _dynamoService = dynamoService;
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteByUserIdAsync([FromBody]string userId)
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

        [HttpPost("file")]
        public async Task<IActionResult> TrainByImageAsync(IFormFile file, string userId)
        {
            try
            {
                await ValidateFileWithRekognitionAsync(file);
                var image = await GetImageAsync(file);
                await TrainAsync(image, userId);
                //return 
                return Ok(new ResultResponse
                {
                    Status = true,
                    Message = "The system training was successful."
                });
            }
            catch(ArgumentException ex)
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

        [HttpPost("faceId")]
        public async Task<IActionResult> TrainByFaceIdAsync(string faceId, string userId)
        {
            try
            {
                //check faceId in dynamodb
                var result = await _dynamoService.IsExistFaceIdAsync(SystermId, faceId);
                if (result)
                {
                    return BadRequest(new ResultResponse
                    {
                        Status = false,
                        Message = "FaceId is existed in systerm"
                    });
                }

                //train
                await TrainFaceIdAsync(userId, faceId);

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

        private async Task TrainAsync(Image file, string userId)
        {
            try
            {
                #region lay thong tin tu token companyid

                #endregion
                var collectionExits = await _collectionService.IsCollectionExistAsync(SystermId);
                if (!collectionExits)
                {
                    await _collectionService.CreateCollectionAsync(SystermId);
                }

                #region index face
                var indexResponse = await _collectionService.IndexFaceByFileAsync(file, SystermId, userId);
                #endregion
                await TrainFaceIdAsync(userId, indexResponse.FaceRecords[0].Face.FaceId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task TrainFaceIdAsync(string userId, string faceId)
        {
            try
            {
                #region check exit UserId
                var isExitUser = await _dynamoService.IsExistUserAsync(SystermId, userId);
                #endregion
                if (!isExitUser)
                {
                    await _collectionService.CreateNewUserAsync(SystermId, userId);
                }
                #region Add user 
                await _collectionService.AssociateFacesAsync(SystermId, new List<string>() { faceId }, userId);
                await _dynamoService.CreateUserInformationAsync(SystermId, userId, faceId);
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

        private async Task AssosiateFaceWithUserAsync(Face face, string key)
        {
            try
            {
                await _collectionService.CreateNewUserAsync(SystermId, key);
                await _collectionService.AssociateFacesAsync(SystermId, new List<string>() { face.FaceId }, key);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

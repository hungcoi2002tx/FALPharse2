using Amazon.Rekognition.Model;
using Amazon.S3;
using FAL.Services.IServices;
using FAL.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Share.Data;
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

        [Authorize]
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
        public async Task<IActionResult> TrainByFaceIdAsync([FromBody] FaceTrainModel info )
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
                await TrainFaceIdAsync(userId, indexResponse.FaceRecords[0].Face.FaceId,systermId);
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

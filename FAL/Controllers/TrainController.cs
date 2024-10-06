using Amazon.Rekognition.Model;
using Amazon.S3;
using FAL.Services.IServices;
using FAL.Utils;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> DeleteByUserIdAsync(string userId)
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
                return BadRequest(new { Status = false, Messange = ex.Message });
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFileAsync(IFormFile file, string userId)
        {
            try
            {
                await ValidateFileWithRekognitionAsync(file);
                var image = await GetImageAsync(file);
                await TrainAsync(image, userId);
                return Content("Train succesfully");
            }
            catch (Exception ex)
            {
                _logger.LogException($"{MethodBase.GetCurrentMethod().Name} - {GetType().Name}", ex);
                return BadRequest(new { Status = false, Messange = ex.Message });
            }
        }

        private async Task<bool> ValidateFileWithRekognitionAsync(IFormFile file)
        {
            try
            {
                var response = await _collectionService.DetectFaceByFileAsync(file);
                if (response.FaceDetails.Count != 1)
                {
                    throw new Exception(message: "File ảnh yêu cầu duy nhất 1 mặt");
                }
                return file.ValidFile();
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

                #region check exit UserId
                var isExitUser = await _dynamoService.IsExitUserAsync(SystermId, userId);
                #endregion

                if (!isExitUser)
                {
                    await _collectionService.CreateNewUserAsync(SystermId, userId);
                }

                #region Add user 
                await _collectionService.AssociateFacesAsync(SystermId, new List<string>() { indexResponse.FaceRecords[0].Face.FaceId }, userId);
                await _dynamoService.CreateUserInformationAsync(SystermId, userId, indexResponse.FaceRecords[0].Face.FaceId);
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

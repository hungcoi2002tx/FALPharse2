using Amazon.Rekognition.Model;
using Amazon.S3;
using FAL.Services.IServices;
using FAL.Utils;
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
        private readonly CustomLog _logger;
        private readonly string SystermId = "FUALUMNI";

        public TrainController(IAmazonS3 s3Client, CustomLog logger, ICollectionService collectionService, IS3Service s3Service)
        {
            _logger = logger;
            _collectionService = collectionService;
            _s3Service = s3Service;
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

        [HttpPost("user/upload")]
        public async Task<IActionResult> UploadFileAsync(IFormFile file, string key)
        {
            try
            {
                await ValidateFileWithRekognitionAsync(file);

                var bucketExists = await _s3Service.IsExistBudget(SystermId);
                if (!bucketExists) return NotFound($"Bucket {SystermId} does not exist.");

                await _s3Service.AddFileToS3Async(file, key, SystermId);

                await TrainAsync(key);

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

        private async Task TrainAsync(string key)
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
                var indexResponse = await _collectionService.IndexFaceAsync(SystermId, SystermId, key);
                #endregion
                #region Add user 
                await AssosiateFaceWithUserAsync(indexResponse.FaceRecords[0].Face, key);
                #endregion
            }
            catch (Exception ex)
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

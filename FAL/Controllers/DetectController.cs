using Amazon.S3;
using FAL.Services;
using FAL.Services.IServices;
using FAL.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Share.SystemModel;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace FAL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetectController : ControllerBase
    {
        private readonly IS3Service _s3Service;
        private readonly ICollectionService _collectionService;
        private readonly CustomLog _logger;
        private readonly string SystermId = GlobalVarians.SystermId;

        public DetectController(IAmazonS3 s3Client, CustomLog logger, ICollectionService collectionService, IS3Service s3Service)
        {
            _logger = logger;
            _collectionService = collectionService;
            _s3Service = s3Service;
        }


        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> DetectAsync(IFormFile file,string mediaId)
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == SystermId).Value;
                #region check input
                file.ValidFile();
                #endregion
                #region add to S3
                var bucketExists = await _s3Service.AddBudgetAsync(systermId);
                if (!bucketExists) return NotFound($"Bucket {systermId} does not exist.");
                var fileName = Guid.NewGuid().ToString();
                var valueS3Return = await _s3Service.AddFileToS3Async(file, fileName, systermId, TypeOfRequest.Tagging,mediaId);
                #endregion
                return Ok(new ResultResponse
                {
                    Status = true,
                    Message = "The system has received the file."
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

        [Authorize]
        [HttpPost("upload-multiple-images")]
        public async Task<IActionResult> UploadMultipleImages([FromForm] IFormFileCollection files)
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == SystermId).Value;
                if (files == null || files.Count == 0)
                {
                    return BadRequest("No files received from the upload.");
                }
                var bucketExists = await _s3Service.AddBudgetAsync(systermId);
                if (!bucketExists) return NotFound($"Bucket {systermId} does not exist.");
                foreach (var item in files)
                {
                    item.ValidImage();
                }
                foreach (var file in files)
                {
                    var fileName = Guid.NewGuid().ToString();
                    var valueS3Return = await _s3Service.AddFileToS3Async(file, fileName, systermId, TypeOfRequest.Tagging,fileName);
                }
                return Ok("Files uploaded successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

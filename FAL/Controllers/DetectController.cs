using Amazon.S3;
using FAL.Services;
using FAL.Services.IServices;
using FAL.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Share.SystemModel;
using System.Net;

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

        [HttpPost("detect")]
        public async Task<IActionResult> DetectAsync(IFormFile file)
        {
            try
            {
                #region check input
                file.ValidFile();
                #endregion
                #region add to S3
                var bucketExists = await _s3Service.AddBudgetAsync(SystermId);
                if (!bucketExists) return NotFound($"Bucket {SystermId} does not exist.");
                var fileName = Guid.NewGuid().ToString();
                var valueS3Return = await _s3Service.AddFileToS3Async(file, fileName, SystermId, TypeOfRequest.Tagging);
                #endregion
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("upload-multiple-images")]
        public async Task<IActionResult> UploadMultipleImages([FromForm] IFormFileCollection files)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest("No files received from the upload.");
                }
                var bucketExists = await _s3Service.AddBudgetAsync(SystermId);
                if (!bucketExists) return NotFound($"Bucket {SystermId} does not exist.");
                foreach (var item in files)
                {
                    item.ValidImage();
                }
                foreach (var file in files)
                {
                    var fileName = Guid.NewGuid().ToString();
                    var valueS3Return = await _s3Service.AddFileToS3Async(file, fileName, SystermId, TypeOfRequest.Tagging);
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

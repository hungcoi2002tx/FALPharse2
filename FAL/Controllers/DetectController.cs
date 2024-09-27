using Amazon.S3;
using FAL.Services;
using FAL.Services.IServices;
using FAL.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Share.SystemModel;

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
                //check valid file
                #region check input
                file.ValidFile();
                #endregion
                //add s3
                var bucketExists = await _s3Service.AddBudgetAsync(SystermId);
                if (!bucketExists) return NotFound($"Bucket {SystermId} does not exist.");
                var fileName = Guid.NewGuid().ToString();
                var valueS3Return = await _s3Service.AddFileToS3Async(file, fileName, SystermId, TypeOfRequest.Tagging);

                //index faces
                //var response = await _collectionService.IndexFaceAsync(SystermId, SystermId, fileName);

                //string result = string.Empty;
                //foreach (var item in response.FaceRecords)
                //{
                //    var faceId = item.Face.FaceId;
                //    var data = await _collectionService.SearchUserByFaceIdsAsync(SystermId, faceId);
                //    if (data.UserMatches != null)
                //    {
                //        var userId = data.UserMatches.First().User.UserId;
                //        result = result + " " + userId;
                //        //train again
                //        await _collectionService.AssociateFacesAsync(SystermId, new List<string>() { faceId }, userId);
                //    }
                //    else
                //    {
                //        //delete 
                //        await _collectionService.DeleteByFaceIdAsync(faceId, SystermId);
                //    }
                //}
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

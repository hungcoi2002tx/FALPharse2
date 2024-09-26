using FAL.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FAL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectionController : ControllerBase
    {
        private readonly IS3Service _s3Service;
        private readonly ICollectionService _collectionService;
        private readonly string SystermId = "FUALUMNI";

        public CollectionController(IS3Service s3Service, ICollectionService collectionService)
        {
            _s3Service = s3Service;
            _collectionService = collectionService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetListFaceIdsAsync()
        {
            try
            {
                var result = await _collectionService.GetFacesAsync(SystermId);
                return Ok(result);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpDelete("faceId")]
        public async Task<IActionResult> DeleteFaceAsync(string faceId)
        {
            try
            {
                var result = await _collectionService.DeleteByFaceIdAsync(faceId, SystermId);
                return Ok(result);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

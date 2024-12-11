
using FAL.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using FAL.Services.IServices;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Share.DTO;
using Share.Utils;
using Share.Constant;
using Share.Model;

namespace FAL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectionController : ControllerBase
    {
        private readonly IS3Service _s3Service;
        private readonly ICollectionService _collectionService;
        private readonly string SystermId = GlobalVarians.SystermId;
        private readonly IMapper _mapper;
        private readonly CustomLog _logger;

        public CollectionController(IS3Service s3Service, ICollectionService collectionService, IMapper mapper, CustomLog log)
        {
            _s3Service = s3Service;
            _collectionService = collectionService;
            _mapper = mapper;
            _logger = log;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetListFaceIdsAsync()
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == SystermId).Value;
                var result = await _collectionService.GetFacesAsync(systermId);
                List<FaceTrainModel> list = _mapper.Map<List<FaceTrainModel>>(result);
                return Ok(list);
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
        [HttpDelete("faceId")]
        public async Task<IActionResult> DeleteFaceAsync(string faceId,string systermId)
        {
            try
            {
                var result = await _collectionService.DeleteByFaceIdAsync(faceId, systermId);
                return Ok(result);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize]
        [HttpGet("getList")]
        public async Task<IActionResult> GetListCollectionAsync()
        {
            try
            {
                var result = await _collectionService.GetCollectionAsync(SystermId);
                return Ok(result);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCollectionAsync([FromBody] string collectionId)
        {
            try
            {
                var result = await _collectionService.CreateCollectionByIdAsync(collectionId);
                return Ok(result);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

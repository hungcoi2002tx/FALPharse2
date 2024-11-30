using Amazon.DynamoDBv2.Model;
using FAL.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Share.Constant;
using Share.Model;
using Share.Utils;
using System.Text.Json;

namespace FAL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultController : ControllerBase
    {
        private readonly IDynamoDBService _dynamoService;
        private readonly CustomLog _logger;


        public ResultController(IDynamoDBService dynamoService, CustomLog logger)
        {
            _dynamoService = dynamoService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("WebhookResult/{mediaId}")]
        public async Task<IActionResult> GetWebhookResult(string mediaId)
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == GlobalVarians.SystermId).Value;

                var result = await _dynamoService.GetWebhookResult(GetDBResultBySystemName(systermId), mediaId);

                await _dynamoService.LogRequestAsync(systermId, RequestTypeEnum.GetWebhookResult, RequestResultEnum.Success, JsonSerializer.Serialize(mediaId));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("DetectStats")]
        public async Task<IActionResult> GetDetectStats()
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == GlobalVarians.SystermId).Value;

                var result = await _dynamoService.GetDetectStats(GetDBResultBySystemName(systermId));

                if (result != null)
                {
                    return Ok(result);
                }
                return StatusCode(500, "Something is wrong with da server");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something is wrong with da server");
            }
        }

        [Authorize]
        [HttpGet("RequestStats")]
        public async Task<IActionResult> GetRequestStats()
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == GlobalVarians.SystermId).Value;

                var result = await _dynamoService.GetRequestStats(systermId);

                if (result != null)
                {
                    return Ok(result);
                }
                return StatusCode(500, "Something is wrong with da server");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something is wrong with da server");
            }
        }

        [Authorize]
        [HttpGet("TrainStats")]
        public async Task<IActionResult> GetTrainStats()
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == GlobalVarians.SystermId).Value;

                var result = await _dynamoService.GetTrainStats(systermId);

                if (result != null)
                {
                    return Ok(result);
                }
                return StatusCode(500, "Something is wrong with da server");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something is wrong with da server");
            }
        }

        private string GetDBResultBySystemName(string systemName)
        {
            try
            {
                return systemName + "-result";
            }
            catch (Exception)
            {

                throw;
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetResult(string fileName)
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == GlobalVarians.SystermId).Value;
                Dictionary<string, AttributeValue> dictionary = new Dictionary<string, AttributeValue>
                {
                    { ":v_fileName", new AttributeValue { S = fileName } }
                };

                var result = await _dynamoService.GetRecordByKeyConditionExpressionAsync(Utils.StringExtention.GetTableNameResult(systermId), "FileName = :v_fileName", dictionary) ?? "";

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

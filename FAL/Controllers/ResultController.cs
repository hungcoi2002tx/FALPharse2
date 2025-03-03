﻿using Amazon.DynamoDBv2.Model;
using Amazon.Rekognition.Model;
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
        private readonly ICollectionService _collectionService;
        private readonly CustomLog _logger;


        public ResultController(IDynamoDBService dynamoService, CustomLog logger, ICollectionService collectionService)
        {
            _dynamoService = dynamoService;
            _logger = logger;
            _collectionService = collectionService;
        }

        [Authorize]
        [HttpGet("WebhookResult/{mediaId}")]
        public async Task<IActionResult> GetWebhookResult(string mediaId)
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == GlobalVarians.SystermId).Value;

                var result = await _dynamoService.GetWebhookResult(systermId, mediaId);

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

                var result = await _dynamoService.GetDetectStats(systermId);

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
        [HttpGet("Detect/Chart")]
        public async Task<IActionResult> GetDetectChart([FromQuery] string year)
        {
            try
            {
                // Retrieve the system ID from user claims
                var systermId = User.Claims.FirstOrDefault(c => c.Type == GlobalVarians.SystermId)?.Value;

                if (string.IsNullOrEmpty(systermId))
                {
                    return BadRequest("System ID is missing in user claims.");
                }

                // Pass the year as a parameter to the service
                var result = await _dynamoService.GetDetectChartStats(systermId, year);

                if (result != null)
                {
                    return Ok(result);
                }

                return StatusCode(500, "Something is wrong with the server.");
            }
            catch (Exception ex)
            {
                // Log the exception (add logging mechanism if necessary)
                return StatusCode(500, "Something is wrong with the server.");
            }
        }

        [Authorize]
        [HttpGet("Train/Chart")]
        public async Task<IActionResult> GetTrainChart([FromQuery] string year)
        {
            try
            {
                // Retrieve the system ID from user claims
                var systermId = User.Claims.FirstOrDefault(c => c.Type == GlobalVarians.SystermId)?.Value;

                if (string.IsNullOrEmpty(systermId))
                {
                    return BadRequest("System ID is missing in user claims.");
                }

                // Pass the year as a parameter to the service
                var result = await _dynamoService.GetTrainChartStats(systermId, year);

                if (result != null)
                {
                    return Ok(result);
                }

                return StatusCode(500, "Something is wrong with the server.");
            }
            catch (Exception ex)
            {
                // Log the exception (add logging mechanism if necessary)
                return StatusCode(500, "Something is wrong with the server.");
            }
        }

        [Authorize]
        [HttpGet("Request/Chart")]
        public async Task<IActionResult> GetRequestChart([FromQuery] string year)
        {
            try
            {
                // Retrieve the system ID from user claims
                var systermId = User.Claims.FirstOrDefault(c => c.Type == GlobalVarians.SystermId)?.Value;

                if (string.IsNullOrEmpty(systermId))
                {
                    return BadRequest("System ID is missing in user claims.");
                }

                // Pass the year as a parameter to the service
                var result = await _dynamoService.GetRequestChartStats(systermId, year);

                if (result != null)
                {
                    return Ok(result);
                }

                return StatusCode(500, "Something is wrong with the server.");
            }
            catch (Exception ex)
            {
                // Log the exception (add logging mechanism if necessary)
                return StatusCode(500, "Something is wrong with the server.");
            }
        }

        [Authorize]
        [HttpGet("Collection/Chart")]
        public async Task<IActionResult> GetCollectionChart([FromQuery] string year)
        {
            try
            {
                // Retrieve the system ID from user claims
                var systermId = User.Claims.FirstOrDefault(c => c.Type == GlobalVarians.SystermId)?.Value;

                if (string.IsNullOrEmpty(systermId))
                {
                    return BadRequest("System ID is missing in user claims.");
                }

                // Pass the year as a parameter to the service
                var result = await _collectionService.GetCollectionChartStats(systermId, year);

                if (result != null)
                {
                    return Ok(result);
                }

                return StatusCode(500, "Something is wrong with the server.");
            }
            catch (Exception ex)
            {
                // Log the exception (add logging mechanism if necessary)
                return StatusCode(500, "Something is wrong with the server.");
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
        [HttpGet("RequestStats/Details/{requestType}")]
        public async Task<IActionResult> GerRequestStatsDetail(string requestType,
    [FromQuery] DateTime? startDate = null,
    [FromQuery] DateTime? endDate = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == GlobalVarians.SystermId).Value;

                var result = await _dynamoService.GetRequestStatsDetail(systermId, requestType, startDate, endDate, page, pageSize);

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
        public async Task<IActionResult> GetTrainStats([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string searchUserId = null)
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == GlobalVarians.SystermId).Value;

                var result = await _dynamoService.GetTrainStats(systermId, page, pageSize, searchUserId);

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
        [HttpGet("TrainStats/Details/{userId}")]
        public async Task<IActionResult> GetTrainStatsDetails(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == GlobalVarians.SystermId).Value;

                var result = await _dynamoService.GetTrainStatsDetail(systermId, userId, page, pageSize);

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
        [HttpDelete("TrainStats/{userId}/{faceId}")]
        public async Task<IActionResult> DeleteTrainStats(string userId, string faceId)
        {
            try
            {
                var systermId = User.Claims.FirstOrDefault(c => c.Type == GlobalVarians.SystermId).Value;

                var result = await _dynamoService.DeleteTrainStat(systermId, userId, faceId);

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
                return GlobalVarians.RESULT_INFO_TABLE_DYNAMODB;
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
                    { ":v_systemName", new AttributeValue { S = systermId } },
                    { ":v_fileName", new AttributeValue { S = fileName } }
                };

                var result = await _dynamoService.GetRecordByKeyConditionExpressionAsync(Utils.StringExtention.GetTableNameResult(systermId), "SystemName = :v_systemName and FileName = :v_fileName", dictionary) ?? "";

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

using Amazon.DynamoDBv2.Model;
using FAL.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Share.SystemModel;

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

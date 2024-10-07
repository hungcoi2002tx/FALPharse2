using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Rekognition.Model;
using FAL.Services.IServices;
using Share.Data;

namespace FAL.Services
{
    public class DynamoDBService : IDynamoDBService
    {
        private readonly IAmazonDynamoDB _dynamoDBService;

        public DynamoDBService(IAmazonDynamoDB dynamoDBService)
        {
            _dynamoDBService = dynamoDBService;
        }

        public async Task<bool> CreateUserInformationAsync(string tableName, string userId, string faceId)
        {
            try
            {
                var request = new PutItemRequest
                {
                    TableName = tableName,
                    Item = new Dictionary<string, AttributeValue>
            {
                {
                    nameof(FaceInformation.UserId), new AttributeValue
                    {
                        S = userId
                    }
                },
                 {
                    nameof(FaceInformation.FaceId), new AttributeValue
                    {
                        S = faceId
                    }
                },
                {
                    nameof(FaceInformation.CreateDate), new AttributeValue
                    {
                        S = DateTime.Now.ToString()
                    }
                }
            }
                };

                await _dynamoDBService.PutItemAsync(request);
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> IsExistFaceIdAsync(string systermId, string faceId)
        {
            try
            {
                var queryRequest = new QueryRequest
                {
                    TableName = systermId,
                    FilterExpression = "FaceId = :v_faceId",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                        {
                            { ":v_faceId", new AttributeValue { S = faceId } }
                        },
                    Limit = 1
                };
                var response = await _dynamoDBService.QueryAsync(queryRequest);
                if (response != null && response.Count > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> IsExistUserAsync(string systermId, string userId)
        {
            try
            {
                var queryRequest = new QueryRequest
                {
                    TableName = systermId,
                    KeyConditionExpression = "UserId = :v_userId",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                        {
                            { ":v_userId", new AttributeValue { S = userId } }
                        },
                    Limit = 1
                };
                var response = await _dynamoDBService.QueryAsync(queryRequest);
                if (response != null && response.Count > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

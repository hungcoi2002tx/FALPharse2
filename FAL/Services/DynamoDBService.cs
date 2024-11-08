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
                var request = new QueryRequest
                {
                    TableName = systermId,
                    IndexName = "FaceIdIndex",  // Use the GSI index name
                    KeyConditionExpression = "FaceId = :faceId",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":faceId", new AttributeValue { S = faceId } }
                    }
                };

                var response = await _dynamoDBService.QueryAsync(request);
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


        public async Task<string?> GetRecordByKeyConditionExpressionAsync(string systermId, string keyConditionExpression, Dictionary<string, AttributeValue> dictionary)
        {
            string? result = null;
            try
            {
                var queryRequest = new QueryRequest
                {
                    TableName = systermId,
                    KeyConditionExpression = keyConditionExpression,
                    ExpressionAttributeValues = dictionary,
                };
                var resultQuery = await _dynamoDBService.QueryAsync(queryRequest);

                var firstRecord = resultQuery.Items.FirstOrDefault();
                if (firstRecord != null)
                {
                    if (firstRecord.ContainsKey("Data") && firstRecord["Data"].S != null)
                    {
                        result = firstRecord["Data"].S;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> DeleteUserInformationAsync(string tableName, string userId, string faceId)
        {
            try
            {
                // Define the key (partition and sort keys if applicable) for the item to be deleted
                var key = new Dictionary<string, AttributeValue>
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
            }
        };

                var request = new DeleteItemRequest
                {
                    TableName = tableName,
                    Key = key
                };

                var response = await _dynamoDBService.DeleteItemAsync(request);

                // Check if the response is successful
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user information: {ex.Message}");
                throw;
            }
        }

    }
}

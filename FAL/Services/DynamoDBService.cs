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

        Task IDynamoDBService.DeleteUserInformationAsync(string systermId, string userId, string faceId)
        {
            throw new NotImplementedException();
        }
        public async Task<List<string>> GetFaceIdsByUserIdAsync(string userId, string systemId)
        {
            var request = new QueryRequest
            {
                TableName = systemId,
                KeyConditionExpression = "UserId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":userId", new AttributeValue { S = userId } }
        }
            };

            var response = await _dynamoDBService.QueryAsync(request);
            return response.Items.Select(item => item["FaceId"].S).ToList();
        }

        public async Task DeleteUserFromDynamoDbAsync(string userId, string systemId)
        {
            try
            {
                // Step 1: Scan for all items with the given UserId
                var scanRequest = new ScanRequest
                {
                    TableName = systemId,
                    FilterExpression = "UserId = :userId",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":userId", new AttributeValue { S = userId } }
            }
                };

                var scanResponse = await _dynamoDBService.ScanAsync(scanRequest);

                // Step 2: Delete each item found in the scan
                foreach (var item in scanResponse.Items)
                {
                    // Assuming 'FaceId' is the sort key in the schema
                    var faceId = item.ContainsKey("FaceId") ? item["FaceId"].S : null;

                    if (faceId == null)
                    {
                        Console.WriteLine($"No FaceId found for UserId {userId}. Skipping item.");
                        continue; // Skip this item if FaceId is not available
                    }

                    var deleteRequest = new DeleteItemRequest
                    {
                        TableName = systemId,
                        Key = new Dictionary<string, AttributeValue>
                {
                    { "UserId", new AttributeValue { S = userId } }, // Partition key
                    { "FaceId", new AttributeValue { S = faceId } }  // Sort key
                }
                    };

                    // Delete the item
                    await _dynamoDBService.DeleteItemAsync(deleteRequest);
                }
            }
            catch (AmazonDynamoDBException dbEx)
            {
                // Handle DynamoDB-specific exceptions
                Console.WriteLine($"DynamoDB error occurred: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                // Handle other types of exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }


    }
}

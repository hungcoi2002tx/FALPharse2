﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Rekognition.Model;
using FAL.Services.IServices;
using Share.Data;
using Share.DTO;
using System.Text.Json;

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
                                S = userId.ToLower()
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
                            { ":v_userId", new AttributeValue { S = userId.ToLower() } }
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

        public async Task<string> GetFaceIdForUserAndFaceAsync(string userId, string faceId, string tableName)
        {
            var queryRequest = new QueryRequest
            {
                TableName = tableName,
                KeyConditionExpression = "UserId = :userId and FaceId = :faceId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":userId", new AttributeValue { S = userId } },
            { ":faceId", new AttributeValue { S = faceId } }
        }
            };

            var queryResponse = await _dynamoDBService.QueryAsync(queryRequest);

            // If the faceId exists, return the FaceId, otherwise return null
            return queryResponse.Items.Count > 0 ? queryResponse.Items.First()["FaceId"].S : null;
        }

        public async Task<string> GetOldestFaceIdForUserAsync(string userId, string tableName)
        {
            var queryRequest = new QueryRequest
            {
                TableName = tableName,
                KeyConditionExpression = "UserId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":userId", new AttributeValue { S = userId } }
        }
            };

            try
            {

                var queryResponse = await _dynamoDBService.QueryAsync(queryRequest);


                // Check if no items were found
                if (queryResponse.Items.Count == 0)
                {
                    return null;
                }

                // Sort items by CreatedDate in ascending order and get the oldest one
                var oldestItem = queryResponse.Items
                .OrderBy(item => DateTime.Parse(item["CreateDate"].S)) // Sort by CreatedDate
                .FirstOrDefault();

                // Log the oldest FaceId found
                var oldestFaceId = oldestItem?["FaceId"].S;

                return oldestFaceId;
            }
            catch (Exception ex)
            {
                throw; // Re-throw the exception after logging it
            }
        }

        public async Task DeleteItem(string userId, string faceId, string collectionName)
        {
            await _dynamoDBService.DeleteItemAsync(new DeleteItemRequest
            {
                TableName = collectionName,
                Key = new Dictionary<string, AttributeValue>
        {
            { "UserId", new AttributeValue { S = userId } },
            { "FaceId", new AttributeValue { S = faceId } }
        }
            });
        }

        public async Task<FaceDetectionResult> GetWebhookResult(string systermId, string mediaId)
        {
            FaceDetectionResult result = null;

            try
            {
                // Define the query request to get the specific item by partition key (fileName)
                var queryRequest = new QueryRequest
                {
                    TableName = systermId,
                    KeyConditionExpression = "FileName = :fileName",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":fileName", new AttributeValue { S = mediaId.ToLower() } }
            }
                };

                // Execute the query
                var queryResponse = await _dynamoDBService.QueryAsync(queryRequest);

                if (queryResponse.Items.Count > 0 && queryResponse.Items[0].TryGetValue("Data", out var dataAttribute))
                {
                    // Deserialize the Data attribute into FaceDetectionResult
                    result = JsonSerializer.Deserialize<FaceDetectionResult>(dataAttribute.S);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing item with FileName: {mediaId}. Error: {ex.Message}");
            }

            return result;
        }

        public async Task<DetectStatsResponse> GetDetectStats(string tableName)
        {
            try
            {
                // Connect to the DynamoDB table
                var table = Table.LoadTable(_dynamoDBService, tableName);

                // Scan the table to retrieve all items
                var search = table.Scan(new ScanOperationConfig());
                var results = new List<Document>();
                do
                {
                    results.AddRange(await search.GetNextSetAsync());
                } while (!search.IsDone);

                // Extract the "FileName" field
                var fileNames = results
                    .Select(doc => doc["FileName"].AsString())
                    .ToList();

                // Calculate unique and duplicate counts
                var uniqueMediaCount = fileNames.Distinct().Count();
                var duplicateMediaCount = fileNames.Count() - uniqueMediaCount;

                // Return the response
                return new DetectStatsResponse
                {
                    TotalMediaDetected = uniqueMediaCount,
                    TotalMediaRetries = duplicateMediaCount
                };
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public async Task<TrainStatsResponse> GetTrainStats(string systermId)
        {
            try
            {
                // Load the table
                var table = Table.LoadTable(_dynamoDBService, systermId);

                // Scan the table to retrieve all records
                var search = table.Scan(new ScanOperationConfig());
                var results = new List<Document>();
                do
                {
                    results.AddRange(await search.GetNextSetAsync());
                } while (!search.IsDone);

                // Group by UserId and count FaceId for each user
                var groupedData = results
                    .GroupBy(doc => doc["UserId"].AsString())
                    .Select(group => new TrainStatsOfUser
                    {
                        UserId = group.Key,
                        TotalNumberOfFaceTrained = group.Count() // Count how many FaceIds each user has
                    })
                    .ToList();

                // Calculate the total unique UserIds
                var totalUniqueUserIds = groupedData.Count;

                // Calculate the total number of FaceIds (all records in the table)
                var totalTrainedFaceId = results.Count;

                // Return the response
                return new TrainStatsResponse
                {
                    TotalTrainedUserId = totalUniqueUserIds,
                    TotalTrainedFaceId = totalTrainedFaceId, // Add total count of all FaceIds
                    UserStats = groupedData
                };
            }
            catch (System.Exception ex)
            {
                // Handle exceptions gracefully (e.g., log the error)
                return null;
            }
        }

    }

}

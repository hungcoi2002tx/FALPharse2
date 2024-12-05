using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Rekognition.Model;
using FAL.Services.IServices;
using Newtonsoft.Json;
using Share.Constant;
using Share.DTO;
using Share.Model;
using System.Collections;
using System.Text.Json;

namespace FAL.Services
{
    public class DynamoDBService : IDynamoDBService
    {
        private readonly IAmazonDynamoDB _dynamoDBService;
        private static readonly string LOG_TABLE_NAME = "ClientRequests";

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
                    TableName = GlobalVarians.FACEID_TABLE_DYNAMODB,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        {
                            nameof(FaceInformation.UserId), new AttributeValue
                            {
                                S = userId.ToLower()
                            }
                        },
                        {
                            nameof(FaceInformation.SystemName), new AttributeValue
                            {
                                S = tableName
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

        public async Task<bool> IsExistFaceIdAsync(string systemName, string faceId)
        {
            try
            {
                var request = new QueryRequest
                {
                    TableName = GlobalVarians.FACEID_TABLE_DYNAMODB,
                    IndexName = GlobalVarians.FACEID_INDEX_ATTRIBUTE_DYNAMODB,
                    KeyConditionExpression = "SystemName = :systemName AND FaceId = :faceId",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":systemName", new AttributeValue { S = systemName } },
                        { ":faceId", new AttributeValue { S = faceId } }
                    }
                };

                var response = await _dynamoDBService.QueryAsync(request);
                // Check if any items exist
                return response != null && response.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> IsExistUserAsync(string systemId, string userId)
        {
            try
            {
                var queryRequest = new QueryRequest
                {
                    TableName = GlobalVarians.FACEID_TABLE_DYNAMODB,
                    IndexName = GlobalVarians.FACEID_INDEX_ATTRIBUTE_DYNAMODB, // Sử dụng GSI trên SystemName
                    KeyConditionExpression = "SystemName = :v_systemName and UserId = :v_userId",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_systemName", new AttributeValue { S = systemId } },
                { ":v_userId", new AttributeValue { S = userId } }
            },
                    Limit = 1 // Giới hạn 1 kết quả để kiểm tra nhanh
                };

                var response = await _dynamoDBService.QueryAsync(queryRequest);

                // Kiểm tra nếu có kết quả trả về
                return response != null && response.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
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
                    TableName = GlobalVarians.RESULT_INFO_TABLE_DYNAMODB,
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
                // Xác định Partition Key (UserId) và Sort Key (FaceId) để xóa
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
                    TableName = GlobalVarians.FACEID_TABLE_DYNAMODB, // Sử dụng bảng từ GlobalVarians
                    Key = key
                };

                var response = await _dynamoDBService.DeleteItemAsync(request);

                // Kiểm tra trạng thái trả về có thành công hay không
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
            try
            {
                var request = new QueryRequest
                {
                    TableName = GlobalVarians.FACEID_TABLE_DYNAMODB,
                    IndexName = GlobalVarians.FACEID_INDEX_ATTRIBUTE_DYNAMODB,
                    KeyConditionExpression = "#sysName = :v_systemName and #userId = :v_userId",
                    ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#sysName", "SystemName" },
                { "#userId", "UserId" }
            },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_systemName", new AttributeValue { S = systemId } },
                { ":v_userId", new AttributeValue { S = userId } }
            },
                    ProjectionExpression = "FaceId"
                };

                // Gửi query request tới DynamoDB
                var response = await _dynamoDBService.QueryAsync(request);

                // Kiểm tra kết quả và trả về danh sách FaceId
                if (response != null && response.Items.Count > 0)
                {
                    // Chuyển đổi kết quả từ AttributeValue sang List<string>
                    return response.Items
                        .Select(item => item["FaceId"].S) // Lấy giá trị của thuộc tính FaceId
                        .ToList();
                }

                // Trả về danh sách rỗng nếu không có bản ghi nào
                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFaceIdsByUserIdAsync: {ex.Message}");
                throw;
            }
        }


        public async Task DeleteUserFromDynamoDbAsync(string userId, string systemId)
        {
            try
            {
                // Query tất cả các bản ghi cần xóa
                var queryRequest = new QueryRequest
                {
                    TableName = GlobalVarians.FACEID_TABLE_DYNAMODB,
                    KeyConditionExpression = "UserId = :userId",
                    FilterExpression = "SystemName = :systemName",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":userId", new AttributeValue { S = userId } },
                { ":systemName", new AttributeValue { S = systemId } }
            }
                };

                var queryResponse = await _dynamoDBService.QueryAsync(queryRequest);

                // Chia danh sách thành các nhóm tối đa 25 bản ghi
                var chunks = queryResponse.Items
                    .Select((item, index) => new { item, index })
                    .GroupBy(x => x.index / 25)
                    .Select(group => group.Select(x => x.item).ToList());

                foreach (var chunk in chunks)
                {
                    var batchWriteRequest = new BatchWriteItemRequest
                    {
                        RequestItems = new Dictionary<string, List<WriteRequest>>
                {
                    {
                        GlobalVarians.FACEID_TABLE_DYNAMODB,
                        chunk.Select(item => new WriteRequest
                        {
                            DeleteRequest = new DeleteRequest
                            {
                                Key = new Dictionary<string, AttributeValue>
                                {
                                    { nameof(FaceInformation.UserId), item[nameof(FaceInformation.UserId)] }, // Partition Key
                                    { nameof(FaceInformation.FaceId), item[nameof(FaceInformation.FaceId)] }  // Sort Key
                                }
                            }
                        }).ToList()
                    }
                }
                    };

                    await _dynamoDBService.BatchWriteItemAsync(batchWriteRequest);
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


        public async Task<string> GetFaceIdForUserAndFaceAsync(string userId, string faceid, string systemName)
        {
            try
            {
                var queryRequest = new QueryRequest
                {
                    TableName = GlobalVarians.FACEID_TABLE_DYNAMODB,
                    KeyConditionExpression = "UserId = :userId AND FaceId = :faceId",
                    FilterExpression = $"{GlobalVarians.SYSTEM_NAME_ATTRIBUTE_DYNAMODB} = :systemName",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":userId", new AttributeValue { S = userId } },
                        { ":faceId", new AttributeValue { S = faceid } },
                        { ":systemName", new AttributeValue { S = systemName } }
                    }
                };

                var queryResponse = await _dynamoDBService.QueryAsync(queryRequest);

                // If a record exists, return the FaceId, otherwise return null
                return queryResponse.Items.Count > 0 ? queryResponse.Items.First()["FaceId"].S : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFaceIdForUserAndFaceAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetOldestFaceIdForUserAsync(string userId, string collectionName)
        {
            var queryRequest = new QueryRequest
            {
                TableName = GlobalVarians.FACEID_TABLE_DYNAMODB,
                IndexName = GlobalVarians.FACEID_INDEX_ATTRIBUTE_DYNAMODB,
                KeyConditionExpression = "UserId = :userId AND SystemName = :systemName", // Sử dụng cả UserId và SystemName
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":userId", new AttributeValue { S = userId } },
                    { ":systemName", new AttributeValue { S = collectionName } }
                }
            };

            var queryResponse = await _dynamoDBService.QueryAsync(queryRequest);

            // Kiểm tra nếu không có dữ liệu
            if (queryResponse.Items.Count == 0)
            {
                return null;
            }

            // Lọc và tìm mục cũ nhất dựa trên CreateDate
            var oldestItem = queryResponse.Items
                .Where(item => item.ContainsKey("CreateDate") && DateTime.TryParse(item["CreateDate"].S, out _))
                .OrderBy(item => DateTime.Parse(item["CreateDate"].S))
                .FirstOrDefault();

            return oldestItem?["FaceId"].S;
        }


        public async Task DeleteItemAsync(string userId, string faceId, string collectionName)
        {
            try
            {
                // Tạo yêu cầu xóa mục
                var deleteRequest = new DeleteItemRequest
                {
                    TableName = GlobalVarians.FACEID_TABLE_DYNAMODB,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "UserId", new AttributeValue { S = userId } }, // Partition key
                        { "FaceId", new AttributeValue { S = faceId } }  // Sort key
                    }
                };

                // Thực thi yêu cầu xóa
                var response = await _dynamoDBService.DeleteItemAsync(deleteRequest);

                // Kiểm tra kết quả trả về
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"Successfully deleted item with UserId: {userId} and FaceId: {faceId}");
                }
                else
                {
                    Console.WriteLine($"Failed to delete item. HTTP Status: {response.HttpStatusCode}");
                }
            }
            catch (AmazonDynamoDBException dbEx)
            {
                Console.WriteLine($"DynamoDB error occurred: {dbEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        //chưa test
        public async Task<FaceDetectionResult> GetWebhookResult(string systermId, string mediaId)
        {
            FaceDetectionResult result = null;

            try
            {
                // Define the query request to get the specific item by partition key (fileName)
                var queryRequest = new QueryRequest
                {
                    TableName = GlobalVarians.RESULT_INFO_TABLE_DYNAMODB,
                    KeyConditionExpression = "SystemName = :v_systemName and FileName = :v_fileName",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":v_systemName", new AttributeValue { S = systermId } },
                        { ":v_fileName", new AttributeValue { S = mediaId } }
                    }
                };

                // Execute the query
                var queryResponse = await _dynamoDBService.QueryAsync(queryRequest);

                if (queryResponse.Items.Count > 0 && queryResponse.Items[0].TryGetValue("Data", out var dataAttribute))
                {
                    // Deserialize the Data attribute into FaceDetectionResult
                    result = System.Text.Json.JsonSerializer.Deserialize<FaceDetectionResult>(dataAttribute.S);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing item with FileName: {mediaId}. Error: {ex.Message}");
            }

            return result;
        }
        //chưa test
        public async Task<DetectStatsResponse> GetDetectStats(string tableName)
        {
            try
            {
                var request = new ScanRequest
                {
                    TableName = GlobalVarians.RESULT_INFO_TABLE_DYNAMODB, // Tên bảng DynamoDB
                    FilterExpression = "SystemName = :systemName",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":systemName", new AttributeValue { S = tableName } } // Điều kiện trên Sort Key
                    },
                    Select = "COUNT" // Chỉ trả về số lượng bản ghi
                };

                var response = await _dynamoDBService.ScanAsync(request);

                // Return the response
                return new DetectStatsResponse
                {
                    TotalMediaDetected = response.Count,
                    TotalMediaRetries = 0
                };
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }


        //public async Task<TrainStatsResponse> GetTrainStats(string systemId, int page, int pageSize, string searchUserId)
        //{
        //    try
        //    {
        //        // Load the table
        //        var table = Table.LoadTable(_dynamoDBService, "YourMainTableName");

        //        // Query using the GSI for systemId
        //        var queryConfig = new QueryOperationConfig
        //        {
        //            IndexName = "systemId-index", // Replace with the name of your GSI
        //            KeyExpression = new Expression
        //            {
        //                ExpressionStatement = "systemId = :v_systemId",
        //                ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
        //        {
        //            { ":v_systemId", systemId }
        //        }
        //            },
        //            Limit = pageSize, // Only fetch the number of records needed for the page
        //            PaginationToken = GetPaginationTokenForPage(page) // Optional: Use token for efficient pagination
        //        };

        //        var search = table.Query(queryConfig);
        //        var results = await search.GetNextSetAsync();

        //        // Filter by UserId if searchUserId is provided
        //        var filteredResults = results;
        //        if (!string.IsNullOrEmpty(searchUserId))
        //        {
        //            filteredResults = results
        //                .Where(doc => doc["UserId"].AsString().IndexOf(searchUserId, StringComparison.OrdinalIgnoreCase) >= 0)
        //                .ToList();
        //        }

        //        // Group by UserId and count FaceId for each user
        //        var groupedData = filteredResults
        //            .GroupBy(doc => doc["UserId"].AsString())
        //            .Select(group => new TrainStatsOfUser
        //            {
        //                UserId = group.Key,
        //                TotalNumberOfFaceTrained = group.Count()
        //            })
        //            .ToList();

        //        // Total trained face IDs
        //        var totalTrainedFaceId = filteredResults.Count;

        //        // Return the response
        //        return new TrainStatsResponse
        //        {
        //            TotalTrainedUserId = groupedData.Count,
        //            TotalTrainedFaceId = totalTrainedFaceId,
        //            TotalRecords = results.Count, // Only count records fetched for this page
        //            CurrentPage = page,
        //            PageSize = pageSize,
        //            UserStats = groupedData
        //        };
        //    }
        //    catch (System.Exception ex)
        //    {
        //        // Handle exceptions gracefully (e.g., log the error)
        //        return null;
        //    }
        //}

        //chưa test
        public async Task<TrainStatsResponse> GetTrainStats(string systemId, int page, int pageSize, string searchUserId)
        {
            try
            {
                // Load the table (using a single table design)
                var table = Table.LoadTable(_dynamoDBService, GlobalVarians.FACEID_TABLE_DYNAMODB);

                // Query the table using the GSI for the specific systemId
                var queryConfig = new QueryOperationConfig
                {
                    IndexName = GlobalVarians.FACEID_INDEX_ATTRIBUTE_DYNAMODB,
                    KeyExpression = new Expression
                    {
                        ExpressionStatement = "#sysId = :v_systemId",
                        ExpressionAttributeNames = new Dictionary<string, string>
                        {
                            { "#sysId", "SystemId" }
                        },
                                ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                        {
                            { ":v_systemId", systemId }
                        }
                    }
                };

                // Execute the query
                var search = table.Query(queryConfig);
                var results = new List<Document>();
                do
                {
                    results.AddRange(await search.GetNextSetAsync());
                } while (!search.IsDone);

                // Filter by UserId if searchUserId is provided (partial matching on the partition key)
                var filteredResults = results;
                if (!string.IsNullOrEmpty(searchUserId))
                {
                    filteredResults = results.Where(doc => doc["UserId"].AsString().IndexOf(searchUserId, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                // Group by UserId and count FaceId for each user
                var groupedData = filteredResults
                    .GroupBy(doc => doc["UserId"].AsString())
                    .Select(group => new TrainStatsOfUser
                    {
                        UserId = group.Key,
                        TotalNumberOfFaceTrained = group.Count() // Count how many FaceIds each user has
                    })
                    .ToList();

                // Pagination logic
                var totalRecords = groupedData.Count;
                var pagedData = groupedData
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Total trained face IDs
                var totalTrainedFaceId = filteredResults.Count;

                // Return the response
                return new TrainStatsResponse
                {
                    TotalTrainedUserId = groupedData.Count,
                    TotalTrainedFaceId = totalTrainedFaceId,
                    TotalRecords = totalRecords,
                    CurrentPage = page,
                    PageSize = pageSize,
                    UserStats = pagedData
                };
            }
            catch (System.Exception ex)
            {
                // Handle exceptions gracefully (e.g., log the error)
                return null;
            }
        }

        public async Task<bool> LogRequestAsync(string systemName, RequestTypeEnum requestType, RequestResultEnum status = RequestResultEnum.Unknown, object requestBody = null)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(systemName))
            {
                throw new ArgumentException("SystemName is required.");
            }

            // Convert UTC to Vietnamese Time (UTC+7)
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var createDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).ToString("o"); // ISO 8601 format

            // Prepare the data for DynamoDB
            var item = new Dictionary<string, AttributeValue>
    {
        { "SystemName", new AttributeValue { S = systemName } },
        { "CreateDate", new AttributeValue { S = createDate } },
        { "RequestType", new AttributeValue { S = requestType.ToString() } }
    };

            // Add status to the item
            item.Add("Status", new AttributeValue { S = status.ToString() });

            // Optionally add requestBody if provided
            if (requestBody != null)
            {
                var requestBodyJson = JsonConvert.SerializeObject(requestBody);
                item.Add("RequestBody", new AttributeValue { S = requestBodyJson });
            }

            // Create the PutItem request
            var putItemRequest = new PutItemRequest
            {
                TableName = LOG_TABLE_NAME,
                Item = item
            };

            try
            {
                // Insert the item into the DynamoDB table
                var response = await _dynamoDBService.PutItemAsync(putItemRequest);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging request: {ex.Message}");
                return false;
            }
        }

        public async Task<GroupedRequestData> GetRequestStatsDetail(
    string systermId,
    string requestType,
    DateTime? startDate,
    DateTime? endDate,
    int page,
    int pageSize)
        {
            try
            {
                // Fetch all data for the given system ID
                var allData = await GetRequestsBySystemIdAsync(systermId);

                if (allData == null || !allData.Any())
                {
                    return new GroupedRequestData
                    {
                        RequestType = requestType,
                        Requests = new List<ClientRequest>(),
                        TotalRecords = 0,
                        CurrentPage = page,
                        PageSize = pageSize
                    };
                }

                // Filter by request type
                var filteredData = allData
                    .Where(x => x.RequestType.Equals(requestType, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Filter by date range if provided
                if (startDate.HasValue)
                {
                    filteredData = filteredData
                        .Where(x => DateTime.TryParse(x.CreateDate, out var createDate) && createDate >= startDate.Value)
                        .ToList();
                }

                if (endDate.HasValue)
                {
                    filteredData = filteredData
                        .Where(x => DateTime.TryParse(x.CreateDate, out var createDate) && createDate <= endDate.Value)
                        .ToList();
                }

                // Calculate total records after filtering
                var totalRecords = filteredData.Count;

                // Apply pagination
                if (page > 0 && pageSize > 0)
                {
                    var skip = (page - 1) * pageSize;
                    filteredData = filteredData.Skip(skip).Take(pageSize).ToList();
                }

                // Return the grouped data with pagination info
                return new GroupedRequestData
                {
                    RequestType = requestType,
                    Requests = filteredData,
                    TotalRecords = totalRecords,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in {nameof(GetRequestStatsDetail)}: {ex.Message}");
                throw;
            }
        }

        public async Task<RequestStatsResponse> GetRequestStats(string systermId)
        {
            try
            {
                // Fetch all data for the given system ID
                var allData = await GetRequestsBySystemIdAsync(systermId);

                if (allData == null || !allData.Any())
                {
                    return new RequestStatsResponse();
                }

                // Calculate statistics
                var totalSuccess = allData.Count(x => x.Status == RequestResultEnum.Success.ToString());
                var totalFailed = allData.Count(x => x.Status == RequestResultEnum.Failed.ToString());

                // Group data by RequestType
                var groupedData = allData
                    .GroupBy(x => x.RequestType)
                    .Select(group => new GroupedRequestData
                    {
                        RequestType = group.Key,
                        Requests = group.ToList()
                    })
                    .ToList();

                // Prepare the response
                var response = new RequestStatsResponse
                {
                    TotalSuccess = totalSuccess,
                    TotalFailed = totalFailed,
                    RequestData = groupedData
                };

                return response;
            }
            catch (Exception ex)
            {
                // Log the exception (replace with your logging framework if available)
                Console.WriteLine($"Error in {nameof(GetRequestStats)}: {ex.Message}");
                throw; // Rethrow the exception to allow higher layers to handle it if needed
            }
        }


        private async Task<List<ClientRequest>> GetRequestsBySystemIdAsync(string systemId)
        {
            var request = new QueryRequest
            {
                TableName = "ClientRequests",
                KeyConditionExpression = "SystemName = :systemId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":systemId", new AttributeValue { S = systemId } }
        }
            };

            try
            {
                var response = await _dynamoDBService.QueryAsync(request);

                // Map the response to a list of ClientRequest objects
                return response.Items.Select(item => new ClientRequest
                {
                    SystemName = item["SystemName"].S,
                    CreateDate = item["CreateDate"].S,
                    RequestType = item.ContainsKey("RequestType") ? item["RequestType"].S : null,
                    Status = item.ContainsKey("Status") ? item["Status"].S : null,
                    RequestBody = item.ContainsKey("RequestBody") ? item["RequestBody"].S : null
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data from DynamoDB: {ex.Message}");
                return null;
            }
        }

        public async Task<PaginatedTrainStatsDetailResponse> GetTrainStatsDetail(string systemId, string userId, int page, int pageSize)
        {
            var tableName = systemId; // The table name is the system ID
            var result = new List<TrainStatsDetailDTO>();

            try
            {
                // Define the query parameters
                var queryRequest = new QueryRequest
                {
                    TableName = GlobalVarians.FACEID_TABLE_DYNAMODB,
                    IndexName = GlobalVarians.FACEID_INDEX_ATTRIBUTE_DYNAMODB,
                    KeyConditionExpression = "UserId = :userId AND SystemName = :systemName", // Sử dụng cả UserId và SystemName
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":userId", new AttributeValue { S = userId } },
                    { ":systemName", new AttributeValue { S = systemId } }
                }
                };

                // Execute the query
                var response = await _dynamoDBService.QueryAsync(queryRequest);

                // Map the results to the DTO
                result = response.Items.Select(item => new TrainStatsDetailDTO
                {
                    UserId = item["UserId"].S,
                    FaceId = item.ContainsKey("FaceId") ? item["FaceId"].S : null,
                    CreateDate = item.ContainsKey("CreateDate")
                        ? DateTime.Parse(item["CreateDate"].S)
                        : DateTime.MinValue
                }).ToList();

                // Apply pagination
                var totalRecords = result.Count;
                var paginatedResult = result
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Return paginated response
                return new PaginatedTrainStatsDetailResponse
                {
                    Data = paginatedResult,
                    TotalRecords = totalRecords,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., table not found, invalid query, etc.)
                Console.WriteLine($"Error querying table {tableName}: {ex.Message}");
                return new PaginatedTrainStatsDetailResponse
                {
                    Data = new List<TrainStatsDetailDTO>(),
                    TotalRecords = 0,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
        }

        //bỏ
        public async Task<bool> DeleteTrainStat(string systemId, string userId, string faceId)
        {
            var tableName = systemId; // The table name is the system ID

            try
            {
                // Define the delete request
                var deleteRequest = new DeleteItemRequest
                {
                    TableName = tableName,
                    Key = new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { S = userId } }, // Partition key
                { "FaceId", new AttributeValue { S = faceId } }  // Sort key
            }
                };

                // Execute the delete request
                var response = await _dynamoDBService.DeleteItemAsync(deleteRequest);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"Successfully deleted item with UserId: {userId} and FaceId: {faceId} from table {tableName}.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to delete item. HTTP Status Code: {response.HttpStatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting item from table {tableName} with UserId: {userId} and FaceId: {faceId}. Error: {ex.Message}");
                return false;
            }
        }
    }

}

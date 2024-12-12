using Alumniphase2.Lambda.Models;
using Alumniphase2.Lambda.Utils;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Runtime.Internal.Util;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Data;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Alumniphase2.Lambda;

public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonRekognition _rekognitionClient;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly HttpClient _httpClient;

    public Function()
    {
        _s3Client = new AmazonS3Client();
        _rekognitionClient = new AmazonRekognitionClient();
        _dynamoDbClient = new AmazonDynamoDBClient();
        _httpClient = new HttpClient();
    }

    public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        var logger = new CloudWatchLogger();

        // Ghi log ra CloudWatch
        await logger.LogMessageAsync("mình là lambda, mình detect ảnh nè");
        try
        {
            var s3Record = evnt.Records[0].S3;
            var result = new FaceDetectionResult();

            if (s3Record == null)
            {
                return;
            }

            var bucket = s3Record.Bucket.Name;
            var key = s3Record.Object.Key;

            var metadataResponse = await _s3Client.GetObjectMetadataAsync(bucket, key);
            var contentType = CheckContentType(metadataResponse);
            var fileName = metadataResponse.Metadata[Utils.SystemConstants.ORIGINAL_FILE_NAME];
            var imageWidth = metadataResponse.Metadata["ImageWidth"];
            var imageHeight = metadataResponse.Metadata["ImageHeight"];
            var systemId = metadataResponse.Metadata["SystemId"];
            await logger.LogMessageAsync($"systemId la {systemId}");
            switch (contentType)
            {
                case (false):
                    result = await DetectImageProcess(bucket,systemId, key, fileName);
                    result.Width = int.Parse(imageWidth);
                    result.Height = int.Parse(imageHeight);
                    result.Key = key;
                    var (webhookUrlImage, webhookSecretkeyImage) = await CreateResponseResult(systemId, result);
                    await StoreResponseResult(result, fileName, systemId);
                    await SendResult(result, logger, webhookSecretkeyImage, webhookUrlImage);
                    break;
                case (true):
                    result = await DetectVideoProcess(bucket, key, fileName, systemId);   
                    result.Width = int.Parse(imageWidth);
                    result.Height = int.Parse(imageHeight);
                    result.Key = key;
                    var (webhookUrlVideo, webhookSecretkeyVideo) = await CreateResponseResult(systemId, result);
                    await logger.LogMessageAsync($"Add vao db {webhookUrlVideo}");
                    await logger.LogMessageAsync($"Add vao db {webhookSecretkeyVideo}");
                    await StoreResponseResult(result, fileName, systemId);
                    await SendResult(result, logger, webhookSecretkeyVideo, webhookUrlVideo);
                    await logger.LogMessageAsync($"Add vao db {result.RegisteredFaces.Count}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private async Task<string> SendResult(FaceDetectionResult result, CloudWatchLogger logger, string webhookSecretkey, string webhookUrl)
    {
        
        try
        {
            string jsonPayload = ConvertToJson(result);
            await logger.LogMessageAsync(jsonPayload);

            // Tính chữ ký HMAC cho payload
            string signature = GenerateHMAC(jsonPayload, webhookSecretkey);
            // Tạo HttpContent để gửi yêu cầu POST
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            // Thêm header "X-Signature" với chữ ký HMAC
            content.Headers.Add("X-Signature", signature);
            await logger.LogMessageAsync(signature);
            var resultJson = ConvertToJson(result);
            var response = await _httpClient.PostAsync(webhookUrl, content);
            await logger.LogMessageAsync(webhookUrl);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            await logger.LogMessageAsync("Response from API: " + responseContent);
            return resultJson;
        }
        catch(Exception e)
        {
            await logger.LogMessageAsync(e.Message);
        }


        return "";
       
    }

    private static string GenerateHMAC(string payload, string secret)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(hashBytes);
        }
    }

    private async Task<(string?, string?)> CreateResponseResult(string systemId, FaceDetectionResult result)
    {
        var systemName = systemId;
        string? webhookUrl = null;
        string? webhookSecretkey = null;
        var logger = new CloudWatchLogger();
        Dictionary<string, AttributeValue> dictionary = new Dictionary<string, AttributeValue>
        {
              { ":systemName", new AttributeValue { S = systemName } }
        };

        var resultQuery = await GetRecordByAttributeIndex("Accounts", "SystemNameIndex", "SystemName = :systemName", dictionary);
        var firstRecord = resultQuery.Items.FirstOrDefault();

        if (firstRecord != null)
        {
            if (firstRecord.ContainsKey("WebhookUrl") && firstRecord["WebhookUrl"].S != null)
            {
                webhookUrl = firstRecord["WebhookUrl"].S;
            }

            if (firstRecord.ContainsKey("WebhookSecretKey") && firstRecord["WebhookSecretKey"].S != null)
            {
                webhookSecretkey = firstRecord["WebhookSecretKey"].S;
            }

            return (webhookUrl, webhookSecretkey);
        }

        return (null, null);
    }


    private async Task<QueryResponse> GetRecordByAttributeIndex(string tableName, string indexName, string keyConditionExpression, Dictionary<string, AttributeValue> dictionary)
    {
        return await _dynamoDbClient.QueryAsync(new QueryRequest
        {
            TableName = tableName,
            IndexName = indexName,
            KeyConditionExpression = keyConditionExpression,
            ExpressionAttributeValues = dictionary
        });
    }


    private async Task<QueryResponse> GetRecordByAttribute(string systemName, string dynamoDbName, string keyConditionExpression, Dictionary<string, AttributeValue> dictionary)
    {
        var queryRequest = new QueryRequest
        {
            TableName = dynamoDbName,
            KeyConditionExpression = keyConditionExpression,
            ExpressionAttributeValues = dictionary,
        };

        return await _dynamoDbClient.QueryAsync(queryRequest);
    }
    private string ConvertToJson<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj);
    }
    private async Task StoreResponseResult(FaceDetectionResult result, string fileName,string systemName)
    {
        string jsonResult = ConvertToJson(result);
        var dictionaryResponseResult = CreateDictionaryFualumniResponeResult(fileName, jsonResult, systemName);
        await CreateNewRecord(Utils.SystemConstants.RESULT_INFO_TABLE_DYNAMODB, dictionaryResponseResult);
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

    private Dictionary<string, AttributeValue> CreateDictionaryFualumniResponeResult(string fileName, string data,string systemName)
    {
        return new Dictionary<string, AttributeValue>
               {
                   {
                       Utils.SystemConstants.FILE_NAME_ATTRIBUTE_DYNAMODB, new AttributeValue
                       {
                           S = fileName
                       }
                   },
                   {
                       Utils.SystemConstants.SYSTEM_NAME_ATTRIBUTE_DYNAMODB, new AttributeValue
                       {
                           S = systemName
                       }
                   },
                   {
                       Utils.SystemConstants.DATA_ATTRIBUTE_DYNAMODB, new AttributeValue
                       {
                           S = data
                       }
                   },
                   {
                       Utils.SystemConstants.CREATE_DATE_ATTRIBUTE_DYNAMODB, new AttributeValue
                       {
                           S = DateTimeUtils.GetDateTimeVietNamNow()
                       }
                   }
               };
    }
    private async Task<FaceDetectionResult> DetectVideoProcess(string bucket, string key, string fileName,string systemId)
    {
        var logger = new CloudWatchLogger();
        string jobId = await StartFaceSearch(bucket, key, systemId);
        await logger.LogMessageAsync($"JobId la {jobId}");
        return await GetFaceSearchResults(jobId, systemId, fileName);
    }
    private async Task<FaceDetectionResult> DetectImageProcess(string bucket,string systemId, string key, string fileName)
    {
        var logger = new CloudWatchLogger();
        try
        {
            

            var resultRegisteredUsers = new List<FaceRecognitionResponse>();
            var resultUnregisteredUsers = new List<FaceRecognitionResponse>();

            var collectionName = systemId;

            var indexFacesResponse = await IndexFaces(bucket, key, collectionName);
            var faceRecords = indexFacesResponse.FaceRecords;
            await logger.LogMessageAsync($"facerecord:{faceRecords.Count}");


            if (faceRecords != null && faceRecords.Count > 0)
            {
                var responseUserFaceId = await FindUserIdByFaceId(faceRecords, collectionName);

                var unregisteredUsers = responseUserFaceId.Item2;
                var registeredUsers = responseUserFaceId.Item1;

                if (unregisteredUsers != null && unregisteredUsers.Count > 0)
                {
                    //await DeleteFaceId(unregisteredUsers, collectionName);

                    foreach (var (faceId, boundingBox) in unregisteredUsers)
                    {
                        var responseObj = CreateResponseObj(fileName, null, boundingBox, faceId, null);
                        resultUnregisteredUsers.Add(responseObj);
                    }
                }

                if (registeredUsers != null && registeredUsers.Count > 0)
                {
                    foreach (var (userId, faceId, boundingBox) in registeredUsers)
                    {
                        await AssociateUser(userId, faceId, collectionName);
                        var dictionary = CreateDictionaryFualumni(userId, faceId, collectionName);
                        await CreateNewRecord(Utils.SystemConstants.FACEID_TABLE_DYNAMODB, dictionary);

                        var responseObj = CreateResponseObj(fileName, null, boundingBox, faceId, userId);
                        resultRegisteredUsers.Add(responseObj);
                    };

                }

                return new FaceDetectionResult
                {
                    FileName = fileName,
                    RegisteredFaces = resultRegisteredUsers,
                    UnregisteredFaces = resultUnregisteredUsers
                };
            }
            else
            {
                throw new Exception("Index face: Cannot detect anyone");
            }
        }catch(Exception ex) {
            await logger.LogMessageAsync($"image process:{ex.Message}");
        }
        return null;
    }
    private Dictionary<string, AttributeValue> CreateDictionaryFualumni(string userId, string faceId, string systemName)
    {
        return new Dictionary<string, AttributeValue>
               {
                   {
                       Utils.SystemConstants.USER_ID_ATTRIBUTE_DYNAMODB, new AttributeValue
                       {
                           S = userId
                       }
                   },
                   {
                       Utils.SystemConstants.SYSTEM_NAME_ATTRIBUTE_DYNAMODB, new AttributeValue
                       {
                           S = systemName
                       }
                   },
                   {
                       Utils.SystemConstants.FACE_ID_ATTRIBUTE_DYNAMODB, new AttributeValue
                       {
                           S = faceId
                       }
                   },
                   {
                       Utils.SystemConstants.CREATE_DATE_ATTRIBUTE_DYNAMODB, new AttributeValue
                       {
                           S = DateTimeUtils.GetDateTimeVietNamNow()
                       }
                   }
               };
    }
    private FaceRecognitionResponse CreateResponseObj(string fileName, string? timeAppearances, Models.BoundingBox? boundingBox, string faceId, string? userId)
    {
        return new FaceRecognitionResponse
        {
            TimeAppearances = timeAppearances,
            BoundingBox = boundingBox,
            FaceId = faceId,
            UserId = userId
        };
    }
    private async Task<string> StartFaceSearch(string s3BucketName, string videoFileName, string systemId)
    {
        var startFaceSearchRequest = new StartFaceSearchRequest
        {
            CollectionId = systemId,
            Video = new Video
            {
                S3Object = new Amazon.Rekognition.Model.S3Object
                {
                    Bucket = s3BucketName,
                    Name = videoFileName
                }
            },
            FaceMatchThreshold = (float)ConfidenceLevel.High,
        };

        var startFaceSearchResponse = await _rekognitionClient.StartFaceSearchAsync(startFaceSearchRequest);

        return startFaceSearchResponse.JobId;
    }
    private async Task<FaceDetectionResult> GetFaceSearchResults(string jobId, string collectionId, string fileName)
    {
        GetFaceSearchRequest getFaceSearchRequest = new GetFaceSearchRequest
        {
            JobId = jobId
        };

        GetFaceSearchResponse faceSearchResponse;
        var faceIdDict = new Dictionary<string, (double Confidence, long Timestamp)>();
        var logger = new CloudWatchLogger();
        try
        {
            do
            {
                faceSearchResponse = await _rekognitionClient.GetFaceSearchAsync(getFaceSearchRequest);

                if (faceSearchResponse.JobStatus == VideoJobStatus.SUCCEEDED)
                {
                    foreach (var personMatch in faceSearchResponse.Persons)
                    {
                        if (personMatch.FaceMatches != null)
                        {
                            foreach (var faceMatch in personMatch.FaceMatches)
                            {
                                string faceId = faceMatch.Face.FaceId;
                                double confidence = faceMatch.Similarity;
                                long timestamp = personMatch.Timestamp;


                                if (faceIdDict.TryGetValue(faceId, out var existingMatch))
                                {

                                    if (confidence > existingMatch.Confidence)
                                    {
                                        faceIdDict[faceId] = (confidence, timestamp);
                                    }
                                }
                                else
                                {
                                    faceIdDict[faceId] = (confidence, timestamp);
                                }
                            }
                        }
                    }
                }
                else if (faceSearchResponse.JobStatus == VideoJobStatus.FAILED)
                {
                    await logger.LogMessageAsync($"Face search failed");
                    break;
                }

            } while (faceSearchResponse.JobStatus == VideoJobStatus.IN_PROGRESS);
        }
        catch (Exception ex)
        {
            await logger.LogMessageAsync($"Error while processing face search results: {ex.Message}");
            return new FaceDetectionResult();
        }

        return await FindListUserIdInVideo(faceIdDict, collectionId, fileName);
    }
    private async Task<FaceDetectionResult> FindListUserIdInVideo(Dictionary<string, (double Confidence, long Timestamp)> faceIdDict, string collectionId, string fileName)
    {
        var userList = new List<FaceRecognitionResponse>();
        var uniqueUserIds = new HashSet<string>();
        var logger = new CloudWatchLogger();
        foreach (var faceIdEntry in faceIdDict)
        {
            string faceId = faceIdEntry.Key;
            long timestamp = faceIdEntry.Value.Timestamp;

            try
            {

                string userId = await SearchUserByFaceId(faceId, collectionId);

                if (!string.IsNullOrEmpty(userId) && uniqueUserIds.Add(userId))
                {
                    string formattedTimestamp = timestamp.ToString();
                    userList.Add(new FaceRecognitionResponse
                    {
                        TimeAppearances = formattedTimestamp,
                        UserId = userId,
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user for faceId {faceId}: {ex.Message}");
                continue;
            }
        }
        await logger.LogMessageAsync($"Tra ket qua ve db");
        return new FaceDetectionResult
        {
            FileName = fileName,
            RegisteredFaces = userList,
            UnregisteredFaces = new List<FaceRecognitionResponse>()
        };
    }
    private async Task<string> SearchUserByFaceId(string faceId, string collectionId)
    {
        var response = await _rekognitionClient.SearchUsersAsync(new SearchUsersRequest
        {
            CollectionId = collectionId,
            MaxUsers = 1,
            FaceId = faceId,
            UserMatchThreshold = (float)ConfidenceLevel.High,
        });

        string faceModelVersion = response.FaceModelVersion;
        SearchedUser searchedUser = response.SearchedUser;
        List<UserMatch> userMatches = response.UserMatches;

        if (userMatches.Count == 1)
        {
            return userMatches.First().User.UserId;
        }
        Console.WriteLine("SearchUserByFaceId: More than 1 user found, or none");
        return "";
    }
    private async Task DeleteFaceId(List<(string, Models.BoundingBox)> faceIds, string collectionName)
    {
        await _rekognitionClient.DeleteFacesAsync(new DeleteFacesRequest
        {
            CollectionId = collectionName,
            FaceIds = faceIds.Select(f => f.Item1).ToList()
        });
    }
    private async Task CreateNewRecord(string tableName, Dictionary<string, AttributeValue> dictionary)
    {
        var request = new PutItemRequest
        {
            TableName = tableName,
            Item = dictionary,
        };

        await _dynamoDbClient.PutItemAsync(request);
    }
    private async Task<string> CreateUser(string userId, string collectionName)
    {
        Console.WriteLine(userId);
        var response = await _rekognitionClient.CreateUserAsync(new CreateUserRequest
        {
            CollectionId = collectionName,
            UserId = userId
        });

        return userId;
    }
    private async Task<AssociateFacesResponse> AssociateUser(string userId, string faceId, string collectionName)
    {
        var logger = new CloudWatchLogger(); // Initialize your custom CloudWatchLogger
        
        var listFaceIDs = await GetFaceIdsByUserIdAsync(userId, collectionName);
        var existingFaceId = await GetFaceIdForUserAndFaceAsync(userId, faceId, collectionName);
        await logger.LogMessageAsync($"Existing face id : {existingFaceId}");
        if (listFaceIDs.Count >= 100 && existingFaceId == null)
        {
            await logger.LogMessageAsync($"More than 100");
            // Get the oldest face record
            var oldestFaceId = await GetOldestFaceIdForUserAsync(userId, collectionName);

            // Disassociate and delete the oldest face
            await DisassociateAndDeleteOldestFaceAsync(userId, oldestFaceId, collectionName);
        }
        else if(faceId.CompareTo(existingFaceId) == 0)
        {
            await logger.LogMessageAsync($"Trung mat : {faceId}");
            return new AssociateFacesResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                // Optional: You can set other fields as needed
            };
        }

        var response = await _rekognitionClient.AssociateFacesAsync(new AssociateFacesRequest
        {
            CollectionId = collectionName,
            FaceIds = new List<string> {
                faceId
            },
            UserId = userId,
            UserMatchThreshold = (float)ConfidenceLevel.High,
        });
        return response;
    }

    private async Task<string> GetFaceIdForUserAndFaceAsync(string userId, string faceid, string systemName)
    {
        try
        {
            var queryRequest = new QueryRequest
            {
                TableName = Utils.SystemConstants.FACEID_TABLE_DYNAMODB,
                KeyConditionExpression = "UserId = :userId AND FaceId = :faceId",
                FilterExpression = $"{Utils.SystemConstants.SYSTEM_NAME_ATTRIBUTE_DYNAMODB} = :systemName",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":userId", new AttributeValue { S = userId } },
                        { ":faceId", new AttributeValue { S = faceid } },
                        { ":systemName", new AttributeValue { S = systemName } }
                    }
            };

            var queryResponse = await _dynamoDbClient.QueryAsync(queryRequest);

            // If a record exists, return the FaceId, otherwise return null
            return queryResponse.Items.Count > 0 ? queryResponse.Items.First()["FaceId"].S : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetFaceIdForUserAndFaceAsync: {ex.Message}");
            throw;
        }
    }
    private async Task AssociateUser(List<(string, string, string)> userFaceIds, string collectionName)
    {

        foreach (var (userId, faceId, imageId) in userFaceIds)
        {
            
            var response = await _rekognitionClient.AssociateFacesAsync(new AssociateFacesRequest
            {
                CollectionId = collectionName,
                FaceIds = new List<string> {
                        faceId
                },
                UserId = userId,
                UserMatchThreshold = (float)ConfidenceLevel.High,
            });
        }
    }

    private async Task DisassociateAndDeleteOldestFaceAsync(string userId, string faceId, string collectionName)
    {
        // Disassociate the oldest face from Rekognition
        await _rekognitionClient.DisassociateFacesAsync(new DisassociateFacesRequest
        {
            CollectionId = collectionName,
            FaceIds = new List<string> { faceId },
            UserId = userId
        });

        var key = new Dictionary<string, AttributeValue>
        {
            {
                Utils.SystemConstants.USER_ID_ATTRIBUTE_DYNAMODB, new AttributeValue
                {
                    S = userId
                }
            },
            {
                Utils.SystemConstants.FACE_ID_ATTRIBUTE_DYNAMODB, new AttributeValue
                {
                    S = faceId
                }
            }
        };

        var request = new DeleteItemRequest
        {
            TableName = Utils.SystemConstants.FACEID_TABLE_DYNAMODB, // Sử dụng bảng từ GlobalVarians
            Key = key
        };

        var response = await _dynamoDbClient.DeleteItemAsync(request);
    }

    private async Task<string> GetOldestFaceIdForUserAsync(string userId, string collectionName)
    {
        var queryRequest = new QueryRequest
        {
            TableName = Utils.SystemConstants.FACEID_TABLE_DYNAMODB,
            IndexName = Utils.SystemConstants.FACEID_INDEX_ATTRIBUTE_DYNAMODB,
            KeyConditionExpression = "UserId = :userId AND SystemName = :systemName", // Sử dụng cả UserId và SystemName
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":userId", new AttributeValue { S = userId } },
                    { ":systemName", new AttributeValue { S = collectionName } }
                }
        };

        var queryResponse = await _dynamoDbClient.QueryAsync(queryRequest);

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

    public async Task<List<string>> GetFaceIdsByUserIdAsync(string userId, string systemId)
    {
        try
        {
            var request = new QueryRequest
            {
                TableName = Utils.SystemConstants.FACEID_TABLE_DYNAMODB,
                IndexName = Utils.SystemConstants.FACEID_INDEX_ATTRIBUTE_DYNAMODB,
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
            var response = await _dynamoDbClient.QueryAsync(request);

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
    private async Task<IndexFacesResponse> IndexFaces(string bucket, string key, string collectionName)
    {
        var request = new IndexFacesRequest
        {
            Image = new Amazon.Rekognition.Model.Image
            {
                S3Object = new Amazon.Rekognition.Model.S3Object
                {
                    Bucket = bucket,
                    Name = key
                }
            },
            CollectionId = collectionName
        };

        return await _rekognitionClient.IndexFacesAsync(request);
    }
    private bool CheckContentType(GetObjectMetadataResponse metadataResponse)
    {
        var contentType = metadataResponse.Metadata[Utils.SystemConstants.CONTENT_TYPE];

        if (contentType.Contains(Utils.SystemConstants.VIDEO))
        {
            return true;
        }
        else if (contentType.Contains(Utils.SystemConstants.IMAGE))
        {
            return false;
        }
        else
        {
            throw new Exception("wrong content type, làm éo gì có content type m chọn hả hưng");
        }
    }
    private async Task<(List<(string, string, Models.BoundingBox)>, List<(string, Models.BoundingBox)>)> FindUserIdByFaceId(List<FaceRecord> faceRecords, string collectionName)
    {
        List<(string, string, Models.BoundingBox)> registeredUsers = new();
        List<(string, Models.BoundingBox)> unregisteredUser = new();

        foreach (var faceRecord in faceRecords)
        {
            var faceId = faceRecord.Face.FaceId;
            var imageId = faceRecord.Face.ImageId;
            var boundingBox = faceRecord.Face.BoundingBox;
            Models.BoundingBox b = new Models.BoundingBox
            {
                Left = boundingBox.Left,
                Top = boundingBox.Top,
                Width = boundingBox.Width,
                Height = boundingBox.Height
            };

            var userResponse = await _rekognitionClient.SearchUsersAsync(new SearchUsersRequest
            {
                CollectionId = collectionName,
                FaceId = faceId,
                UserMatchThreshold = (float)ConfidenceLevel.High,
            });

            if (userResponse.UserMatches.Count > 0)
            {
                registeredUsers.Add((userResponse.UserMatches[0].User.UserId, faceId, b));
            }
            else
            {
                unregisteredUser.Add((faceId, b));
            }
        }

        return (registeredUsers, unregisteredUser);
    }
}
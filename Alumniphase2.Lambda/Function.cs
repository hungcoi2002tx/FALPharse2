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
                context.Logger.LogError("No S3 record found in the event.");
                return;
            }

            var bucket = s3Record.Bucket.Name;
            var key = s3Record.Object.Key;

            var metadataResponse = await _s3Client.GetObjectMetadataAsync(bucket, key);
            var contentType = CheckContentType(metadataResponse);
            var fileName = metadataResponse.Metadata[Utils.Constants.ORIGINAL_FILE_NAME];

            switch (contentType)
            {
                case (false):
                    result = await DetectImageProcess(bucket, key, fileName);
                    var (webhookUrlImage, webhookSecretkeyImage) = await CreateResponseResult(bucket, result);
                    await SendResult(result, logger, webhookSecretkeyImage, webhookUrlImage);
                    await StoreResponseResult(result, fileName);
                    await logger.LogMessageAsync(DateTimeUtils.GetDateTimeVietNamNow());
                    break;
                case (true):
                    result = await DetectVideoProcess(bucket, key, fileName);
                    var (webhookUrlVideo, webhookSecretkeyVideo) = await CreateResponseResult(bucket, result);
                    await logger.LogMessageAsync($"Add vao db {webhookUrlVideo}");
                    await logger.LogMessageAsync($"Add vao db {webhookSecretkeyVideo}");
                    await SendResult(result, logger, webhookSecretkeyVideo, webhookUrlVideo);
                    await StoreResponseResult(result, fileName);
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
        string jsonPayload = ConvertToJson(result);
        await logger.LogMessageAsync(jsonPayload);

        // Tính chữ ký HMAC cho payload
        string signature = GenerateHMAC(jsonPayload, webhookSecretkey);
        // Tạo HttpContent để gửi yêu cầu POST
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        // Thêm header "X-Signature" với chữ ký HMAC
        content.Headers.Add("X-Signature", signature);

        var resultJson = ConvertToJson(result);
        var response = await _httpClient.PostAsync(webhookUrl, content);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        await logger.LogMessageAsync("Response from API: " + responseContent);

        return resultJson;
    }

    private static string GenerateHMAC(string payload, string secret)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(hashBytes);
        }
    }

    private async Task<(string?, string?)> CreateResponseResult(string bucket, FaceDetectionResult result)
    {
        var systemName = bucket;
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
    private async Task StoreResponseResult(FaceDetectionResult result, string fileName)
    {
        string jsonResult = ConvertToJson(result);
        var dictionaryResponseResult = CreateDictionaryFualumniResponeResult(fileName, jsonResult);
        await CreateNewRecord(Utils.Constants.FUALUMNI_RESPONSE_RESULT_TABLE, dictionaryResponseResult);
    }
    private Dictionary<string, AttributeValue> CreateDictionaryFualumniResponeResult(string fileName, string data)
    {
        return new Dictionary<string, AttributeValue>
               {
                   {
                       Utils.Constants.FILE_NAME_ATTRIBUTE_DYNAMODB, new AttributeValue
                       {
                           S = fileName
                       }
                   },
                   {
                       Utils.Constants.DATA_ATTRIBUTE_DYNAMODB, new AttributeValue
                       {
                           S = data
                       }
                   },
                   {
                       Utils.Constants.CREATE_DATE_ATTRIBUTE_DYNAMODB, new AttributeValue
                       {
                           S = DateTimeUtils.GetDateTimeVietNamNow()
                       }
                   }
               };
    }
    private async Task<FaceDetectionResult> DetectVideoProcess(string bucket, string key, string fileName)
    {
        var logger = new CloudWatchLogger();
        string jobId = await StartFaceSearch(bucket, key);
        await logger.LogMessageAsync($"JobId la {jobId}");
        return await GetFaceSearchResults(jobId, bucket, fileName);
    }
    private async Task<FaceDetectionResult> DetectImageProcess(string bucket, string key, string fileName)
    {
        var resultRegisteredUsers = new List<FaceRecognitionResponse>();
        var resultUnregisteredUsers = new List<FaceRecognitionResponse>();

        var collectionName = bucket;
        var dynamoDbName = bucket;

        var indexFacesResponse = await IndexFaces(bucket, key, collectionName);
        var faceRecords = indexFacesResponse.FaceRecords;

        if (faceRecords != null && faceRecords.Count > 0)
        {
            var responseUserFaceId = await FindUserIdByFaceId(faceRecords, collectionName);

            var unregisteredUsers = responseUserFaceId.Item2;
            var registeredUsers = responseUserFaceId.Item1;

            if (unregisteredUsers != null && unregisteredUsers.Count > 0)
            {
                await DeleteFaceId(unregisteredUsers, collectionName);

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
                    var dictionary = CreateDictionaryFualumni(userId, faceId);
                    await CreateNewRecord(dynamoDbName, dictionary);

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
    }
    private Dictionary<string, AttributeValue> CreateDictionaryFualumni(string userId, string faceId)
    {
        return new Dictionary<string, AttributeValue>
               {
                   {
                       Utils.Constants.USER_ID_ATTRIBUTE_DYNAMODB, new AttributeValue
                       {
                           S = userId
                       }
                   },
                   {
                       Utils.Constants.FACE_ID_ATTRIBUTE_DYNAMODB, new AttributeValue
                       {
                           S = faceId
                       }
                   },
                   {
                       Utils.Constants.CREATE_DATE_ATTRIBUTE_DYNAMODB, new AttributeValue
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
    private async Task<string> StartFaceSearch(string s3BucketName, string videoFileName)
    {
        var startFaceSearchRequest = new StartFaceSearchRequest
        {
            CollectionId = s3BucketName,
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
        var contentType = metadataResponse.Metadata[Utils.Constants.CONTENT_TYPE];

        if (contentType.Contains(Utils.Constants.VIDEO))
        {
            return true;
        }
        else if (contentType.Contains(Utils.Constants.IMAGE))
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
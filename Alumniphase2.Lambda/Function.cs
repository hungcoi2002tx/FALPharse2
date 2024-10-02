using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.S3;
using Amazon.S3.Model;
using System.Reflection.Emit;
using static System.Net.Mime.MediaTypeNames;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Alumniphase2.Lambda;

public class Function
{
    public const string TRAIN_PROCESS = "Training";
    public const string DETECT_PROCESS = "Tagging";
    public const string TYPE_OF_REQUEST = "TypeOfRequest";
    public const string CONTENT_TYPE = "ContentType";

    public const string VIDEO = "Video";
    public const string IMAGE = "Image";

    public const string USER_ID_ATTRIBUTE = "UserId";

    public const string USER_ID_ATTRIBUTE_DYNAMODB = "UserId";
    public const string FACE_ID_ATTRIBUTE_DYNAMODB = "FaceId";
    public const string UPLOAD_DATE_ATTRIBUTE_DYNAMODB = "UploadDate";
    public const string IMAGE_ID_ATTRIBUTE_DYNAMODB = "ImageId";

    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonRekognition _rekognitionClient;
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public Function()
    {
        _s3Client = new AmazonS3Client();
        _rekognitionClient = new AmazonRekognitionClient();
        _dynamoDbClient = new AmazonDynamoDBClient();
    }

    public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        try
        {
            foreach (var record in evnt.Records)
            {
                var s3Record = record.S3;

                if (s3Record == null)
                {
                    context.Logger.LogError("No S3 record found in the event.");
                    return;
                }

                var bucket = s3Record.Bucket.Name;
                var key = s3Record.Object.Key;

                var metadataResponse = await _s3Client.GetObjectMetadataAsync(bucket, key);
                var contentType = CheckContentType(metadataResponse);
                var process = CheckProcess(metadataResponse);

                switch ((contentType, process))
                {
                    case (false, true):
                        await TrainImageProcess(bucket, key, metadataResponse);
                        break;
                    case (false, false):
                        await DetectImageProcess(bucket, key, metadataResponse);
                        break;
                    case (true, false):
                        await DetectVideoProcess(bucket, key);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private async Task DetectVideoProcess(string bucket, string key)
    {
        List<(string, string)> listUserIds = new List<(string, string)>();
        string jobId = await StartFaceSearch(bucket, key);

        listUserIds = await GetFaceSearchResults(jobId, bucket);
        // Convert the list of tuples to a formatted string
        var formattedResult = string.Join(", ", listUserIds.Select(tuple => $"({tuple.Item1}, {tuple.Item2})"));
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
            FaceMatchThreshold = 80
        };

        var startFaceSearchResponse = await _rekognitionClient.StartFaceSearchAsync(startFaceSearchRequest);

        return startFaceSearchResponse.JobId;
    }

    private async Task<List<(string userId, string timestamp)>> GetFaceSearchResults(string jobId, string collectionId)
    {
        GetFaceSearchRequest getFaceSearchRequest = new GetFaceSearchRequest
        {
            JobId = jobId
        };

        GetFaceSearchResponse faceSearchResponse;
        var faceIdDict = new Dictionary<string, (double Confidence, long Timestamp)>();

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
                    Console.WriteLine("Face search job failed.");
                    break;
                }

            } while (faceSearchResponse.JobStatus == VideoJobStatus.IN_PROGRESS);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while processing face search results: {ex.Message}");
            return new List<(string, string)>(); 
        }

        var userList = new List<(string userId, string timestamp)>();
        var uniqueUserIds = new HashSet<string>();

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
                    userList.Add((userId, formattedTimestamp));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user for faceId {faceId}: {ex.Message}");
                continue;
            }
        }

        return userList;
    }


    private async Task<string> SearchUserByFaceId(string faceId, string collectionId)
    {
        var response = await _rekognitionClient.SearchUsersAsync(new SearchUsersRequest
        {
            CollectionId = collectionId,
            MaxUsers = 1,
            FaceId = faceId,
            UserMatchThreshold = 80
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



    private async Task TrainImageProcess(string bucket, string key, GetObjectMetadataResponse metadataResponse)
    {
        var collectionName = bucket;
        var dynamoDbName = bucket;
        var userId = metadataResponse.Metadata[USER_ID_ATTRIBUTE];

        var response = await IndexFaces(bucket, key, collectionName);

        if (response.HttpStatusCode == System.Net.HttpStatusCode.OK && response.FaceRecords.Count > 0)
        {
            foreach (var faceRecord in response.FaceRecords)
            {
                var faceId = faceRecord.Face.FaceId;
                var imageId = faceRecord.Face.ImageId;

                var itemResponse = await GetRecordByUserId(userId, dynamoDbName);

                if (itemResponse.Items.Count > 0)
                {
                    throw new Exception("User Id is duplicate!");
                }
                else
                {
                    await CreateUser(userId, collectionName);

                    await AssociateUser(userId, faceId, collectionName);

                    var metadata = metadataResponse.Metadata;

                    if (metadata != null && metadata.Count > 0)
                    {
                        await CreateNewRecord(dynamoDbName, userId, imageId, faceId);
                    }
                }
            }
        }
        else
        {
            throw new Exception("index face failed");
        }
    }

    private async Task DetectImageProcess(string bucket, string key, GetObjectMetadataResponse metadataResponse)
    {
        var collectionName = bucket;
        var dynamoDbName = bucket;


        var indexFacesResponse = await IndexFaces(bucket, key, collectionName);
        var responseUserFaceId = await FindUserIdByFaceId(indexFacesResponse.FaceRecords, collectionName);
        var faceIdsDelete = responseUserFaceId.Item2;

        if (faceIdsDelete != null && faceIdsDelete.Count > 0)
        {
            await DeleteFaceId(faceIdsDelete, collectionName);
        }
        await AssociateUser(responseUserFaceId.Item1, collectionName);
        foreach (var (userId, faceId, imageId) in responseUserFaceId.Item1)
        {
            await CreateNewRecord(dynamoDbName, userId, imageId, faceId);
        }

        SearchFacesResponse searchFacesResponse = null!;

        foreach (var faceRecord in indexFacesResponse.FaceRecords)
        {
            searchFacesResponse = await _rekognitionClient.SearchFacesAsync(new SearchFacesRequest()
            {
                CollectionId = collectionName,
                FaceId = faceRecord.Face.FaceId,
                FaceMatchThreshold = 70F
            });

            foreach (FaceMatch face in searchFacesResponse.FaceMatches)
            {
                var userResponse = await _rekognitionClient.SearchUsersAsync(new SearchUsersRequest
                {
                    CollectionId = collectionName,
                    FaceId = face.Face.FaceId,
                    UserMatchThreshold = 70
                });

                foreach (var user in userResponse.UserMatches)
                {
                    var itemResponse = await GetRecordByUserId(user.User.UserId, dynamoDbName);

                    if (itemResponse != null)
                    {
                        var item = itemResponse.Items[0]; 

                        string faceId = item["FaceId"].S;
                        string imageId = item["ImageId"].S;

                        Console.WriteLine($"FaceId: {faceId}, ImageId: {imageId}");
                    }
                    else
                    {
                        throw new Exception("Cannot detect anyone");
                    }
                }
            }
        }
    }

    private async Task<QueryResponse> GetRecordByUserId(string userId, string dynamoDbName)
    {
        //var getItemRequest = new GetItemRequest
        //{
        //    TableName = dynamoDbName,
        //    Key = new Dictionary<string, AttributeValue>
        //    {
        //        {
        //            USER_ID_ATTRIBUTE_DYNAMODB, new AttributeValue {
        //                S = userId
        //            }
        //        }
        //    }
        //};


        var queryRequest = new QueryRequest
        {
            TableName = dynamoDbName,
            KeyConditionExpression = "UserId = :v_userId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_userId", new AttributeValue { S = userId } }
            },
            Limit = 1
        };

        return await _dynamoDbClient.QueryAsync(queryRequest);
    }

    private async Task DeleteFaceId(List<string> faceIds, string collectionName)
    {
        await _rekognitionClient.DeleteFacesAsync(new DeleteFacesRequest
        {
            CollectionId = collectionName,
            FaceIds = faceIds
        });
    }


    private async Task CreateNewRecord(string tableName, string userId, string imageId, string faceId)
    {
        var request = new PutItemRequest
        {
            TableName = tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                {
                    USER_ID_ATTRIBUTE_DYNAMODB, new AttributeValue
                    {
                        S = userId
                    }
                },
                 {
                    FACE_ID_ATTRIBUTE_DYNAMODB, new AttributeValue
                    {
                        S = faceId
                    }
                },
                {
                    UPLOAD_DATE_ATTRIBUTE_DYNAMODB, new AttributeValue
                    {
                        S = DateTime.Now.ToString()
                    }
                },
                {
                    IMAGE_ID_ATTRIBUTE_DYNAMODB, new AttributeValue
                    {
                        S = imageId
                    }
                },
            }
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
            UserMatchThreshold = 70
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
                UserMatchThreshold = 70
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
        var contentType = metadataResponse.Metadata[CONTENT_TYPE];

        if (contentType.Contains(VIDEO))
        {
            return true;
        }
        else if (contentType.Contains(IMAGE))
        {
            return false;
        }
        else
        {
            throw new Exception("wrong content type, làm éo gì có content type m chọn hả hưng");
        }
    }
    private bool CheckProcess(GetObjectMetadataResponse metadataResponse)
    {
        var methods = metadataResponse.Metadata[TYPE_OF_REQUEST];

        if (methods == TRAIN_PROCESS)
        {
            return true;
        }
        else if (methods == DETECT_PROCESS)
        {

            return false;
        }
        else
        {
            throw new Exception("wrong process, làm éo gì có process m chọn hả hưng");
        }
    }
    private async Task<(List<(string, string, string)>, List<string>)> FindUserIdByFaceId(List<FaceRecord> faceRecords, string collectionName)
    {
        List<(string, string, string)> responseUserFaceId = new();
        List<string> responseFaceId = new();

        foreach (var faceRecord in faceRecords)
        {
            var faceId = faceRecord.Face.FaceId;
            var imageId = faceRecord.Face.ImageId;
            var userResponse = await _rekognitionClient.SearchUsersAsync(new SearchUsersRequest
            {
                CollectionId = collectionName,
                FaceId = faceId,
                UserMatchThreshold = 70
            });

            if (userResponse.UserMatches.Count > 0)
            {
                responseUserFaceId.Add((userResponse.UserMatches[0].User.UserId, faceId, imageId));
            }
            else
            {
                responseFaceId.Add(faceId);
            }
        }

        return (responseUserFaceId, responseFaceId);
    }
}
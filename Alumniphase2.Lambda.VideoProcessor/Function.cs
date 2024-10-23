using Alumniphase2.Lambda.Models;
using Alumniphase2.Lambda.Utils;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Runtime.Internal.Util;
using Amazon.S3;
using Newtonsoft.Json;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Alumniphase2.Lambda.VideoProcessor;

public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonRekognition _rekognitionClient;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    public Function()
    {
        _s3Client = new AmazonS3Client();
        _rekognitionClient = new AmazonRekognitionClient();
        _dynamoDbClient = new AmazonDynamoDBClient();
    }

    public async Task<FaceDetectionResult> FunctionHandler(JsonElement input, ILambdaContext context)
    {
        string bucketName = input.GetProperty("bucketName").GetString();
        string prefixName = input.GetProperty("prefixName").GetString();
        string key = input.GetProperty("key").GetString();
        string objectKey = input.GetProperty("myObjectKey").GetString();
        var result = new FaceDetectionResult();
        result = await DetectVideoProcess(bucketName, key);
        result.FileName = objectKey;
        return result;
    }

    private async Task<FaceDetectionResult> DetectVideoProcess(string bucket, string key)
    {
        var logger = new CloudWatchLogger();
        string jobId = await StartFaceSearch(bucket, key);
        await logger.LogMessageAsync($"JobId la {jobId}");
        return await GetFaceSearchResults(jobId, bucket, key);
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
                        FaceId = faceId,
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
            FileName = "fileName",
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
}

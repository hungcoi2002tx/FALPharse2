using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.S3;
using Amazon.S3.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Alumniphase2.Lambda;

public class Function
{
    public const string TRAIN_PROCESS = "train";
    public const string DETECT_PROCESS = "detect";
    public const string VIDEO = "video";
    public const string IMAGE = "image";
    public const string COLLECTION = "facerecognition_collection";

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
                    case (true, true):

                        break;
                    case (false, true):

                        break;
                    case (false, false):

                        break;
                    case (true, false):
                        List<string> userIdList = await DetectVideoProcess("", bucket, key);
                        break;
                }
            }
        }
        catch (Exception ex)
        {

        }
    }

    private async Task<List<string>> DetectVideoProcess(string collectionId,string bucket, string key)
    {
        string jobId = await StartFaceSearch(collectionId, bucket, key);

        List<string> userIdList = await GetFaceSearchResults(jobId, collectionId);
        return userIdList;
    }

    private async Task<string> StartFaceSearch(string collectionId, string s3BucketName, string videoFileName)
    {
        var startFaceSearchRequest = new StartFaceSearchRequest
        {
            CollectionId = collectionId,
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

    private async Task<List<string>> GetFaceSearchResults(string jobId, string collectionId)
    {
        GetFaceSearchRequest getFaceSearchRequest = new GetFaceSearchRequest
        {
            JobId = jobId
        };

        GetFaceSearchResponse faceSearchResponse;

        Dictionary<string, (double Confidence, long Timestamp)> faceIdDict = new Dictionary<string, (double, long)>();

        do
        {
            faceSearchResponse = await _rekognitionClient.GetFaceSearchAsync(getFaceSearchRequest);

            if (faceSearchResponse.JobStatus == VideoJobStatus.SUCCEEDED)
            {
                foreach (var personMatch in faceSearchResponse.Persons)
                {
                    if (personMatch.FaceMatches != null && personMatch.FaceMatches.Count > 0)
                    {
                        foreach (var faceMatch in personMatch.FaceMatches)
                        {
                            string faceId = faceMatch.Face.FaceId;
                            double confidence = faceMatch.Similarity;
                            long timestamp = personMatch.Timestamp;

                            if (faceIdDict.ContainsKey(faceId))
                            {
                                if (confidence > faceIdDict[faceId].Confidence)
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

            await Task.Delay(5000);

        } while (faceSearchResponse.JobStatus == VideoJobStatus.IN_PROGRESS);



        HashSet<string> userIds = new HashSet<string>();

        foreach (var entry in faceIdDict)
        {
            string userId = await SearchUserByFaceId(entry.Key, collectionId);
            if (!string.IsNullOrEmpty(userId))
            {
                userIds.Add(userId);
            }
        }

        return userIds.ToList();
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
        Console.WriteLine("No user found");
        return "";
    }

    private async Task TrainImageProcess(string bucket, string key)
    {
        var response = await IndexFaces(bucket, key);
    }

    private void DetectImageProcess(GetObjectMetadataResponse metadataResponse)
    {
        if (CheckContentType(metadataResponse))
        {

        }
        else
        {

        }
    }





    private async Task<IndexFacesResponse> IndexFaces(string bucket, string key)
    {
        var request = new IndexFacesRequest
        {
            Image = new Image
            {
                S3Object = new Amazon.Rekognition.Model.S3Object
                {
                    Bucket = bucket,
                    Name = key
                }
            },
            CollectionId = COLLECTION
        };

        return await _rekognitionClient.IndexFacesAsync(request);
    }


    private bool CheckContentType(GetObjectMetadataResponse metadataResponse)

    {
        var contentType = metadataResponse.Headers.ContentType;

        if (contentType == VIDEO)
        {
            return true;
        }
        else if (contentType == IMAGE)
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
        var methods = metadataResponse.Metadata["method"];

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
}
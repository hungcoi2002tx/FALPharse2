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

                        break;
                }
            }
        }
        catch (Exception ex)
        {

        }
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
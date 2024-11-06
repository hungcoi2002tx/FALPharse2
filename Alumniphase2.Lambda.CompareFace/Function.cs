
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Alumniphase2.Lambda.CompareFace;

public class Function
{
    private readonly IAmazonRekognition _rekognitionClient;
    private readonly IAmazonS3 _s3Client;

    public Function()
    {
        _rekognitionClient = new AmazonRekognitionClient();
        _s3Client = new AmazonS3Client();
    }
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {

        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processing message: {message.Body}");

        // Deserialize the message body to get S3 image links and similarity threshold
        var request = JsonSerializer.Deserialize<CompareFaceRequest>(message.Body);

        if (request == null || string.IsNullOrEmpty(request.SourceImageUrl) || string.IsNullOrEmpty(request.TargetImageUrl))
        {
            context.Logger.LogError("Invalid message format or missing image URLs.");
            return;
        }

        try
        {
            // Download the images from S3
            var sourceImageBytes = await DownloadImageFromS3Async(request.SourceImageUrl);
            var targetImageBytes = await DownloadImageFromS3Async(request.TargetImageUrl);

            var resultId = request.ResultId;

            // Prepare Rekognition compare request
            var compareFacesRequest = new CompareFacesRequest
            {
                SourceImage = new Image
                {
                    Bytes = new MemoryStream(sourceImageBytes)
                },
                TargetImage = new Image
                {
                    Bytes = new MemoryStream(targetImageBytes)
                },
                SimilarityThreshold = request.SimilarityThreshold
            };

            // Call CompareFaces on Rekognition
            var response = await _rekognitionClient.CompareFacesAsync(compareFacesRequest);

            //string resultId = Guid.NewGuid().ToString(); // Generate a unique ID for the result
            string payloadString;

            if (response.FaceMatches.Count > 0)
            {
                var similarity = response.FaceMatches[0].Similarity;
                payloadString = $"Faces matched with similarity: {similarity}";

                // Store the result in DynamoDB
                await StoreComparisonResultInDynamoDB(resultId.ToString(), request.SourceImageUrl, request.TargetImageUrl, similarity);

                // TODO: VIẾT HÀM GỌI API TRẢ VỀ ComparisonResult

            }
            else
            {
                payloadString = "No matching faces found.";

                // Store the result in DynamoDB with similarity as null or 0
                await StoreComparisonResultInDynamoDB(resultId.ToString(), request.SourceImageUrl, request.TargetImageUrl, null);

                // TODO: VIẾT HÀM GỌI API TRẢ VỀ ComparisonResult

            }

            // Delete images from S3 after processing
            await DeleteImageFromS3Async(request.SourceImageUrl);
            await DeleteImageFromS3Async(request.TargetImageUrl);

            var sqsClient = new AmazonSQSClient(); // Initialize your SQS client
            var deleteMessageRequest = new DeleteMessageRequest
            {
                QueueUrl = "https://sqs.ap-southeast-1.amazonaws.com/257394496117/CompareFaces", // Replace with your SQS queue URL
                ReceiptHandle = message.ReceiptHandle
            };

            await sqsClient.DeleteMessageAsync(deleteMessageRequest);
            context.Logger.LogInformation("Message deleted from SQS.");
        }
        catch (AmazonRekognitionException ex)
        {
            context.Logger.LogError($"Error calling Rekognition CompareFaces: {ex.Message}");
        }
    }

    private async Task StoreComparisonResultInDynamoDB(string resultId, string sourceImageUrl, string targetImageUrl, float? similarity)
    {
        var dynamoDbClient = new AmazonDynamoDBClient();

        // Get the current timestamp
        var timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601 format

        var item = new Dictionary<string, AttributeValue>

        {
            { "Id", new AttributeValue { S = resultId } },
            { "SourceImageUrl", new AttributeValue { S = sourceImageUrl } },
            { "TargetImageUrl", new AttributeValue { S = targetImageUrl } },
            { "Similarity", new AttributeValue { N = similarity.HasValue ? similarity.Value.ToString() : "0" } },
            { "Timestamp", new AttributeValue { S = timestamp } } // Add the timestamp
        };

        var putItemRequest = new PutItemRequest
        {
            TableName = "compare-result", // Your DynamoDB table name
            Item = item
        };

        await dynamoDbClient.PutItemAsync(putItemRequest);
    }

    // Helper method to download image bytes from S3
    private async Task<byte[]> DownloadImageFromS3Async(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            throw new ArgumentNullException(nameof(imageUrl), "Image URL cannot be null or empty.");
        }

        var uri = new Uri(imageUrl);
        var bucketName = uri.Host.Split('.')[0];  // Extract bucket name from URL
        var key = Uri.UnescapeDataString(uri.AbsolutePath.Substring(1));  // Extract key from URL

        var response = await _s3Client.GetObjectAsync(bucketName, key);
        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    // Helper method to delete image from S3
    private async Task DeleteImageFromS3Async(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            throw new ArgumentNullException(nameof(imageUrl), "Image URL cannot be null or empty.");
        }

        var uri = new Uri(imageUrl);
        var bucketName = uri.Host.Split('.')[0];
        var key = Uri.UnescapeDataString(uri.AbsolutePath.Substring(1));

        await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = key
        });
    }

    public class CompareFaceRequest
    {
        public string SourceImageUrl { get; set; }  // Base64 encoded source image
        public string TargetImageUrl { get; set; }  // Base64 encoded target image
        public float SimilarityThreshold { get; set; }
        public int ResultId { get; set; }
    }

    public class ComparisonResult
    {
        public string? SourceImageUrl { get; set; }
        public string? TargetImageUrl { get; set; }
        public float? Similarity { get; set; }
        public int? ResultId { get; set; }

    }
}
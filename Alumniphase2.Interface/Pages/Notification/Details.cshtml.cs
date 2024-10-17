using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Share.Data;

namespace Alumniphase2.Interface.Pages.Notification
{
    public class DetailsModel : PageModel
    {
        public string? PictureUrl { get; private set; }
        public string Key { get; private set; }
        public int ImageWidth { get; private set; }
        public int ImageHeight { get; private set; }
        private const string _tableName = "client-storeData";
        string bucketName = "fualumni";
        public List<FaceRecognitionResponse> RegisteredFaces { get; private set; } = new List<FaceRecognitionResponse>();
        public List<FaceRecognitionResponse> UnregisteredFaces { get; private set; } = new List<FaceRecognitionResponse>();
        private readonly IAmazonDynamoDB _dynamoDBClient;
        private readonly IAmazonS3 _s3Client;

        public DetailsModel(IAmazonDynamoDB dynamoDBClient, IAmazonS3 s3Client)
        {
            _dynamoDBClient = dynamoDBClient;
            _s3Client = s3Client;
        }

        public async Task OnGetAsync(string fileName)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            var response = await QueryDynamoDBAsync(fileName);

            ProcessResponse(response);
            await GetUrlImageFromS3();
        }

        private async Task GetUrlImageFromS3()
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = Key,
                Expires = DateTime.UtcNow.AddMinutes(30) // URL sẽ hết hạn sau 15 phút
            };
            PictureUrl = await _s3Client.GetPreSignedURLAsync(request);
        }

        private async Task<QueryResponse> QueryDynamoDBAsync(string fileName)
        {
            var queryRequest = new QueryRequest
            {
                TableName = _tableName,
                KeyConditionExpression = "FileName = :v_fileName",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":v_fileName", new AttributeValue { S = fileName } }
                }
            };

            return await _dynamoDBClient.QueryAsync(queryRequest);
        }

        private void ProcessResponse(QueryResponse response)
        {
            if (response.Items.Count == 1)
            {
                var item = response.Items[0];

                if (item.TryGetValue("Data", out var dataValue))
                {
                    // Deserialize the Data column into the specified type
                    var data = JsonConvert.DeserializeObject<FaceDetectionResult>(dataValue.S);
                    ImageWidth = data.Width ?? 0;
                    ImageHeight = data.Height ?? 0;
                    Key = data.Key;
                    RegisteredFaces = data.RegisteredFaces ?? new List<FaceRecognitionResponse>();
                    UnregisteredFaces = data.UnregisteredFaces ?? new List<FaceRecognitionResponse>();
                }
            }
            else if (response.Items.Count == 0)
            {
                throw new Exception("No item found for the given fileName.");
            }
            else
            {
                throw new Exception("Multiple items found for the given fileName. Please ensure fileName is unique.");
            }
        }
    }

}

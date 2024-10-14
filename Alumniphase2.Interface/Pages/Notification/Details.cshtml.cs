using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Share.Data;

namespace Alumniphase2.Interface.Pages.Notification
{
    public class DetailsModel : PageModel
    {
        public string? PictureUrl { get; private set; } 
        public int ImageWidth { get; private set; } 
        public int ImageHeight { get; private set; } 
        private const string _tableName = "client-storeData"; 
        public List<FaceRecognitionResponse> RegisteredFaces { get; private set; } = new List<FaceRecognitionResponse>();
        public List<FaceRecognitionResponse> UnregisteredFaces { get; private set; } = new List<FaceRecognitionResponse>();
        private readonly IAmazonDynamoDB _dynamoDBClient;

        public DetailsModel(IAmazonDynamoDB dynamoDBClient)
        {
            _dynamoDBClient = dynamoDBClient;
        }

        public async Task OnGetAsync(string fileName)
        {
            PictureUrl = $"/images/{fileName}";
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var image = System.Drawing.Image.FromFile(filePath))
            {
                ImageWidth = image.Width;
                ImageHeight = image.Height;
            }
            var response = await QueryDynamoDBAsync(fileName);

            ProcessResponse(response);
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

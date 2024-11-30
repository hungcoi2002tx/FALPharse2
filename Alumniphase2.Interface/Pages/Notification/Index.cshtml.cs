using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Share.DTO;

namespace Alumniphase2.Interface.Pages.Notification
{
    public class IndexModel : PageModel
    {

        public List<ClientStoredData> ListData { get; set; }
        private readonly IAmazonDynamoDB _dynamoDbClient;
        public IndexModel(IAmazonDynamoDB dynamoDbClient)
        {
            _dynamoDbClient = dynamoDbClient;
        }
        public async Task OnGetAsync()
        {
            ListData = new List<ClientStoredData>();

            var tableName = "client-storeData";

            var request = new ScanRequest
            {
                TableName = tableName
            };

            var response = await _dynamoDbClient.ScanAsync(request);

            foreach (var item in response.Items)
            {
                var file = new ClientStoredData
                {
                    FileName = item.ContainsKey("FileName") ? item["FileName"].S : string.Empty,
                    CreateDate = item.ContainsKey("CreateDate") ? item["CreateDate"].S : string.Empty 
                };

                ListData.Add(file);
            }
        }
    }
}

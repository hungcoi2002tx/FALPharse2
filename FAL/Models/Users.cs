using Amazon.DynamoDBv2.DataModel;
namespace FAL.Models;


[DynamoDBTable("Users")]
public class User
{
    [DynamoDBHashKey]  // Khóa chính (partition key)
    public string Username { get; set; }

    public string Password { get; set; }
    public string SystemName { get; set; }
    public string? WebhookUrl { get; set; }
    public string? WebhookSecretKey { get; set; }
    public string Status { get; set; }
}

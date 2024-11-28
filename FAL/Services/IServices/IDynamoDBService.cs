using Amazon.DynamoDBv2.Model;
using Share.Data;
using Share.DTO;

namespace FAL.Services.IServices
{
    public interface IDynamoDBService
    {
        Task<bool> IsExistUserAsync(string systermId, string userId);
        Task<bool> CreateUserInformationAsync(string tableName, string userId, string faceId);
        Task<bool> IsExistFaceIdAsync(string tableName, string faceId);
        Task<string?> GetRecordByKeyConditionExpressionAsync(string systermId, string keyConditionExpression, Dictionary<string, AttributeValue> dictionary);
        Task<bool> DeleteUserInformationAsync(string systermId, string userId, string faceId);
        Task<List<string>> GetFaceIdsByUserIdAsync(string userId, string systemId);
        Task DeleteUserFromDynamoDbAsync(string userId, string systemId);
        Task<string?> GetFaceIdForUserAndFaceAsync(string userId, string faceId, string collectionName);
        Task<string> GetOldestFaceIdForUserAsync(string userId, string collectionName);
        Task DeleteItem(string userId, string faceId, string collectionName);
        Task<FaceDetectionResult> GetWebhookResult(string systermId,string mediaId);
        Task<DetectStatsResponse> GetDetectStats(string v);
        Task<TrainStatsResponse> GetTrainStats(string systermId);
    }
}

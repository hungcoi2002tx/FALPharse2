using Amazon.DynamoDBv2.Model;
using Share.DTO;
using Share.Model;

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
        Task<TrainStatsResponse> GetTrainStats(string systermId,int page, int pageSize,string searchUserId);
        Task<bool> LogRequestAsync(string systemName, RequestTypeEnum requestType, RequestResultEnum status = RequestResultEnum.Unknown, object requestBody = null);
        Task<RequestStatsResponse> GetRequestStats(string systermId);
        Task<PaginatedTrainStatsDetailResponse> GetTrainStatsDetail(string systermId, string userId,int page,int pageSize);
        Task<bool> DeleteTrainStat(string systermId, string userId, string faceId);
    }
}

namespace FAL.Services.IServices
{
    public interface IDynamoDBService
    {
        Task<bool> IsExitUserAsync(string systermId, string userId);
        Task<bool> CreateUserInformationAsync(string tableName, string userId, string faceId);
    }
}

using Share.SystemModel;

namespace FAL.Services.IServices
{
    public interface IS3Service
    {
        Task<bool> IsExistBudgetAsync(string budgetName);
        Task<bool> AddBudgetAsync(string budgetName);
        Task<bool> AddFileToS3Async(IFormFile file, string imageName, string bucketName, TypeOfRequest type, string mediaId, string userId = null);
    }
}

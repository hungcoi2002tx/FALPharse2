using Share.Model;

namespace FAL.Services.IServices
{
    public interface IS3Service
    {
        Task<bool> IsExistBudgetAsync(string budgetName);
        Task<bool> IsAddBudgetAsync(string budgetName);
        Task<bool> AddFileToS3Async(IFormFile file, string imageName, string bucketName, TypeOfRequest type, string mediaId, string systemId, string userId = null);
    }
}

using Share.SystemModel;

namespace FAL.Services.IServices
{
    public interface IS3Service
    {
        Task<bool> IsExistBudget(string budgetName);
        Task<bool> AddFileToS3Async(IFormFile file, string imageName, string bucketName, TypeOfRequest type);
    }
}

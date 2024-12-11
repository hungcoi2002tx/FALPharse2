using Amazon.Rekognition.Model;
using Share.DTO;

namespace FAL.Services.IServices
{
    public interface ICollectionService
    {
        Task<bool> DeleteByUserIdAsync(string id, string systermId);
        Task<bool> IsCollectionExistAsync(string systermId);
        Task<bool> DeleteByFaceIdAsync(string faceId, string systermId);
        Task<bool> DisassociatedFaceAsync(string systermId, string faceId, string userId);
        Task DeleteUserFromRekognitionCollectionAsync(string systemId, string userId);
        Task<bool> CreateCollectionAsync(string systermId);
        Task<IndexFacesResponse> IndexFaceAsync(string systermId, string bucketName, string imageName, string key = null);
        Task<IndexFacesResponse> IndexFaceByFileAsync(Image file, string systermId, string key = null);
        Task<DetectFacesResponse> DetectFaceByFileAsync(IFormFile file);
        Task<DetectFacesResponse> DetectFaceByFileAsync(Image file);
        Task<bool> AssociateFacesAsync(string systermId, List<string> faceIds, string key);
        Task<bool> IsUserExistByUserIdAsync(string systermId, string userId);
        Task<bool> CreateNewUserAsync(string systermId, string userId);
        Task<SearchUsersResponse> SearchUserByFaceIdsAsync(string systermId, string faceId);
        Task<List<Face>> GetFacesAsync(string systermId);
        Task<List<string>> GetCollectionAsync(string systermId);
        Task<bool> CreateCollectionByIdAsync(string collectionId);
        Task<bool> DeleteFromCollectionAsync(string userId, string systermId);
        Task<CollectionChartStats> GetCollectionChartStats(string systermId, string year);
    }
}

﻿using Share.Data;
using Share.SearchModel;

namespace FAL.Services.IServices
{
    public interface IDynamoDBService
    {
        Task<bool> IsExistUserAsync(string systermId, string userId);
        Task<bool> CreateUserInformationAsync(string tableName, string userId, string faceId);
        Task<bool> IsExistFaceIdAsync(string tableName, string faceId);
        Task<List<FaceTrainModel>> GetListFaceTrainModelAsync(FaceSearchModel search);
    }
}

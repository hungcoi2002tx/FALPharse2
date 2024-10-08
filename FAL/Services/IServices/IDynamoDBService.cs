﻿using Amazon.DynamoDBv2.Model;

namespace FAL.Services.IServices
{
    public interface IDynamoDBService
    {
        Task<bool> IsExistUserAsync(string systermId, string userId);
        Task<bool> CreateUserInformationAsync(string tableName, string userId, string faceId);
        Task<bool> IsExistFaceIdAsync(string tableName, string faceId);
        Task<string?> GetRecordByKeyConditionExpressionAsync(string systermId, string keyConditionExpression, Dictionary<string, AttributeValue> dictionary);
    }
}

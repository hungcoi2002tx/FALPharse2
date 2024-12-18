﻿using Amazon.DynamoDBv2.Model;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Runtime.Internal.Util;
using FAL.Services.IServices;
using Share.DTO;
using Share.Model;
using Share.Utils;
using System.Reflection;

namespace FAL.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly IAmazonRekognition _rekognitionClient;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly CustomLog _logger;
        public CollectionService(IAmazonRekognition rekognitionClient, CustomLog logger, IDynamoDBService dynamoDBService)
        {
            _rekognitionClient = rekognitionClient;
            _logger = logger;
            _dynamoDBService = dynamoDBService;
            _dynamoDBService = dynamoDBService;
        }

        public async Task<bool> CreateCollectionAsync(string systermId)
        {
            try
            {
                // Tạo yêu cầu CreateCollectionRequest
                var request = new CreateCollectionRequest
                {
                    CollectionId = systermId // Tên của collection muốn tạo
                };

                // Gửi yêu cầu tạo collection
                var response = await _rekognitionClient.CreateCollectionAsync(request);

                if (response.StatusCode != (int)System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Collection '{systermId}' fail to create");
                }
                return true;
            }
            catch (InvalidParameterException ex)
            {
                throw new Exception(message: "Collection id invalid.");
            }
            catch (ResourceAlreadyExistsException ex)
            {
                throw new Exception(message: "Collection already exists.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteByFaceIdAsync(string faceId, string systermId)
        {
            try
            {
                var deleteRequest = new DeleteFacesRequest
                {
                    CollectionId = systermId,
                    FaceIds = new List<string> { faceId }
                };

                var respone = await _rekognitionClient.DeleteFacesAsync(deleteRequest);

                if (respone.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new Exception(message: "Lỗi request xóa faceid trong collection");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> DeleteByUserIdAsync(string userId, string systermId)
        {
            try
            {
                if (string.IsNullOrEmpty(systermId) || string.IsNullOrEmpty(userId))
                {
                    throw new Exception(message: "Invalid parameter");
                }
                string paginationToken = null;
                do
                {
                    // Tạo yêu cầu ListFacesRequest để liệt kê các khuôn mặt
                    var request = new ListFacesRequest
                    {
                        CollectionId = systermId,
                        MaxResults = 10, // Giới hạn kết quả trả về mỗi lần (tối đa 4096)
                        NextToken = paginationToken
                    };

                    // Gửi yêu cầu
                    var response = await _rekognitionClient.ListFacesAsync(request);

                    // Duyệt qua từng khuôn mặt trong collection
                    foreach (var face in response.Faces)
                    {
                        // Kiểm tra (userId)
                        if (face.UserId == userId)
                        {
                            //gỡ tag
                            if (face.UserId != null)
                            {
                                await DisassociatedFaceAsync(systermId, face.FaceId, userId);
                            }

                            await DeleteByFaceIdAsync(face.FaceId, systermId);
                        }
                        // Cập nhật token để lấy thêm kết quả nếu có
                        paginationToken = response.NextToken;
                    }
                } while (paginationToken != null); // Lặp qua các kết quả nếu có nhiều khuôn mặt
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public virtual async Task<bool> DisassociatedFaceAsync(string systermId, string faceId, string userId)
        {
            try
            {
                var response = await _rekognitionClient.DisassociateFacesAsync(new DisassociateFacesRequest()
                {
                    CollectionId = systermId,
                    FaceIds = new List<string> { faceId },
                    UserId = userId,
                });
                if (response.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new Exception(message: $"Disassociated Face with faceId {faceId} - UserId {userId} - Collection {systermId} fail");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogException($"{MethodBase.GetCurrentMethod().Name} - {GetType().Name}", ex);
                throw;
            }
        }

        public async Task<IndexFacesResponse> IndexFaceAsync(string systermId, string bucketName, string imageName, string key = null)
        {
            try
            {
                var request = new IndexFacesRequest()
                {
                    CollectionId = systermId,
                    Image = new Image()
                    {
                        S3Object = new Amazon.Rekognition.Model.S3Object()
                        {
                            Bucket = bucketName,
                            Name = imageName
                        }
                    },
                    ExternalImageId = key,
                };
                return await _rekognitionClient.IndexFacesAsync(request);
            }
            catch (InvalidS3ObjectException ex)
            {
                throw new Exception(message: "S3 object does not exist.");
            }
            catch (Amazon.Rekognition.Model.ResourceNotFoundException ex)
            {
                throw new Exception(message: "The resource specified in the request cannot be found.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> IsCollectionExistAsync(string systermId)
        {
            try
            {
                var request = new ListCollectionsRequest();
                ListCollectionsResponse response;
                do
                {
                    response = await _rekognitionClient.ListCollectionsAsync(request);
                    if (response.CollectionIds.Contains(systermId)) return true;
                    request.NextToken = response.NextToken;
                }
                while (response.NextToken != null);
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DetectFacesResponse> DetectFaceByFileAsync(IFormFile file)
        {
            try
            {
                using (var memStream = new MemoryStream())
                {
                    await file.CopyToAsync(memStream);
                    memStream.Position = 0;
                    return await GetFaceDetectAsync(new Amazon.Rekognition.Model.Image()
                    {
                        Bytes = memStream
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<DetectFacesResponse> GetFaceDetectAsync(Image image)
        {
            try
            {
                return await _rekognitionClient.DetectFacesAsync(new DetectFacesRequest()
                {
                    Image = image,
                    Attributes = new List<string>() { "DEFAULT" }
                });
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<TrainResult> AssociateFacesAsync(string systemId, List<string> faceIds, string key)
        {
            try
            {
                bool hasSuccessfulAssociations = false;

                // Iterate through each faceId
                foreach (var faceId in faceIds)
                {
                    var response = await AssociateUserAsync(key, faceId, systemId);

                    // Check if any face was successfully associated
                    if (response.AssociatedFaces != null && response.AssociatedFaces.Count > 0)
                    {
                        hasSuccessfulAssociations = true;
                    }
                    else
                    {
                        // Optionally log the failed association
                        Console.WriteLine($"Associate request failed for faceId: {faceId}");
                    }
                }

                if (hasSuccessfulAssociations)
                {
                    return TrainResult.Success;
                }
                return TrainResult.Fail;
            }
            catch (Exception ex)
            {
                return TrainResult.Error;
                //// Log the exception and rethrow it
                //throw new Exception("An error occurred while associating faces.", ex);
            }
        }

        private async Task<AssociateFacesResponse> AssociateUserAsync(string userId, string faceId, string collectionName)
        {
            var listFaceIDs = await _dynamoDBService.GetFaceIdsByUserIdAsync(userId, collectionName);
            var existingFaceId = await _dynamoDBService.GetFaceIdForUserAndFaceAsync(userId, faceId, collectionName);
            if (listFaceIDs.Count >= 100 && existingFaceId == null)
            {
                // Get the oldest face record
                var oldestFaceId = await _dynamoDBService.GetOldestFaceIdForUserAsync(userId, collectionName);

                // Disassociate and delete the oldest face
                await DisassociateAndDeleteOldestFaceAsync(userId, oldestFaceId, collectionName);
            }
            else if (faceId.CompareTo(existingFaceId) == 0)
            {
                return new AssociateFacesResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    // Optional: You can set other fields as needed
                };
            }

            var response = await _rekognitionClient.AssociateFacesAsync(new AssociateFacesRequest
            {
                CollectionId = collectionName,
                FaceIds = new List<string> {
                faceId
            },
                UserId = userId,
                UserMatchThreshold = 80F,
            });
            return response;
        }

        private async Task DisassociateAndDeleteOldestFaceAsync(string userId, string faceId, string collectionName)
        {
            // Disassociate the oldest face from Rekognition
            await DisassociatedFaceAsync(collectionName, faceId, userId);

            // Delete the oldest face record from DynamoDB
            await _dynamoDBService.DeleteItemAsync(userId, faceId, collectionName);
        }

        public async Task<bool> IsUserExistByUserIdAsync(string systermId, string userId)
        {
            try
            {
                var request = new SearchUsersRequest()
                {
                    CollectionId = systermId,
                    UserId = userId,
                };
                var response = await _rekognitionClient.SearchUsersAsync(request);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new Exception(message: $"Search User fail");
                }
                if (response.UserMatches?.Any() == true)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> CreateNewUserAsync(string systermId, string userId)
        {
            try
            {
                var request = new CreateUserRequest()
                {
                    CollectionId = systermId,
                    UserId = userId.ToLower()
                };
                var response = await _rekognitionClient.CreateUserAsync(request);
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(message: $"Create User fail");
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SearchUsersResponse> SearchUserByFaceIdsAsync(string systermId, string faceId)
        {
            try
            {
                var request = new SearchUsersRequest()
                {
                    CollectionId = systermId,
                    FaceId = faceId,
                    UserMatchThreshold = 80
                };
                return await _rekognitionClient.SearchUsersAsync(request);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Face>> GetFacesAsync(string systermId)
        {
            try
            {
                ListFacesResponse listFacesResponse = null;
                List<Face> faces = new List<Face>();
                String paginationToken = null;
                do
                {
                    if (listFacesResponse != null)
                        paginationToken = listFacesResponse.NextToken;

                    ListFacesRequest listFacesRequest = new ListFacesRequest()
                    {
                        CollectionId = systermId,
                        MaxResults = 1000,
                        NextToken = paginationToken
                    };

                    listFacesResponse = await _rekognitionClient.ListFacesAsync(listFacesRequest);
                    foreach (Face face in listFacesResponse.Faces)
                        faces.Add(face);
                } while (listFacesResponse != null && !String.IsNullOrEmpty(listFacesResponse.NextToken));
                return faces;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<string>> GetCollectionAsync(string systermId)
        {
            try
            {
                int limit = 10;
                List<string> result = new List<string>();
                ListCollectionsResponse listCollectionsResponse = null;
                String paginationToken = null;
                do
                {
                    if (listCollectionsResponse != null)
                        paginationToken = listCollectionsResponse.NextToken;

                    ListCollectionsRequest listCollectionsRequest = new ListCollectionsRequest()
                    {
                        MaxResults = limit,
                        NextToken = paginationToken
                    };

                    listCollectionsResponse = await _rekognitionClient.ListCollectionsAsync(listCollectionsRequest);

                    foreach (String resultId in listCollectionsResponse.CollectionIds)
                        result.Add(resultId);
                } while (listCollectionsResponse != null && listCollectionsResponse.NextToken != null);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> CreateCollectionByIdAsync(string collectionId)
        {
            try
            {
                var response = await _rekognitionClient.CreateCollectionAsync(new CreateCollectionRequest
                {
                    CollectionId = collectionId
                });
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(message: $"Create Collection fail");
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IndexFacesResponse> IndexFaceByFileAsync(Image file, string systermId, string key = null)
        {
            try
            {
                var request = new IndexFacesRequest()
                {
                    CollectionId = systermId,
                    Image = file,
                };
                var response = await _rekognitionClient.IndexFacesAsync(request);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new Exception(message: "Error went request to Rekognition Server");
                }
                return response;
            }
            catch (InvalidS3ObjectException ex)
            {
                throw new Exception(message: "S3 object does not exist.");
            }
            catch (Amazon.Rekognition.Model.ResourceNotFoundException ex)
            {
                throw new Exception(message: "The resource specified in the request cannot be found.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteFromCollectionAsync(string userId, string systemId)
        {
            try
            {

                // Step 1: Query DynamoDB to get face IDs associated with the userId
                var faceIds = await _dynamoDBService.GetFaceIdsByUserIdAsync(userId, systemId);

                // If there are no face IDs, delete the user record and return true
                if (faceIds == null || !faceIds.Any())
                {
                    await _dynamoDBService.DeleteUserFromDynamoDbAsync(userId, systemId);
                    await DeleteUserFromRekognitionCollectionAsync(systemId, userId);
                    return true;
                }
                foreach (var face in faceIds)
                {
                    await DisassociatedFaceAsync(systemId, face, userId);
                }
                // Step 2: Delete faces from the Rekognition collection
                //var deleteFacesRequest = new DeleteFacesRequest
                //{
                //    CollectionId = systemId,
                //    FaceIds = faceIds
                //};

                //var deleteFacesResponse = await _rekognitionClient.DeleteFacesAsync(deleteFacesRequest);

                // Step 3: Check if any faces were deleted
                //if (deleteFacesResponse.DeletedFaces.Any())
                //{
                    // Step 4: Delete the user from DynamoDB
                    await _dynamoDBService.DeleteUserFromDynamoDbAsync(userId, systemId);

                    await DeleteUserFromRekognitionCollectionAsync(systemId, userId);
                    return true;
                //}

                // If no faces were deleted, return false
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting faces for userId {userId}: {ex.Message}");
                return false;
            }
        }

        public virtual async Task DeleteUserFromRekognitionCollectionAsync(string systemId, string userId)
        {
            try
            {
                // Call AWS Rekognition to delete the user from the collection
                var deleteUserRequest = new DeleteUserRequest
                {
                    CollectionId = systemId,
                    UserId = userId

                };

                var deleteCollectionResponse = await _rekognitionClient.DeleteUserAsync(deleteUserRequest);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user from Rekognition collection: {userId}. Error: {ex.Message}");
            }
        }

        public Task<DetectFacesResponse> DetectFaceByFileAsync(Image file)
        {
            try
            {
                return GetFaceDetectAsync(file);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CollectionChartStats> GetCollectionChartStats(string systemId, string year)
        {
            try
            {
                // Step 1: Initialize the AWS Rekognition client
                var rekognitionClient = new AmazonRekognitionClient();

                // Step 2: List all faces in the collection
                var listFacesRequest = new ListFacesRequest
                {
                    CollectionId = systemId,
                    MaxResults = 1000 // Retrieve up to 1000 faces at a time
                };

                var faceIds = new HashSet<string>();
                var userIds = new HashSet<string>();
                string paginationToken = null;

                do
                {
                    // Set the pagination token for subsequent requests
                    listFacesRequest.NextToken = paginationToken;

                    // Call Rekognition to list faces
                    var listFacesResponse = await rekognitionClient.ListFacesAsync(listFacesRequest);

                    // Process the response
                    foreach (var face in listFacesResponse.Faces)
                    {
                        faceIds.Add(face.FaceId);

                        // Add UserId if it exists
                        if (!string.IsNullOrEmpty(face.ExternalImageId))
                        {
                            userIds.Add(face.ExternalImageId);
                        }
                    }

                    // Update the pagination token
                    paginationToken = listFacesResponse.NextToken;
                }
                while (!string.IsNullOrEmpty(paginationToken));

                // Step 3: Count users and face IDs
                int userCount = userIds.Count;
                int faceCount = faceIds.Count;

                // Assuming _dynamoDBService.GetDetectStats returns a result with a TotalMediaDetected property
                var mediaCount = await _dynamoDBService.GetDetectStatsByYear(systemId, year);

                // Step 4: Return the result as a strongly typed object
                return new CollectionChartStats
                {
                    UserCount = userCount,
                    FaceCount = faceCount,
                    MediaCount = mediaCount
                };
            }
            catch (Exception ex)
            {
                // Handle exceptions and log them if necessary
                throw new Exception("Error occurred while fetching collection chart stats", ex);
            }
        }

        public async Task<bool> IsUserExistByCollection(string collectionId, string userId)
        {
            try
            {
                var request = new ListFacesRequest
                {
                    CollectionId = collectionId,
                    MaxResults = 1000  // Adjust as needed
                };

                ListFacesResponse response;
                do
                {
                    response = await _rekognitionClient.ListFacesAsync(request);

                    // Check if userId exists in the collection
                    if (response.Faces.Any(face => face.ExternalImageId == userId))
                    {
                        return true;
                    }

                    // Update pagination token
                    request.NextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(response.NextToken));

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCollectionAsync(string systemName)
        {
            try
            {
                // Create the DeleteCollectionRequest
                var request = new DeleteCollectionRequest
                {
                    CollectionId = systemName
                };

                // Send the request to delete the collection
                var response = await _rekognitionClient.DeleteCollectionAsync(request);

                // Check if the delete operation was successful
                if (response.StatusCode == (int)System.Net.HttpStatusCode.OK)
                {
                    return true;
                }

                throw new Exception($"Failed to delete collection '{systemName}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting collection {systemName}: {ex.Message}");
                return false;
            }
        }
    }
}
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using FAL.Services.IServices;
using Share.SystemModel;
using System.Reflection;

namespace FAL.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly IAmazonRekognition _rekognitionClient;
        private readonly CustomLog _logger;
        public CollectionService(IAmazonRekognition rekognitionClient, CustomLog logger)
        {
            _rekognitionClient = rekognitionClient;
            _logger = logger;
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

        public async Task<bool> DisassociatedFaceAsync(string systermId, string faceId, string userId)
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
            catch (ResourceNotFoundException ex)
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
                    memStream.Position = 0; // Đặt lại vị trí con trỏ của stream về 0

                    // Gửi yêu cầu DetectLabels đến Amazon Rekognition
                    return await _rekognitionClient.DetectFacesAsync(new DetectFacesRequest()
                    {
                        Image = new Amazon.Rekognition.Model.Image()
                        {
                            Bytes = memStream
                        },
                        Attributes = new List<string>() { "DEFAULT" }
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AssociateFacesAsync(string systermId, List<string> faceIds, string key)
        {
            try
            {
                var associateRequest = new AssociateFacesRequest()
                {
                    CollectionId = systermId,
                    UserId = key,
                    FaceIds = faceIds
                };
                var response = await _rekognitionClient.AssociateFacesAsync(associateRequest);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new Exception(message: $"Accociate request fail");
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
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
                    UserId = userId
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
                        MaxResults = 1,
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
                    ExternalImageId = key,
                };
                var response = await _rekognitionClient.IndexFacesAsync(request);
                if(response.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new Exception(message: "Error went request to Rekognition Server");
                }
                return response;
            }
            catch (InvalidS3ObjectException ex)
            {
                throw new Exception(message: "S3 object does not exist.");
            }
            catch (ResourceNotFoundException ex)
            {
                throw new Exception(message: "The resource specified in the request cannot be found.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task DisassociateFacesAsync(string systermId, List<string> list, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
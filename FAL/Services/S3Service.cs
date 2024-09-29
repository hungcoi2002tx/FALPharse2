using Amazon.S3.Transfer;
using Amazon.S3;
using FAL.Services.IServices;
using Share.SystemModel;
using Share.Data;

namespace FAL.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _service;
        private readonly long _maxFileSize = GlobalVarians.MAXFILESIZE; // 15 GB, có thể tùy chỉnh
        private readonly long _partSize = 500 * 1024 * 1024; // 500 MB - Part size lớn cho multipart upload
        private readonly long _divideSize = 500 * 1024 * 1024; // 500 MB - Part size lớn cho multipart upload


        public S3Service(IAmazonS3 service)
        {
            _service = service;
        }

        public async Task<bool> AddFileToS3Async(IFormFile file, string fileName, string bucketName, TypeOfRequest type, string userId = null)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(_service);

                // Sử dụng stream để upload
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    // Kiểm tra kích thước tệp và quyết định phương thức upload
                    if (file.Length < _divideSize)
                    {
                        // Upload file nhỏ hơn TransferUtility.MinimumPartSize (5MB mặc định)
                        var uploadRequest = new TransferUtilityUploadRequest
                        {
                            InputStream = stream,
                            Key = fileName,
                            BucketName = bucketName,
                            StorageClass = S3StorageClass.Standard
                        };

                        // Thêm metadata nếu có
                        uploadRequest = GetMetaData(uploadRequest,type,userId); 
                        await fileTransferUtility.UploadAsync(uploadRequest);
                    }
                    else
                    {
                        // Sử dụng multipart upload cho tệp lớn hơn
                        var uploadRequest = new TransferUtilityUploadRequest
                        {
                            InputStream = stream,
                            Key = fileName,
                            BucketName = bucketName,
                            PartSize = _partSize, // Multipart upload với phần lớn hơn (500 MB)
                            StorageClass = S3StorageClass.Standard, // Có thể điều chỉnh storage class
                        };
                        // Thêm metadata nếu có
                        uploadRequest = GetMetaData(uploadRequest, type, userId);
                        await fileTransferUtility.UploadAsync(uploadRequest);
                    }
                }
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                throw;
            }
        }

        private TransferUtilityUploadRequest GetMetaData(TransferUtilityUploadRequest request, TypeOfRequest type, string userId)
        {
            try
            {
                request.Metadata.Add(nameof(TypeOfRequest), type.ToString());
                request.Metadata.Add(nameof(FaceInformation.UserId), userId);
                return request;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> IsExistBudgetAsync(string bucketName)
        {
            try
            {
                return await _service.DoesS3BucketExistAsync(bucketName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddBudgetAsync(string budgetName)
        {
            try
            {
                
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

using Amazon.S3.Transfer;
using Amazon.S3;
using FAL.Services.IServices;
using Share.SystemModel;
using Share.Data;
using FAL.Utils;
using System.Drawing;
using System.IO;
using SixLabors.ImageSharp;
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
                    int width = 0, height = 0;
                    if (IsImage(file))
                    {
                        (width, height) = GetImageDimensions(stream);
                    }
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
                        uploadRequest = GetMetaData(uploadRequest, type, userId, file, width, height);
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
                        uploadRequest = GetMetaData(uploadRequest, type, userId, file, width, height);
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

        private (int width, int height) GetImageDimensions(Stream stream)
        {
            stream.Position = 0;

            using (Image image = Image.Load(stream))
            {
                return (image.Width, image.Height);
            }
        }

        private TransferUtilityUploadRequest GetMetaData(TransferUtilityUploadRequest request, TypeOfRequest type, string userId, IFormFile file, int width, int height)
        {
            try
            {
                request.Metadata.Add("OriginalFileName", file.FileName);
                request.Metadata.Add(nameof(FaceInformation.UserId), userId);
                request.Metadata.Add(nameof(FaceDetectionResult.ImageWidth), width.ToString());
                request.Metadata.Add(nameof(FaceDetectionResult.ImageHeight), height.ToString());
                if (IsVideo(file))
                {
                    request.Metadata.Add(nameof(ContentType), ContentType.Video.ToString());
                }
                else if (IsImage(file))
                {
                    request.Metadata.Add(nameof(ContentType), ContentType.Image.ToString());
                }
                return request;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private bool IsImage(IFormFile file)
        {
            try
            {
                // Lấy loại file (đuôi file)
                string fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool IsVideo(IFormFile file)
        {
            try
            {
                // Lấy loại file (đuôi file)
                string fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (fileExtension == ".mp4" || fileExtension == ".mov" || fileExtension == ".avi")
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
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

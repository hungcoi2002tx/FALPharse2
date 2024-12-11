using Amazon.Rekognition.Model;
using Microsoft.AspNetCore.Http;

namespace FAL.Utils
{
    public static class FileValidationExtention
    {
        public static bool ValidImage(this IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    throw new ArgumentException(message: "File không được null.");
                }

                // Kiểm tra kích thước của file
                const long maxImageSize = 5 * 1024 * 1024; // 5MB
                
                // Lấy loại file (đuôi file)
                string fileExtension = Path.GetExtension(file.FileName).ToLower();

                // Kiểm tra nếu là ảnh (jpg, png)
                if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
                {
                    if (file.Length > maxImageSize)
                    {
                        throw new ArgumentException(message: "Kích thước file ảnh phải nhỏ hơn 5MB.");
                    }
                }
                else
                {
                    throw new ArgumentException(message: "Chỉ chấp nhận file ảnh có đuôi .jpg, .jpeg, .png.");
                }
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool ValidVideo(this IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    throw new Exception(message: "File không được null.");
                }

                // Kiểm tra kích thước của file
                const long maxVideoSize = 15 * 1024 * 1024; // 15MB

                // Lấy loại file (đuôi file)
                string fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (fileExtension == ".mp4" || fileExtension == ".mov" || fileExtension == ".avi")
                {
                    if (file.Length > maxVideoSize)
                    {
                        throw new Exception(message: "Kích thước file video phải nhỏ hơn 15MB.");
                    }
                }
                else
                {
                    throw new Exception(message: "Chỉ chấp nhận file video.");
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool ValidFile(this IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    throw new ArgumentException(message: "File không được null.");
                }

                // Định nghĩa kích thước tối đa cho ảnh và video
                const long maxImageSize = 5 * 1024 * 1024; // 5MB cho ảnh
                const long maxVideoSize = 15 * 1024 * 1024; // 15MB cho video

                // Lấy loại file (đuôi file)
                string fileExtension = Path.GetExtension(file.FileName).ToLower();

                // Kiểm tra nếu file là ảnh
                if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
                {
                    if (file.Length > maxImageSize)
                    {
                        throw new ArgumentException(message: "Kích thước file ảnh phải nhỏ hơn 5MB.");
                    }
                }
                // Kiểm tra nếu file là video
                else if (fileExtension == ".mp4" || fileExtension == ".mov" || fileExtension == ".avi")
                {
                    if (file.Length > maxVideoSize)
                    {
                        throw new ArgumentException(message: "Kích thước file video phải nhỏ hơn 15MB.");
                    }
                }
                else
                {
                    throw new ArgumentException(message: "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png) hoặc file video (.mp4, .mov, .avi).");
                }

                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool IsJpeg(this Byte[] bytes)
        {
            try
            {
                if (bytes.Length < 8)
                    return false;

                return bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static bool IsPng(this Byte[] bytes)
        {
            try
            {
                // Kiểm tra chiều dài mảng
                if (bytes.Length < 8)
                    return false;

                // Kiểm tra byte đầu tiên cho PNG
                return bytes[0] == 0x89 &&
                       bytes[1] == 0x50 &&
                       bytes[2] == 0x4E &&
                       bytes[3] == 0x47 &&
                       bytes[4] == 0x0D &&
                       bytes[5] == 0x0A &&
                       bytes[6] == 0x1A &&
                       bytes[7] == 0x0A; // Kiểm tra PNG
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static async Task<string> SaveZipToTemporaryLocation(IFormFile zipFile)
        {
            var tempZipFilePath = Path.GetTempFileName();
            using (var stream = new FileStream(tempZipFilePath, FileMode.Create))
            {
                await zipFile.CopyToAsync(stream);
            }
            return tempZipFilePath;
        }

        public static bool IsValidZipFile(IFormFile zipFile, out string errorMessage)
        {
            if (zipFile == null || zipFile.Length == 0)
            {
                errorMessage = "No ZIP file uploaded.";
                return false;
            }

            if (!Path.GetExtension(zipFile.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Only ZIP files are supported.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public static List<string> GetImageFilesFromDirectory(string directoryPath)
        {
            return Directory.GetFiles(directoryPath)
                .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static void CleanupTemporaryFiles(string tempZipFilePath, string extractPath)
        {
            if (System.IO.File.Exists(tempZipFilePath))
            {
                System.IO.File.Delete(tempZipFilePath);
            }

            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }
        }

        /// <summary>
        /// Chuyển đổi IFormFile thành mảng byte.
        /// </summary>
        /// <param name="file">Tệp tin cần chuyển đổi.</param>
        /// <returns>Một mảng byte từ nội dung của tệp tin.</returns>
        public static async Task<byte[]> ToByteArrayAsync(this IFormFile file)
        {
            if (file == null)
            {
                return null;
            }
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            return stream.ToArray();
        }
    }
}

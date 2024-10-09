using Amazon.Rekognition.Model;

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
    }
}

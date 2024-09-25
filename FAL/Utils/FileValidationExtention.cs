namespace FAL.Utils
{
    public static class FileValidationExtention
    {
        public static bool ValidFile(this IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    throw new Exception(message: "File không được null.");
                }

                // Kiểm tra kích thước của file
                const long maxImageSize = 15 * 1024 * 1024; // 15MB
                const long maxVideoSize = 15L * 1024 * 1024 * 1024; // 15GB

                // Lấy loại file (đuôi file)
                string fileExtension = Path.GetExtension(file.FileName).ToLower();

                // Kiểm tra nếu là ảnh (jpg, png)
                if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
                {
                    if (file.Length > maxImageSize)
                    {
                        throw new Exception(message: "Kích thước file ảnh phải nhỏ hơn 16MB.");
                    }
                }
                // Kiểm tra nếu là video
                else if (fileExtension == ".mp4" || fileExtension == ".mov" || fileExtension == ".avi")
                {
                    if (file.Length > maxVideoSize)
                    {
                        throw new Exception(message: "Kích thước file video phải nhỏ hơn 16GB.");
                    }
                }
                else
                {
                    throw new Exception(message: "Chỉ chấp nhận file ảnh có đuôi .jpg, .jpeg, .png hoặc video.");
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

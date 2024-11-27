using Demo_Eos_Api.DTOs;
using Demo_Eos_Api.Service.Implement;
using Demo_Eos_Api.Utilities;
using Microsoft.Extensions.Options;
using System.Drawing.Imaging;
using System.Net;

namespace Demo_Eos_Api.Service.Interface
{
    public class FileConfig
    {
        public int MaxImageSize { get; set; }
        public int MaxCVFileSize { get; set; }
        public int MaxImageSubmit { get; set; }
        public int Quality { get; set; }
        public int Scale { get; set; }
    }

    public class ImageService : IImageService
    {

        private readonly FileConfig _fileConfig;

        public ImageService(IOptions<FileConfig> fileConfig)
        {
            _fileConfig = fileConfig.Value;
        }

        public async Task<ResponseResult> SaveImageToFile(SaveImageDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.ImageBase))
            {
                return new ResponseResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    DevMsg = "Request data is invalid.",
                    UserMsg = "Invalid input provided. Please check your data and try again."
                };
            }

            try
            {
                // Decode the Base64 string to byte array
                byte[] imageBytes = Convert.FromBase64String(request.ImageBase);
                string projectDirectory = Directory.GetCurrentDirectory();

                // Navigate up to the solution directory (assuming the project is one level below the solution folder)
                string solutionDirectory = Directory.GetParent(projectDirectory).FullName;
                var semester = FindCurrentSemester();

                // Construct the path to the EOS folder outside the project directory
                string baseFolderPath = Path.Combine(solutionDirectory, "EOS", "ImageStudentCapture", semester, request.ExamCode);

                // Ensure the directory exists
                if (!Directory.Exists(baseFolderPath))
                {
                    Directory.CreateDirectory(baseFolderPath);
                }

                // Determine the image file extension (default to .jpg if detection fails)
                string fileExtension = ".jpg"; // Default
                string contentType = GetImageContentType(imageBytes);
                if (!string.IsNullOrEmpty(contentType))
                {
                    fileExtension = GetFileExtensionFromContentType(contentType);
                }

                // Construct the full file path using StudentCode
                string fileName = $"{request.StudentCode}{fileExtension}";
                string fullFilePath = Path.Combine(baseFolderPath, fileName);

                // Save the image with compression
                using (var memoryStream = new MemoryStream(imageBytes))
                {
                    IFormFile file = new FormFile(memoryStream, 0, memoryStream.Length, null, fileName)
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = contentType ?? "image/jpeg"
                    };

                    Utility.CompressAndSaveImage(file, fullFilePath, _fileConfig.Quality, _fileConfig.Scale);
                }

                // Return success response
                return new ResponseResult
                {
                    Success = true,
                    StatusCode = HttpStatusCode.OK,
                    DevMsg = "Image successfully saved.",
                    UserMsg = "Your image has been successfully saved.",
                    Data = new { FileName = fileName, FilePath = fullFilePath }
                };
            }
            catch (Exception ex)
            {
                // Handle errors
                return new ResponseResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    DevMsg = $"An error occurred: {ex.Message}",
                    UserMsg = "An error occurred while processing your request. Please try again later."
                };
            }
        }

        private string FindCurrentSemester()
        {
            // Define Vietnam time zone
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            // Get the current date and time in Vietnam
            DateTime vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            // Extract the current month and year
            int month = vietnamTime.Month;
            int year = vietnamTime.Year;

            // Determine the semester based on the month
            string semester = month switch
            {
                >= 1 and <= 4 => "Spring",
                >= 5 and <= 8 => "Summer",
                >= 9 and <= 12 => "Fall",
                _ => throw new InvalidOperationException("Invalid month encountered.")
            };

            // Return the formatted semester string
            return $"{semester}{year}";
        }


        // Helper method to determine content type of image
        private string GetImageContentType(byte[] imageBytes)
        {
            try
            {
                using (var stream = new MemoryStream(imageBytes))
                {
                    var image = System.Drawing.Image.FromStream(stream);
                    if (ImageFormat.Jpeg.Equals(image.RawFormat))
                        return "image/jpeg";
                    if (ImageFormat.Png.Equals(image.RawFormat))
                        return "image/png";
                    if (ImageFormat.Gif.Equals(image.RawFormat))
                        return "image/gif";
                }
            }
            catch
            {
                // Ignored - fallback to default
            }

            return null;
        }

        // Helper method to map content type to file extension
        private string GetFileExtensionFromContentType(string contentType)
        {
            return contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                _ => ".jpg"
            };
        }

    }
}

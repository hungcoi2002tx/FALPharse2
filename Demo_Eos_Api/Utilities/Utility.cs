using System.Drawing.Imaging;
using System.Drawing;


namespace AuthenExamReceiveData.Utilities
{
    public static class Utility
    {

        /// <summary>
        /// nén ảnh
        /// </summary>
        /// <param name="file"></param>
        /// <param name="outputPath"></param>
        /// <param name="quality">chất lượng ảnh</param>
        /// <param name="scale">tỉ lệ ảnh</param>
        /// <exception cref="ArgumentException"></exception>
        public static void CompressAndSaveImage(IFormFile file, string outputPath, long quality, int scale = 100)
        {
            int newWidth;
            int newHeight;
            float percentSize = scale / 100.0f;

            // Kiểm tra file có dữ liệu không
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File không hợp lệ");
            }

            using (var stream = new MemoryStream())
            {

                file.CopyTo(stream);
                stream.Position = 0;


                using (var image = Image.FromStream(stream))
                {
                    newWidth = (int)(image.Width * percentSize);
                    newHeight = (int)(image.Height * percentSize);


                    using (var newImage = new Bitmap(newWidth, newHeight))
                    using (var graphics = Graphics.FromImage(newImage))
                    {
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;


                        graphics.DrawImage(image, 0, 0, newWidth, newHeight);


                        var encoder = ImageCodecInfo.GetImageDecoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                        var encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                        // Lưu ảnh đã thay đổi kích thước và chất lượng
                        newImage.Save(outputPath, encoder, encoderParams);
                    }
                }
            }
        }
    }
}

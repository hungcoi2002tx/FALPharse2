using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Windows.Forms;

namespace AuthenExamCameraLibrary
{
    public class CameraHandler
    {
        private FilterInfoCollection filterInfoCollection;
        private PictureBox targetPictureBox;

        private int rectangleWidth;  // Chiều rộng khung hình chữ nhật
        private int rectangleHeight; // Chiều cao khung hình chữ nhật

        /// <summary>
        /// Khởi tạo thư viện xử lý camera.
        /// </summary>
        public CameraHandler()
        {
            filterInfoCollection = GetListCamera();

            if (filterInfoCollection.Count == 0)
            {
                throw new Exception("Không tìm thấy thiết bị camera nào!");
            }
        }

        /// <summary>
        /// Bắt đầu hiển thị luồng camera trên PictureBox.
        /// </summary>
        /// <param name="videoCaptureDevice">VideoCaptureDevice camera dùng để chụp ảnh.</param>
        /// <param name="pictureBox">PictureBox dùng để hiển thị camera.</param>
        /// <param name="cameraIndex">Chỉ số của camera (mặc định là 0).</param>
        /// <param name="rectWidth">Chiều rộng khung hình chữ nhật.</param>
        /// <param name="rectHeight">Chiều cao khung hình chữ nhật.</param>
        public void StartCamera(VideoCaptureDevice videoCaptureDevice, PictureBox pictureBox, int rectWidth = 350, int rectHeight = 500, int camWidth = 1097, int camHeight = 624)
        {
            if (videoCaptureDevice == null)
            {
                throw new Exception("Không có camera tương ứng!");
            }

            if (pictureBox == null)
            {
                throw new ArgumentNullException(nameof(pictureBox));
            }
            pictureBox.Width = camWidth;
            pictureBox.Height = camHeight;

            targetPictureBox = pictureBox ?? throw new ArgumentNullException(nameof(pictureBox));

            // Lưu chiều rộng và chiều cao hình chữ nhật
            rectangleWidth = rectWidth;
            rectangleHeight = rectHeight;

            videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            videoCaptureDevice.Start();
        }

        public void StartCamera(PictureBox pictureBox, int rectWidth = 350, int rectHeight = 500, int cameraIndex = 0, int camWidth = 1097, int camHeight = 624)
        {
            if (cameraIndex < 0 || cameraIndex >= filterInfoCollection.Count)
            {
                throw new Exception("Không có camera tương ứng!");
            }

            if (pictureBox == null)
            {
                throw new ArgumentNullException(nameof(pictureBox));
            }

            pictureBox.Width = camWidth;
            pictureBox.Height = camHeight;

            targetPictureBox = pictureBox ?? throw new ArgumentNullException(nameof(pictureBox));

            // Lưu chiều rộng và chiều cao hình chữ nhật
            rectangleWidth = rectWidth;
            rectangleHeight = rectHeight;

            VideoCaptureDevice videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[cameraIndex].MonikerString);
            videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;

            videoCaptureDevice.Start();
        }

        /// <summary>
        /// Dừng luồng camera.
        /// </summary>
        public void StopCamera(VideoCaptureDevice videoCaptureDevice)
        {
            if (videoCaptureDevice != null && videoCaptureDevice.IsRunning)
            {
                videoCaptureDevice.SignalToStop();
                videoCaptureDevice.WaitForStop();
                videoCaptureDevice = null;
            }
        }

        /// <summary>
        /// Kiểm tra xem camera có đang được sử dụng bởi ứng dụng khác không.
        /// </summary>
        /// <param name="videoCaptureDevice">Thiết bị camera cần kiểm tra.</param>
        /// <returns>True nếu camera đang được sử dụng; ngược lại False.</returns>
        public bool IsCameraInUse(VideoCaptureDevice videoCaptureDevice)
        {
            if (videoCaptureDevice == null)
                throw new ArgumentNullException(nameof(videoCaptureDevice));

            bool frameReceived = false;

            // Sự kiện nhận khung hình
            void OnNewFrame(object sender, NewFrameEventArgs eventArgs)
            {
                frameReceived = true;
                videoCaptureDevice.NewFrame -= OnNewFrame; // Dừng lắng nghe ngay khi nhận khung hình
            }

            // Gắn sự kiện NewFrame
            videoCaptureDevice.NewFrame += OnNewFrame;

            try
            {
                // Bắt đầu camera để kiểm tra
                videoCaptureDevice.Start();

                // Chờ trong 3 giây để nhận khung hình
                int elapsedTime = 0;
                const int waitInterval = 100; // Kiểm tra mỗi 100ms
                while (!frameReceived && elapsedTime < 3000)
                {
                    System.Threading.Thread.Sleep(waitInterval);
                    elapsedTime += waitInterval;
                }
            }
            finally
            {
                // Ngừng camera bất kể kết quả
                videoCaptureDevice.SignalToStop();
                videoCaptureDevice.WaitForStop();
                videoCaptureDevice.NewFrame -= OnNewFrame;
            }

            return frameReceived;
        }

        /// <summary>
        /// Lấy list camera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public FilterInfoCollection GetListCamera()
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            return filterInfoCollection;
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();

            Bitmap resizedFrame = new Bitmap(targetPictureBox.Width, targetPictureBox.Height);
            using (Graphics g = Graphics.FromImage(resizedFrame))
            {
                g.DrawImage(frame, 0, 0, targetPictureBox.Width, targetPictureBox.Height);

                // Vẽ hình chữ nhật giữa màn hình với kích thước do người dùng cung cấp
                Pen orangePen = new Pen(Color.Orange, 2);
                int rectX = (resizedFrame.Width - rectangleWidth) / 2;
                int rectY = (resizedFrame.Height - rectangleHeight) / 2;
                g.DrawRectangle(orangePen, rectX, rectY, rectangleWidth, rectangleHeight);
            }

            if (targetPictureBox.IsHandleCreated)
            {
                targetPictureBox.BeginInvoke(new Action(() =>
                {
                    targetPictureBox.Image?.Dispose();
                    targetPictureBox.Image = resizedFrame;
                }));
            }

            frame.Dispose();
        }

        /// <summary>
        /// Chụp và cắt ảnh từ PictureBox theo khung vàng.
        /// </summary>
        /// <param name="pictureBox">PictureBox chứa hình ảnh cần chụp.</param>
        /// <param name="cropWidth">Chiều rộng của khung cắt.</param>
        /// <param name="cropHeight">Chiều cao của khung cắt.</param>
        /// <param name="outputFolderPath">Đường dẫn thư mục để lưu ảnh.</param>
        /// <returns>Binary string ảnh đã cắt</returns>
        public string CaptureAndCropImageToBinaryString(PictureBox pictureBox, string outputFolderPath, bool isDeleteImage = true)
        {
            int cropWidth = rectangleWidth;
            int cropHeight = rectangleHeight;
            string croppedFilePath = null;
            string originalFilePath = null;

            if (pictureBox.Image == null)
            {
                throw new InvalidOperationException("PictureBox không chứa hình ảnh.");
            }

            EnsureFolderExists(outputFolderPath);

            // Lấy hình ảnh từ PictureBox
            Bitmap originalImage = new Bitmap(pictureBox.Image);

            try
            {
                // Lưu ảnh gốc
                originalFilePath = SaveOriginalImage(originalImage, outputFolderPath);

                // Cắt ảnh
                Bitmap croppedImage = CropImage(originalImage, cropWidth, cropHeight);

                // Lưu ảnh đã cắt
                croppedFilePath = SaveCroppedImage(croppedImage, outputFolderPath);

                // Trả về ảnh đã cắt dạng binary string
                return ConvertImageToBinaryString(croppedFilePath);
            }
            finally
            {
                originalImage.Dispose();
                if (isDeleteImage)
                {
                    DeleteImage(croppedFilePath);
                    DeleteImage(originalFilePath);
                }
            }
        }


        ///// <summary>
        ///// Chụp và cắt ảnh từ PictureBox theo khung vàng.
        ///// </summary>
        ///// <param name="pictureBox">PictureBox chứa hình ảnh cần chụp.</param>
        ///// <param name="cropWidth">Chiều rộng của khung cắt.</param>
        ///// <param name="cropHeight">Chiều cao của khung cắt.</param>
        ///// <param name="outputFolderPath">Đường dẫn thư mục để lưu ảnh.</param>
        ///// <returns>Đường dẫn của ảnh đã cắt.</returns>
        //public string CaptureAndCropImage(PictureBox pictureBox, string outputFolderPath)
        //{
        //    int cropWidth = rectangleWidth;
        //    int cropHeight = rectangleHeight;

        //    if (pictureBox.Image == null)
        //    {
        //        throw new InvalidOperationException("PictureBox không chứa hình ảnh.");
        //    }

        //    EnsureFolderExists(outputFolderPath);

        //    // Lấy hình ảnh từ PictureBox
        //    Bitmap originalImage = new Bitmap(pictureBox.Image);

        //    try
        //    {
        //        // Lưu ảnh gốc
        //        string originalFilePath = SaveOriginalImage(originalImage, outputFolderPath);

        //        // Cắt ảnh
        //        Bitmap croppedImage = CropImage(originalImage, cropWidth, cropHeight);

        //        // Lưu ảnh đã cắt
        //        string croppedFilePath = SaveCroppedImage(croppedImage, outputFolderPath);

        //        return croppedFilePath;
        //    }
        //    finally
        //    {
        //        originalImage.Dispose();
        //    }
        //}

        /// <summary>
        /// Xóa một ảnh trong thư mục.
        /// </summary>
        /// <param name="filePath">Đường dẫn đầy đủ của ảnh cần xóa.</param>
        private void DeleteImage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Đường dẫn ảnh không hợp lệ!", nameof(filePath));
            }

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    Console.WriteLine($"Đã xóa ảnh: {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi xóa ảnh: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Ảnh không tồn tại trong thư mục.");
            }
        }

        private void EnsureFolderExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        /// <summary>
        /// Chuyển đổi một ảnh thành chuỗi nhị phân.
        /// </summary>
        /// <param name="imagePath">Đường dẫn đến file ảnh.</param>
        /// <returns>Chuỗi nhị phân đại diện cho ảnh, hoặc thông báo lỗi nếu không thành công.</returns>
        private string ConvertImageToBinaryString(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                throw new ArgumentException("Đường dẫn ảnh không được để trống.", nameof(imagePath));

            if (!File.Exists(imagePath))
                throw new FileNotFoundException("Không tìm thấy file ảnh.", imagePath);

            try
            {
                // Đọc toàn bộ file ảnh thành mảng byte
                byte[] imageBytes = File.ReadAllBytes(imagePath);

                // Sử dụng StringBuilder để tối ưu hóa việc nối chuỗi
                StringBuilder binaryStringBuilder = new StringBuilder();

                foreach (byte b in imageBytes)
                {
                    // Chuyển mỗi byte thành chuỗi nhị phân (8-bit)
                    binaryStringBuilder.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
                }

                return binaryStringBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Đã xảy ra lỗi trong quá trình chuyển đổi ảnh.", ex);
            }
        }

        private string SaveOriginalImage(Image image, string outputFolderPath)
        {
            string fileName = $"original_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            string filePath = Path.Combine(outputFolderPath, fileName);
            image.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            return filePath;
        }

        private Bitmap CropImage(Bitmap originalImage, int width, int height)
        {
            int rectX = (originalImage.Width - width) / 2;
            int rectY = (originalImage.Height - height) / 2;
            Rectangle cropRect = new Rectangle(rectX, rectY, width, height);
            return originalImage.Clone(cropRect, originalImage.PixelFormat);
        }

        private string SaveCroppedImage(Bitmap croppedImage, string outputFolderPath)
        {
            string fileName = $"cropped_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            string filePath = Path.Combine(outputFolderPath, fileName);
            croppedImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            return filePath;
        }
    }
}

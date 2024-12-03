using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Runtime.Remoting.Lifetime;
using System.Windows.Forms;

namespace CameraLibrary
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
        public void StartCamera(VideoCaptureDevice videoCaptureDevice, PictureBox pictureBox, int rectWidth = 350, int rectHeight = 500)
        {
            if (videoCaptureDevice == null)
            {
                throw new Exception("Không có camera tương ứng!");
            }

            targetPictureBox = pictureBox ?? throw new ArgumentNullException(nameof(pictureBox));

            // Lưu chiều rộng và chiều cao hình chữ nhật
            rectangleWidth = rectWidth;
            rectangleHeight = rectHeight;

            videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            videoCaptureDevice.Start();
        }

        public void StartCamera(PictureBox pictureBox, int rectWidth = 350, int rectHeight = 500, int cameraIndex = 0)
        {
            if (cameraIndex < 0 || cameraIndex >= filterInfoCollection.Count)
            {
                throw new Exception("Không có camera tương ứng!");
            }

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
    }
}

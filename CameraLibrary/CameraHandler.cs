using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CameraLibrary
{
    public class CameraHandler
    {
        private VideoCaptureDevice videoCaptureDevice;
        private FilterInfoCollection filterInfoCollection;
        private PictureBox targetPictureBox;

        private int rectangleWidth;  // Chiều rộng khung hình chữ nhật
        private int rectangleHeight; // Chiều cao khung hình chữ nhật

        /// <summary>
        /// Khởi tạo thư viện xử lý camera.
        /// </summary>
        public CameraHandler()
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (filterInfoCollection.Count == 0)
            {
                throw new Exception("Không tìm thấy thiết bị camera nào!");
            }
        }

        /// <summary>
        /// Bắt đầu hiển thị luồng camera trên PictureBox.
        /// </summary>
        /// <param name="pictureBox">PictureBox dùng để hiển thị camera.</param>
        /// <param name="cameraIndex">Chỉ số của camera (mặc định là 0).</param>
        /// <param name="rectWidth">Chiều rộng khung hình chữ nhật.</param>
        /// <param name="rectHeight">Chiều cao khung hình chữ nhật.</param>
        public void StartCamera(PictureBox pictureBox, int cameraIndex = 0, int rectWidth = 350, int rectHeight = 500)
        {
            if (cameraIndex >= filterInfoCollection.Count)
            {
                throw new ArgumentOutOfRangeException("CameraIndex", "Chỉ số camera không hợp lệ.");
            }

            targetPictureBox = pictureBox ?? throw new ArgumentNullException(nameof(pictureBox));

            // Lưu chiều rộng và chiều cao hình chữ nhật
            rectangleWidth = rectWidth;
            rectangleHeight = rectHeight;

            videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[cameraIndex].MonikerString);
            videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            videoCaptureDevice.Start();
        }

        /// <summary>
        /// Dừng luồng camera.
        /// </summary>
        public void StopCamera()
        {
            if (videoCaptureDevice != null && videoCaptureDevice.IsRunning)
            {
                videoCaptureDevice.SignalToStop();
                videoCaptureDevice.WaitForStop();
                videoCaptureDevice = null;
            }
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

            targetPictureBox.Invoke(new Action(() =>
            {
                targetPictureBox.Image?.Dispose();
                targetPictureBox.Image = resizedFrame;
            }));

            frame.Dispose();
        }
    }

}

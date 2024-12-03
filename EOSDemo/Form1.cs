using AForge.Video.DirectShow;
using CameraLibrary;

namespace EOSDemo
{
    public partial class Form1 : Form
    {
        private CameraHandler cameraHandler;
        private int cameraIndex = 0;
        private VideoCaptureDevice videoCaptureDevice;

        public Form1()
        {
            InitializeComponent();

            try
            {
                cameraHandler = new CameraHandler();
                var videoDevices = cameraHandler.GetListCamera();

                // Kiểm tra nếu không có thiết bị camera nào
                if (videoDevices.Count == 0)
                {
                    MessageBox.Show(
                        "Không tìm thấy thiết bị camera. Vui lòng kiểm tra kết nối.",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // Tạo video capture device
                videoCaptureDevice = new VideoCaptureDevice(videoDevices[cameraIndex].MonikerString);


                // Kiểm tra xem camera có đang được sử dụng bởi ứng dụng khác không
                if (cameraHandler.IsCameraInUse(videoCaptureDevice))
                {
                    MessageBox.Show(
                        "Camera đang được sử dụng bởi ứng dụng khác. Vui lòng đóng ứng dụng đó và thử lại.",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    // Dừng camera nếu đang chạy
                    cameraHandler.StopCamera(videoCaptureDevice);
                    this.Close();
                    return;
                }


                // Bắt đầu luồng camera
                cameraHandler.StartCamera(videoCaptureDevice, pictureBoxCamera, 400, 300);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi khởi tạo camera: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Dừng camera trước khi đóng form
            if (videoCaptureDevice != null)
            {
                cameraHandler.StopCamera(videoCaptureDevice);
            }
            Application.Exit();
        }
    }
}

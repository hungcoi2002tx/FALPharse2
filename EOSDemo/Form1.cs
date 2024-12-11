using AForge.Video.DirectShow;
using AuthenExamCameraLibrary;
using System.Windows.Forms;

namespace EOSDemo
{
    public partial class Form1 : Form
    {
        private AuthenExamCameraLibrary.CameraHandler cameraHandler;
        private int cameraIndex = 0;
        private VideoCaptureDevice videoCaptureDevice;

        public Form1()
        {
            InitializeComponent();

            try
            {
                cameraHandler = new AuthenExamCameraLibrary.CameraHandler();
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
                if (!cameraHandler.IsCameraInUse(videoCaptureDevice))
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

            // Bắt đầu luồng camera
            cameraHandler.StartCamera(videoCaptureDevice, pictureBoxCamera);
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

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(cameraHandler.CaptureAndCropImageToBinaryString(pictureBoxCamera, @"C:\MyImages",false)[..50], "Binary String", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

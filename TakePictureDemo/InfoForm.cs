using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using CameraCaptureLibrary;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TakePictureDemo
{
    public partial class InfoForm : Form
    {
        private CameraCapture cameraCapture;
        private ApiClient apiClient;

        public InfoForm()
        {
            InitializeComponent();
            cameraCapture = new CameraCapture();
            apiClient = new ApiClient("https://localhost:7031/api/CompareFace/register-compare");
            cameraCapture.OnImageCaptured += OnImageCaptured;
        }

        private void OnImageCaptured(string imagePath)
        {
            if (InvokeRequired)
            {
                // Gọi Invoke để chuyển sang UI thread
                Invoke(new Action(() => OnImageCaptured(imagePath)));
            }
            else
            {
                // Cập nhật giao diện trên UI thread
                MessageBox.Show($"Đã chụp ảnh và lưu vào {imagePath}");
                //textBox1.Text = imagePath;
            }
        }


        private void sendBtn_Click(object sender, EventArgs e)
        {
            string studentNo = textBox2.Text;
            //string response = apiClient.RegisterCompare(textBox1.Text, studentNo);
            //MessageBox.Show(response);
        }

        private void captureBtn_Click(object sender, EventArgs e)
        {
            //cameraCapture.StartCamera();

            using (CameraForm formCamera = new CameraForm())
            {
                this.Hide(); // Ẩn form hiện tại
                if (formCamera.ShowDialog() == DialogResult.OK)
                {
                    // Xử lý khi formCamera trả về kết quả OK
                    // string savedImagePath = formCamera.SavedImagePath;
                    // MessageBox.Show("Đường dẫn ảnh đã lưu: " + savedImagePath);
                }
                this.Close(); // Đóng form hiện tại
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        //private void OnFormClosing(object sender, FormClosingEventArgs e)
        //{
        //    cameraCapture.StopCamera();
        //    base.OnFormClosing(e);
        //}
    }
}

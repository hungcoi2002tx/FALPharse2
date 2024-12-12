using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AuthenExamTakePicture
{
    public partial class InfoForm : Form
    {
        public InfoForm()
        {
            InitializeComponent();
        }

        private void OnImageCaptured(string imagePath)
        {
            //if (InvokeRequired)
            //{
            //    // Gọi Invoke để chuyển sang UI thread
            //    Invoke(new Action(() => OnImageCaptured(imagePath)));
            //}
            //else
            //{
            //    // Cập nhật giao diện trên UI thread
            //    MessageBox.Show($"Đã chụp ảnh và lưu vào {imagePath}");
            //    //textBox1.Text = imagePath;
            //}
        }


        private void sendBtn_Click(object sender, EventArgs e)
        {
            //string studentNo = textBox2.Text;
            //string response = apiClient.RegisterCompare(textBox1.Text, studentNo);
            //MessageBox.Show(response);
        }

        private void captureBtn_Click(object sender, EventArgs e)
        {
            using (CameraForm formCamera = new CameraForm())
            {
                formCamera.ExamCode = textBox3.Text;
                formCamera.StudentCode = textBox2.Text;

                this.Hide();
                if (formCamera.ShowDialog() == DialogResult.OK)
                {

                }
                this.Close();
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

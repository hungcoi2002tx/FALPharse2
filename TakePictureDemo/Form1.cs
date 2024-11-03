using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TakePictureDemo
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private string captureImagePath;
        private string targetImagePath;
        private string studentNo;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap image = (Bitmap)eventArgs.Frame.Clone();

            // Lưu ảnh vào file
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "capture.jpg");
            image.Save(path);
            // Dừng việc capture sau khi đã chụp ảnh
            videoSource.SignalToStop();
            captureImagePath = path;

            this.Invoke(new Action(() =>
            {
                MessageBox.Show($"Đã chụp ảnh và lưu vào {path}");
                textBox1.Text = captureImagePath;
            }));
        }

        private async Task CallRegisterCompareApiAsync()
        {
            var targetImagePath = textBox1.Text;
            var url = "https://localhost:7031/api/CompareFace/register-compare";

            using (var client = new HttpClient())
            {
                using (var form = new MultipartFormDataContent())
                {
                    // Thêm mã sinh viên vào URL
                    var fullUrl = $"{url}?studentCode={studentNo}";

                    // Đọc file ảnh và gửi lên như là một phần của request
                    var targetImageContent = new StreamContent(File.OpenRead(targetImagePath));
                    targetImageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    form.Add(targetImageContent, "targetImage", Path.GetFileName(targetImagePath));

                    // Gửi request đến API
                    var response = await client.PostAsync(fullUrl, form);

                    // Kiểm tra phản hồi từ API
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Image comparison registered successfully.");
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error: {error}");
                    }
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Dừng video stream nếu form đóng
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
            base.OnFormClosing(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Lấy danh sách thiết bị video (webcam)
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count > 0)
            {
                // Chọn webcam đầu tiên
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);

                // Bắt đầu stream video
                videoSource.Start();
            }
            else
            {
                MessageBox.Show("Không tìm thấy webcam.");
            }
        }

        private async void sendBtn_Click(object sender, EventArgs e)
        {
            studentNo = textBox2.Text;
            await CallRegisterCompareApiAsync();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}

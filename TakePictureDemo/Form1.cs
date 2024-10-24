using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace TakePictureDemo
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
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

        private async void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Chụp ảnh từ frame video
            Bitmap image = (Bitmap)eventArgs.Frame.Clone();

            // Lưu ảnh vào file
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "capture.jpg");
            image.Save(path);
            // Dừng việc capture sau khi đã chụp ảnh
            videoSource.SignalToStop();

            MessageBox.Show($"Đã chụp ảnh và lưu vào {path}");

            // Gửi ảnh đến API
            await SendImageToApi(path);
        }

        private async Task SendImageToApi(string filePath)
        {
            try
            {
                var handler = new HttpClientHandler
                  {
                      ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                  };
                using (HttpClient client = new HttpClient(handler))
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "http://fal-dev.eba-55qpmvbp.ap-southeast-1.elasticbeanstalk.com/api/Detect");
                    request.Headers.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJraGFubSIsInJvbGVJZCI6IjEiLCJzeXN0ZW1OYW1lIjoiZnVhbHVtbmkiLCJqdGkiOiIwYjM0MTQ5NS04ZmY1LTRjODQtYTk0Zi03Y2YyM2UzMWQ1NzMiLCJleHAiOjE3Mjk3MDIxMzQsImlzcyI6InlvdXJfaXNzdWVyIiwiYXVkIjoieW91cl9hdWRpZW5jZSJ9.rv9AKboWOGZbM1E4bbGQ7u6uzLAXe9Aaeq6QnTpx9To");

                    // Chuẩn bị nội dung cho request
                    var content = new MultipartFormDataContent();
                    content.Add(new StreamContent(File.OpenRead(filePath)), "File", "capture.jpg");
                    request.Content = content;

                    // Gửi request và xử lý phản hồi
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Kết quả từ server: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi ảnh: {ex.Message}");
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
    }
}

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
            captureImagePath = path;
            MessageBox.Show($"Đã chụp ảnh và lưu vào {path}");

            // Gửi ảnh đến API

        }

        private async Task SendImageToApi()
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                var apiUrl = "https://localhost:7065/api/Compare/compare";
                var token = txtBoxToken.Text;
                var sourceImagePath = captureImagePath;
                var targetImagePath = textBox1.Text;


                using (var httpClient = new HttpClient(handler))
                {
                    // Nếu token không trống, thêm Authorization header
                    if (!string.IsNullOrEmpty(token))
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    using (var form = new MultipartFormDataContent())
                    {
                        // Đọc nội dung ảnh
                        var sourceImageContent = new ByteArrayContent(File.ReadAllBytes(sourceImagePath));
                        var targetImageContent = new ByteArrayContent(File.ReadAllBytes(targetImagePath));

                        // Thêm nội dung vào form data với tên của các phần tệp
                        form.Add(sourceImageContent, "SourceImage", "source_image.jpg");
                        form.Add(targetImageContent, "TargetImage", "target_image.jpg");

                        try
                        {
                            // Gửi yêu cầu POST tới API
                            var response = await httpClient.PostAsync(apiUrl, form);

                            // Xử lý phản hồi
                            if (response.IsSuccessStatusCode)
                            {
                                var responseContent = await response.Content.ReadAsStringAsync();
                                Console.WriteLine("Response: " + responseContent);

                                txtResult.Text = responseContent;
                            }
                            else
                            {
                                txtResult.Text = response.StatusCode.ToString();
                                Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception: {ex.Message}");
                        }
                    }
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

        private void button1_Click(object sender, EventArgs e)
        {
            // Tạo OpenFileDialog để người dùng chọn tệp
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Thiết lập bộ lọc cho các loại tệp, ví dụ chỉ cho phép chọn ảnh
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png|All files (*.*)|*.*";

                // Hiển thị hộp thoại chọn tệp
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Lưu đường dẫn của tệp đã chọn
                    string filePath = openFileDialog.FileName;

                    // Hiển thị đường dẫn trong một TextBox hoặc Label (hoặc lưu ở nơi khác)
                    textBox1.Text = filePath;  // Hiển thị đường dẫn trong TextBox
                }
            }
        }

        private async void sendBtn_Click(object sender, EventArgs e)
        {
            await SendImageToApi();
        }
    }
}

using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TakePictureDemo
{
    public partial class CameraForm : Form
    {
        private FilterInfoCollection filterInfoCollection;
        private VideoCaptureDevice videoCaptureDevice;
        private Button captureButton;
        private Button sendButton;

        public string ImagePath { get; private set; }

        public CameraForm()
        {
            InitializeComponent();
            pictureBox.Width = 1280;
            pictureBox.Height = 720;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            this.Resize += CameraForm_Resize;
            this.FormClosing += CameraForm_FormClosing;
            CreateRoundButton();
            CreateSendButton();
        }

        private void CreateRoundButton()
        {
            captureButton = new Button
            {
                Width = 60, // Đường kính nút
                Height = 60,
                BackColor = Color.Orange,
                FlatStyle = FlatStyle.Flat,
            };

            // Loại bỏ viền nút
            captureButton.FlatAppearance.BorderSize = 0;
            captureButton.FlatAppearance.MouseOverBackColor = Color.DarkOrange; // Màu khi hover
            captureButton.FlatAppearance.MouseDownBackColor = Color.OrangeRed; // Màu khi nhấn

            // Tạo hình tròn cho nút
            var graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
            graphicsPath.AddEllipse(0, 0, captureButton.Width, captureButton.Height); // Hình tròn
            captureButton.Region = new Region(graphicsPath);

            // Gắn sự kiện click
            captureButton.Click += (s, e) =>
            {
                CaptureAndSaveAndCropImage();
            };

            // Thêm nút vào form
            this.Controls.Add(captureButton);

            // Đặt vị trí căn giữa form
            captureButton.Left = (this.ClientSize.Width - captureButton.Width) / 2;
            captureButton.Top = this.ClientSize.Height - captureButton.Height - 50;
        }

        private void CreateSendButton()
        {
            // Tạo nút hình tròn
            Button sendButton = new Button
            {
                Width = 57,  // Đường kính nút
                Height = 57,
                BackColor = Color.LightBlue, // Màu nền
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter, // Canh chữ và hình ảnh
                Text = "", // Không có chữ
            };

            // Loại bỏ viền mặc định của nút
            sendButton.FlatAppearance.BorderSize = 0;

            // Tạo hình ảnh mũi tên
            Bitmap arrowImage = new Bitmap(15, 15);  // Kích thước của mũi tên
            using (Graphics g = Graphics.FromImage(arrowImage))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    // Vẽ mũi tên
                    g.DrawLine(pen, 0, 7, 10, 7); // Đoạn ngang
                    g.DrawLine(pen, 7, 0, 10, 7); // Đoạn chéo trên
                    g.DrawLine(pen, 7, 14, 10, 7); // Đoạn chéo dưới
                }
            }

            // Gán hình ảnh mũi tên vào nút
            sendButton.Image = arrowImage;
            sendButton.ImageAlign = ContentAlignment.MiddleCenter; // Canh hình ảnh vào giữa

            // Tạo hình tròn cho nút
            var graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
            graphicsPath.AddEllipse(0, 0, sendButton.Width, sendButton.Height);
            sendButton.Region = new Region(graphicsPath);

            // Gắn sự kiện click
            sendButton.Click += (s, e) =>
            {
                MessageBox.Show("Sending...");
            };

            // Thêm nút vào form
            this.Controls.Add(sendButton);

            // Đặt vị trí nút căn giữa form
            sendButton.Left = (this.ClientSize.Width - sendButton.Width + 250) / 2;
            sendButton.Top = this.ClientSize.Height - sendButton.Height + 63;
        }




        private void CameraForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            CenterPictureBox();

            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterInfo in filterInfoCollection)
            {
                comboBoxCamera.Items.Add(filterInfo.Name);
            }
            comboBoxCamera.SelectedIndex = 0;

            videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[comboBoxCamera.SelectedIndex].MonikerString);
            videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            videoCaptureDevice.Start();
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Lấy frame hiện tại
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();

            // Resize frame để vừa với PictureBox (1280x720)
            Bitmap resizedFrame = new Bitmap(pictureBox.Width, pictureBox.Height);
            using (Graphics g = Graphics.FromImage(resizedFrame))
            {
                g.DrawImage(frame, 0, 0, pictureBox.Width, pictureBox.Height);

                // Thiết lập màu và độ dày nét vẽ
                Pen orangePen = new Pen(Color.Orange, 2);

                // Tính toán tọa độ và kích thước khung hình chữ nhật ở giữa
                int rectWidth = 350; // Chiều rộng của khung
                int rectHeight = 500; // Chiều cao của khung
                int rectX = (resizedFrame.Width - rectWidth) / 2; // Tọa độ X
                int rectY = (resizedFrame.Height - rectHeight) / 2; // Tọa độ Y

                // Vẽ hình chữ nhật
                g.DrawRectangle(orangePen, rectX, rectY, rectWidth, rectHeight);
            }

            // Hiển thị hình ảnh đã chỉnh sửa lên PictureBox
            pictureBox.Image = resizedFrame;

            // Giải phóng tài nguyên cũ
            frame.Dispose();
        }


        private void CameraForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoCaptureDevice != null && videoCaptureDevice.IsRunning)
            {
                videoCaptureDevice.SignalToStop();
                videoCaptureDevice.WaitForStop();
                videoCaptureDevice = null;
            }

            // Thoát toàn bộ ứng dụng
            Application.Exit();
        }

        private void CenterPictureBox()
        {
            pictureBox.Left = (this.ClientSize.Width - pictureBox.Width) / 2;
            pictureBox.Top = (this.ClientSize.Height - pictureBox.Height) / 2;
        }

        private void CenterButton()
        {
            if(captureButton == null)
            {
                return;
            }
            int bottomMargin = 50;
            captureButton.Left = (this.ClientSize.Width - captureButton.Width) / 2;
            captureButton.Top = this.ClientSize.Height - captureButton.Height - bottomMargin;
        }

        private void CameraForm_Resize(object sender, EventArgs e)
        {
            CenterPictureBox();
            CenterButton();
        }

        private void CaptureAndSaveAndCropImage()
        {
            // Kiểm tra xem pictureBox có hình ảnh hay không
            if (pictureBox.Image != null)
            {
                try
                {
                    // Đường dẫn thư mục lưu ảnh
                    string folderPath = @"C:\CapturedImages"; // Đảm bảo thư mục này tồn tại
                    if (!System.IO.Directory.Exists(folderPath))
                    {
                        System.IO.Directory.CreateDirectory(folderPath);
                    }

                    // Tạo tên file cho ảnh gốc
                    string originalFileName = $"original_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                    string originalFilePath = System.IO.Path.Combine(folderPath, originalFileName);

                    // Lưu ảnh gốc vào thư mục
                    pictureBox.Image.Save(originalFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                    // Tải lại ảnh gốc để thực hiện cắt
                    Bitmap originalImage = new Bitmap(originalFilePath);

                    // Tính toán tọa độ và kích thước cắt
                    int rectWidth = 350; // Chiều rộng của khung
                    int rectHeight = 500; // Chiều cao của khung
                    int rectX = (originalImage.Width - rectWidth) / 2; // Tọa độ X
                    int rectY = (originalImage.Height - rectHeight) / 2; // Tọa độ Y

                    // Tạo hình chữ nhật cắt
                    Rectangle cropRect = new Rectangle(rectX, rectY, rectWidth, rectHeight);

                    // Cắt ảnh
                    Bitmap croppedImage = originalImage.Clone(cropRect, originalImage.PixelFormat);

                    // Tạo tên file cho ảnh đã cắt
                    string croppedFileName = $"cropped_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                    string croppedFilePath = System.IO.Path.Combine(folderPath, croppedFileName);

                    // Lưu ảnh đã cắt
                    croppedImage.Save(croppedFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                    // Gán ảnh đã cắt vào PictureBox
                    pictureBox.Image = croppedImage;

                    // Giải phóng tài nguyên
                    originalImage.Dispose();

                    // Thông báo thành công
                    MessageBox.Show($"Ảnh gốc được lưu tại: {originalFilePath}\nẢnh đã cắt được lưu tại: {croppedFilePath}");
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi
                    MessageBox.Show($"Lỗi khi lưu và cắt ảnh: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Không có hình ảnh để lưu và cắt.");
            }
        }

    }


}

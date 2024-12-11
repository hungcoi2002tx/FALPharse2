using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AuthenExamTakePicture
{
    public partial class CameraForm : Form
    {
        private FilterInfoCollection filterInfoCollection;
        private VideoCaptureDevice videoCaptureDevice;
        private System.Windows.Forms.Button captureButton;
        private bool frameReceived = false;
        private Timer frameCheckTimer;
        private string base64Image;
        string folderPath = @"C:\CapturedImages";

        public string ExamCode { get; set; }
        public string StudentCode { get; set; }

        public CameraForm()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            CreateRoundButton();
            this.Resize += CameraForm_Resize;
            this.FormClosing += CameraForm_FormClosing;
        }

        private void CreateRoundButton()
        {
            captureButton = new System.Windows.Forms.Button
            {
                Width = 60,
                Height = 60,
                BackColor = Color.Orange,
                FlatStyle = FlatStyle.Flat,
            };

            captureButton.FlatAppearance.BorderSize = 0;
            captureButton.FlatAppearance.MouseOverBackColor = Color.DarkOrange;
            captureButton.FlatAppearance.MouseDownBackColor = Color.OrangeRed;

            var graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
            graphicsPath.AddEllipse(0, 0, captureButton.Width, captureButton.Height);
            captureButton.Region = new Region(graphicsPath);

            captureButton.Click += (s, e) =>
            {
                CaptureAndSaveAndCropImage();
            };

            this.Controls.Add(captureButton);
        }

        private void CameraForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;

            // Lấy danh sách các thiết bị camera
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            // Kiểm tra nếu không tìm thấy thiết bị camera nào
            if (filterInfoCollection.Count == 0)
            {
                DialogResult result = MessageBox.Show(
                   "No suitable camera devices found. Please contact support for assistance.",
                   "Notification",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Warning
                );

                if (result == DialogResult.OK)
                {
                    // Show an alternate form and close this one
                    this.Hide();
                    InfoForm infoForm = new InfoForm();
                    if (infoForm.ShowDialog() == DialogResult.OK)
                    {
                        // Additional logic if needed
                    }
                    this.Close();
                }
                return;
            }

            // Thêm các camera vào ComboBox
            foreach (FilterInfo filterInfo in filterInfoCollection)
            {
                comboBoxCamera.Items.Add(filterInfo.Name);
            }

            comboBoxCamera.SelectedIndex = 0;

            try
            {
                videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[comboBoxCamera.SelectedIndex].MonikerString);
                videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;

                // Bắt đầu luồng video
                videoCaptureDevice.Start();

                // Kiểm tra nếu không nhận được khung hình trong vòng 3 giây đầu tiên
                if (!CheckCameraAvailability(videoCaptureDevice))
                {
                    MessageBox.Show(
                        "Camera is currently in use by another application or unavailable. Please try again.",
                        "Camera Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    videoCaptureDevice.SignalToStop();
                    videoCaptureDevice.WaitForStop();
                    videoCaptureDevice = null;

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể truy cập camera.\nChi tiết lỗi: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                this.Close();
            }
        }

        private bool CheckCameraAvailability(VideoCaptureDevice device)
        {
            bool frameReceived = false;

            void OnNewFrame(object sender, NewFrameEventArgs eventArgs)
            {
                frameReceived = true;
                device.NewFrame -= OnNewFrame; // Ngừng lắng nghe sự kiện sau khi nhận được khung hình
            }

            device.NewFrame += OnNewFrame;

            // Chờ tối đa 3 giây
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (!frameReceived && stopwatch.ElapsedMilliseconds < 3000)
            {
                Application.DoEvents();
            }

            device.NewFrame -= OnNewFrame; // Bảo đảm gỡ sự kiện nếu timeout
            return frameReceived;
        }


        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            frameReceived = true;

            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();

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

                g.DrawRectangle(orangePen, rectX, rectY, rectWidth, rectHeight);
            }

            pictureBox.Image = resizedFrame;

            frame.Dispose();
        }

        private string ConvertImageToBase64(string filePath)
        {
            try
            {
                // Đọc ảnh từ đường dẫn và chuyển đổi sang Base64
                byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chuyển đổi ảnh sang Base64: {ex.Message}");
                return string.Empty;
            }
        }

        private void FrameCheckTimer_Tick(object sender, EventArgs e)
        {
            if (!frameReceived)
            {
                // Nếu không nhận được khung hình trong 3 giây, hiển thị thông báo
                frameCheckTimer.Stop();
                DialogResult result = MessageBox.Show(
                    "Camera có thể đang được sử dụng bởi ứng dụng khác. Vui lòng đóng ứng dụng đó và thử lại.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                // Dừng video và đóng form
                if (videoCaptureDevice != null && videoCaptureDevice.IsRunning)
                {
                    videoCaptureDevice.SignalToStop();
                    videoCaptureDevice.WaitForStop();
                }

                if (result == DialogResult.OK)
                {
                    // Show an alternate form and close this one
                    this.Hide();
                    InfoForm infoForm = new InfoForm();
                    if (infoForm.ShowDialog() == DialogResult.OK)
                    {
                        // Additional logic if needed
                    }
                    this.Close();
                }
                return;
            }

            // Reset cờ để kiểm tra lần tiếp theo
            frameReceived = false;
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

        private void CameraForm_Resize(object sender, EventArgs e)
        {
            ResizeControls();
        }

        private void ResizeControls()
        {
            if (pictureBox != null)
            {
                double widthRatio = 0.7;
                double heightRatio = 0.7;

                // Tính kích thước mới của PictureBox
                int newWidth = (int)(this.ClientSize.Width * widthRatio);
                int newHeight = (int)(this.ClientSize.Height * heightRatio);

                // Cập nhật kích thước PictureBox
                pictureBox.Width = newWidth;
                pictureBox.Height = newHeight;

                pictureBox.Left = (this.ClientSize.Width - pictureBox.Width) / 2;
                pictureBox.Top = (this.ClientSize.Height - pictureBox.Height) / 2;
            }

            if (label1 != null)
            {
                label1.Font = new Font(label1.Font.FontFamily, (float)(this.ClientSize.Height * 0.015));
                label1.Left = (int)(this.ClientSize.Width * 0.168);
                label1.Top = (int)(this.ClientSize.Height * 0.05);
            }

            if (label2 != null)
            {
                label2.Font = new Font(label1.Font.FontFamily, (float)(this.ClientSize.Height * 0.015));
                label2.Left = (int)(this.ClientSize.Width * 0.15);
                label2.Top = (int)(this.ClientSize.Height * 0.09);
            }


            if (comboBoxCamera != null)
            {
                comboBoxCamera.Width = (int)(this.ClientSize.Width * 0.2);
                comboBoxCamera.Left = label1 != null ? label1.Right + 10 : 0;
                comboBoxCamera.Top = label1 != null ? label1.Top : 0;
            }

            if (textBox1 != null)
            {
                textBox1.Width = (int)(this.ClientSize.Width * 0.2);
                textBox1.Left = label2 != null ? label2.Right + 10 : 0;
                textBox1.Top = label2 != null ? label2.Top : 0;
            }

            if (textBox1 != null && label3 != null && comboBoxCamera != null)
            {
                label3.Font = new Font(label3.Font.FontFamily, (float)(this.ClientSize.Height * 0.009));

                label3.Left = textBox1.Right + 20;
                label3.Top = (int)(this.ClientSize.Height * 0.03);
            }


            if (captureButton != null && pictureBox != null)
            {
                captureButton.Width = (int)(this.ClientSize.Width * 0.04);
                captureButton.Height = captureButton.Width;
                captureButton.Left = (this.ClientSize.Width - captureButton.Width) / 2;
                captureButton.Top = pictureBox.Bottom + 10;
                UpdateButtonShape(captureButton);
            }
        }

        private void UpdateButtonShape(System.Windows.Forms.Button button)
        {
            var graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
            graphicsPath.AddEllipse(0, 0, button.Width, button.Height);
            button.Region = new Region(graphicsPath);
        }

        private void CaptureAndSaveAndCropImage()
        {
            if (pictureBox.Image == null)
            {
                MessageBox.Show("Không có hình ảnh để lưu và cắt.");
                return;
            }

            try
            {
                EnsureFolderExists(folderPath);

                // Lưu ảnh gốc
                Bitmap originalImage = new Bitmap(pictureBox.Image);
                string originalFilePath = SaveOriginalImage(originalImage);

                // Cắt ảnh
                using (Bitmap croppedImage = CropImage(originalImage, 350, 500))
                {
                    string croppedFilePath = SaveCroppedImage(croppedImage);

                    // Hiển thị ảnh đã cắt trong PictureBox
                    pictureBox.Image = new Bitmap(croppedFilePath);

                    // Chuyển ảnh đã cắt sang Base64
                    base64Image = ConvertImageToBase64(croppedFilePath);

                    if (videoCaptureDevice != null && videoCaptureDevice.IsRunning)
                    {
                        videoCaptureDevice.SignalToStop();
                        videoCaptureDevice.WaitForStop();
                        videoCaptureDevice = null;
                    }

                    // Mở form khác với dữ liệu
                    using (ImageForm imageForm = new ImageForm())
                    {
                        imageForm.ExamCode = ExamCode;
                        imageForm.StudentCode = StudentCode;
                        imageForm.CroppedFilePath = croppedFilePath;
                        imageForm.Base64Image = base64Image;

                        this.Hide();
                        if (imageForm.ShowDialog() == DialogResult.OK)
                        {
                            // Xử lý nếu cần khi form con trả về DialogResult.OK
                        }
                        this.Close();
                    }
                }

                originalImage.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu và cắt ảnh: {ex.Message}");
            }
        }


        private void EnsureFolderExists(string folderPath)
        {
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }
        }
        private string SaveOriginalImage(Image image)
        {
            string fileName = $"original_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            string filePath = System.IO.Path.Combine(folderPath, fileName);
            image.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            return filePath;
        }

        private Bitmap CropImage(Bitmap originalImage, int width, int height)
        {
            int rectX = (originalImage.Width - width) / 2;
            int rectY = (originalImage.Height - height) / 2;
            Rectangle cropRect = new Rectangle(rectX, rectY, width, height);
            return originalImage.Clone(cropRect, originalImage.PixelFormat);
        }

        private string SaveCroppedImage(Bitmap croppedImage)
        {
            string fileName = $"cropped_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            string filePath = System.IO.Path.Combine(folderPath, fileName);
            croppedImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            return filePath;
        }
    }
}

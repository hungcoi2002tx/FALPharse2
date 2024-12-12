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
using AuthenExamCameraLibrary;

namespace AuthenExamTakePicture
{
    public partial class CameraForm : Form
    {
        private CameraHandler cameraHandler;
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
            cameraHandler = new CameraHandler();
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

            filterInfoCollection = cameraHandler.GetListCamera();

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

                if (!cameraHandler.IsCameraInUse(videoCaptureDevice, 5000))
                {
                    DialogResult result = MessageBox.Show(
                        "Camera is currently in use by another application or unavailable. Do you want to retry?",
                        "Camera Error",
                        MessageBoxButtons.RetryCancel,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.Retry)
                    {
                        this.Hide();
                        Form newForm = new CameraForm();
                        newForm.ShowDialog();
                        this.Close();
                    }
                    else
                    {
                        // Người dùng Cancel, đóng form
                        videoCaptureDevice.SignalToStop();
                        videoCaptureDevice.WaitForStop();
                        videoCaptureDevice = null;

                        this.Close();
                    }
                }

                cameraHandler.StartCamera(videoCaptureDevice, pictureBox);
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

        private void CameraForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            cameraHandler.StopCamera(videoCaptureDevice);
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
            try
            {
                var imageBinary = cameraHandler.CaptureAndCropImageToBinaryString(pictureBox, folderPath, true);
                var imageCapture = ConvertBinaryStringToImage(imageBinary);

                // Mở form khác với dữ liệu
                using (ImageForm imageForm = new ImageForm())
                {
                    imageForm.ExamCode = ExamCode;
                    imageForm.StudentCode = StudentCode;
                    imageForm.ImageCapture = imageCapture;

                    this.Hide();
                    if (imageForm.ShowDialog() == DialogResult.OK)
                    {
                        // Xử lý nếu cần khi form con trả về DialogResult.OK
                    }
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu và cắt ảnh: {ex.Message}");
            }
        }

        private Image ConvertBinaryStringToImage(string binaryString)
        {
            if (string.IsNullOrEmpty(binaryString))
                throw new ArgumentException("Chuỗi nhị phân rỗng hoặc null.", nameof(binaryString));

            // Chuỗi nhị phân phải có độ dài chia hết cho 8
            if (binaryString.Length % 8 != 0)
                throw new FormatException("Chuỗi nhị phân không hợp lệ (không chia hết cho 8).");

            int byteCount = binaryString.Length / 8;
            byte[] imageBytes = new byte[byteCount];

            for (int i = 0; i < byteCount; i++)
            {
                string byteString = binaryString.Substring(i * 8, 8);
                imageBytes[i] = Convert.ToByte(byteString, 2);
            }

            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                return Image.FromStream(ms);
            }
        }
    }
}

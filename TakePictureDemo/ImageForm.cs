using Newtonsoft.Json;
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
using System.Drawing.Drawing2D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using AuthenExamTakePicture.DTOs;

namespace AuthenExamTakePicture
{
    public partial class ImageForm : Form
    {
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.Button retryButton;
        private string apiUrl = "https://localhost:7237/api/Image/SaveFile";

        public string ExamCode { get; set; }
        public string StudentCode { get; set; }
        public string CroppedFilePath { get; set; }
        public string Base64Image { get; set; }

        public ImageForm()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.WindowState = FormWindowState.Maximized; // Toàn màn hình
            //this.FormBorderStyle = FormBorderStyle.None; // Loại bỏ viền cửa sổ
            CreateSendButton();
            CreateRetryButton();
            croppedPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            this.Resize += ImageForm_Resize;
        }

        private void ImageForm_Load(object sender, EventArgs e)
        {
            croppedPictureBox.Image = new Bitmap(CroppedFilePath);
        }

        private void CreateRetryButton()
        {
            retryButton = new System.Windows.Forms.Button
            {
                Width = 60,
                Height = 60,
                BackColor = Color.Orange,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Re-Try",
                ForeColor = Color.White
            };

            retryButton.FlatAppearance.BorderSize = 0;

            // Bo góc cho nút
            var graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
            graphicsPath.AddEllipse(0, 0, retryButton.Width, retryButton.Height);
            retryButton.Region = new Region(graphicsPath);

            // Sự kiện khi nhấn nút Re-Try
            retryButton.Click += (s, e) =>
            {
                HandleRetryButtonClick();
            };

            // Vị trí nút trên form
            retryButton.Location = new Point(this.ClientSize.Width - 80, this.ClientSize.Height - 80);
            retryButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            this.Controls.Add(retryButton);
        }

        private void HandleRetryButtonClick()
        {
            using (CameraForm formCamera = new CameraForm())
            {
                formCamera.ExamCode = ExamCode;
                formCamera.StudentCode = StudentCode;

                this.Hide();
                if (formCamera.ShowDialog() == DialogResult.OK)
                {

                }
                this.Close();
            }
        }


        private void CreateSendButton()
        {
            sendButton = new System.Windows.Forms.Button
            {
                Width = 60,
                Height = 60,
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "",
            };

            sendButton.FlatAppearance.BorderSize = 0;

            Bitmap arrowImage = new Bitmap(15, 15);
            using (Graphics g = Graphics.FromImage(arrowImage))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    g.DrawLine(pen, 0, 7, 10, 7);
                    g.DrawLine(pen, 7, 0, 10, 7);
                    g.DrawLine(pen, 7, 14, 10, 7);
                }
            }

            sendButton.Image = arrowImage;
            sendButton.ImageAlign = ContentAlignment.MiddleCenter;

            var graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
            graphicsPath.AddEllipse(0, 0, sendButton.Width, sendButton.Height);
            sendButton.Region = new Region(graphicsPath);

            sendButton.Click += (s, e) =>
            {
                SendDataToServer();
            };

            this.Controls.Add(sendButton);
        }

        private void SendDataToServer()
        {
            var saveImageDTO = new SaveImageDto
            {
                ImageBase = Base64Image, // Biến toàn cục chứa ảnh Base64
                ExamCode = ExamCode,
                StudentCode = StudentCode
            };

            try
            {
                // Chuyển đổi DTO thành JSON
                string jsonContent = JsonConvert.SerializeObject(saveImageDTO);

                // Tạo HttpWebRequest
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
                request.Method = "POST";
                request.ContentType = "application/json";
                byte[] data = Encoding.UTF8.GetBytes(jsonContent);
                request.ContentLength = data.Length;

                // Ghi dữ liệu vào body của request
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                // Nhận phản hồi từ server
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseData = reader.ReadToEnd();
                        MessageBox.Show($"Data sent successfully! Response: {responseData}");
                    }
                }
            }
            catch (WebException ex)
            {
                // Xử lý lỗi từ server
                using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                {
                    string errorData = reader.ReadToEnd();
                    MessageBox.Show($"Failed to send data. Error: {errorData}");
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khác
                MessageBox.Show($"Error while sending data: {ex.Message}");
            }
        }

        private void ImageForm_Resize(object sender, EventArgs e)
        {
            // Kích thước khung hình
            int rectWidth = 500;  // Chiều rộng của khung
            int rectHeight = 650; // Chiều cao của khung

            // Tính toán vị trí cho croppedPictureBox (đặt ở trung tâm)
            int pictureBoxX = (this.ClientSize.Width - rectWidth) / 2;
            int pictureBoxY = (this.ClientSize.Height - rectHeight) / 2;
            croppedPictureBox.Location = new Point(pictureBoxX, pictureBoxY);
            croppedPictureBox.Size = new Size(rectWidth, rectHeight);

            // Đặt label1 ngay phía trên croppedPictureBox
            label1.Location = new Point(pictureBoxX, pictureBoxY - 30); // Cách trên 30px
            label1.AutoSize = true; // Tự động điều chỉnh kích thước theo nội dung

            // Vị trí nút sendButton (bên phải croppedPictureBox)
            sendButton.Location = new Point(pictureBoxX + rectWidth + 20, pictureBoxY + rectHeight - sendButton.Height);

            // Vị trí nút retryButton (bên trái croppedPictureBox)
            retryButton.Location = new Point(pictureBoxX - retryButton.Width - 20, pictureBoxY + rectHeight - retryButton.Height);
        }
    }
}

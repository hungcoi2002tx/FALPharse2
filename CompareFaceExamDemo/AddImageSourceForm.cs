using AuthenExamCompareFace.Dtos;
using AuthenExamCompareFace.Models;
using AuthenExamCompareFace.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AuthenExamCompareFace
{
    public partial class AddImageSourceForm : Form
    {
        public ImageSourceDto? ImageSourceData { get; set; }

        public AddImageSourceForm()
        {
            InitializeComponent();
           
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            textBox2.ReadOnly = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                // Chỉ cho phép chọn các file có định dạng ảnh
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.Title = "Select an Image File";

                // Hiển thị hộp thoại chọn file
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string selectedFilePath = openFileDialog.FileName;
                        textBox2.Text = selectedFilePath;

                        using (var stream = new FileStream(selectedFilePath, FileMode.Open, FileAccess.Read))
                        {
                            pictureBox1.Image = Image.FromStream(stream); // Load ảnh từ stream
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool IsImageEqual(string filePath1, string filePath2)
        {
            using (var hashAlgorithm = SHA256.Create())
            {
                byte[] hash1 = GetFileHash(filePath1, hashAlgorithm);
                byte[] hash2 = GetFileHash(filePath2, hashAlgorithm);

                return hash1.SequenceEqual(hash2);
            }
        }

        private byte[] GetFileHash(string filePath, HashAlgorithm hashAlgorithm)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return hashAlgorithm.ComputeHash(stream);
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            SettingModel _settingForm = Config.GetSetting();
            var destinationPath = Path.Combine(_settingForm.DirectoryImageSource, $"{textBox1.Text.Trim().ToUpper()}.jpg");
            string fileName = Path.GetFileName(textBox2.Text);


            if (File.Exists(destinationPath))
            {
                if (IsImageEqual(destinationPath, textBox2.Text))
                {
                    MessageBox.Show($"The image already exists and is identical.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var resultOverride = MessageBox.Show($"The student code image {textBox1.Text.Trim()} already exists. Do you want to overwrite it?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (resultOverride == DialogResult.No)
                {
                    return;
                }
            }
            else
            {
                var result = MessageBox.Show($"Are you sure you want to add the image {fileName.Trim()} to student {textBox1.Text.Trim()}?",
                                             "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            try
            {
                // Kiểm tra và tạo thư mục nếu cần
                var directory = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Sử dụng FileStream để tránh khóa file
                using (var stream = new FileStream(textBox2.Text, FileMode.Open, FileAccess.Read))
                {
                    using (var image = Image.FromStream(stream))
                    {
                        // Tạo encoder để đảm bảo định dạng ảnh
                        var code = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                        if (code == null)
                        {
                            MessageBox.Show("JPEG code not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        var encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);

                        // Ghi đè ảnh trực tiếp
                        image.Save(destinationPath, code, encoderParams);
                    }
                }


                // Thông báo thành công
                MessageBox.Show($"The image {fileName.Trim()} has been successfully saved for student {textBox1.Text.Trim()}.",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                var parentForm = this.Owner as SourceImageForm; // Thay thế "YourParentFormName" bằng tên form gọi
                if (parentForm != null)
                {
                    parentForm.GetImageSourceData();
                    parentForm.LoadListData();
                }

                this.Close();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"No access to file or directory: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error accessing file or directory: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the image: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private bool ValidateInputs()
        {
            Regex regex = new Regex(@"^[a-zA-Z]{2}\d*");

            if (string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                MessageBox.Show("Please enter student code.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(textBox2.Text.Trim()) || !File.Exists(textBox2.Text))
            {
                MessageBox.Show("Please select a valid photo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!regex.IsMatch(textBox1.Text))
            {
                MessageBox.Show("Please enter your student code correctly\n Example: HE161572", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void AddImageSourceForm_Load(object sender, EventArgs e)
        {
            if (ImageSourceData != null)
            {
                textBox2.Text = ImageSourceData.ImagePath;
                textBox1.Text = ImageSourceData.StudentNumber;

                using (var stream = new FileStream(ImageSourceData.ImagePath, FileMode.Open, FileAccess.Read))
                {
                    pictureBox1.Image = Image.FromStream(stream); // Load ảnh từ stream
                }
            }
        }
    }
}

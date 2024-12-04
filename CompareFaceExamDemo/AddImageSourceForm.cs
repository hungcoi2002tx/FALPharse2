using AuthenExamCompareFaceExam.Dtos;
using AuthenExamCompareFaceExam.Models;
using AuthenExamCompareFaceExam.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AuthenExamCompareFaceExam
{
    public partial class AddImageSourceForm : Form
    {
        public ImageSourceData? ImageSourceData { get; set; }

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
                var resultOverride = MessageBox.Show($"Ảnh của mã sinh viên {textBox1.Text.Trim()} đã tồn tại. Bạn có muốn ghi đè không?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (resultOverride == DialogResult.No)
                {
                    return;
                }
            }
            else
            {
                var result = MessageBox.Show($"Bạn có chắc chắn muốn thêm ảnh {fileName.Trim()} cho sinh viên {textBox1.Text.Trim()}?",
                                             "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                        var codec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                        if (codec == null)
                        {
                            throw new Exception("Không tìm thấy codec JPEG.");
                        }

                        var encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);

                        // Ghi đè ảnh trực tiếp
                        image.Save(destinationPath, codec, encoderParams);
                    }
                }


                // Thông báo thành công
                MessageBox.Show($"Ảnh {fileName.Trim()} đã được lưu thành công cho sinh viên {textBox1.Text.Trim()}.",
                                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show($"Không có quyền truy cập vào file hoặc thư mục: {ex.Message}",
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Lỗi khi truy cập file hoặc thư mục: {ex.Message}",
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi khi lưu ảnh: {ex.Message}",
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private bool ValidateInputs()
        {
            Regex regex = new Regex(@"^[a-zA-Z]{2}\d{6}$");

            if (string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                MessageBox.Show("Vui lòng nhập mã sinh viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(textBox2.Text.Trim()) || !File.Exists(textBox2.Text))
            {
                MessageBox.Show("Vui lòng chọn ảnh hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!regex.IsMatch(textBox1.Text))
            {
                MessageBox.Show("Vui lòng nhập mã sinh viên đúng quy tắc\n Ví dụ: HE161572", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void AddImageSourceForm_Load(object sender, EventArgs e)
        {
            if (ImageSourceData != null)
            {
                //textBox2.Text = ImageSourceData.ImagePath;
                textBox1.Text = ImageSourceData.StudentNumber;

                //using (var stream = new FileStream(ImageSourceData.ImagePath, FileMode.Open, FileAccess.Read))
                //{
                //    pictureBox1.Image = Image.FromStream(stream); // Load ảnh từ stream
                //}
            }
        }

    }
}

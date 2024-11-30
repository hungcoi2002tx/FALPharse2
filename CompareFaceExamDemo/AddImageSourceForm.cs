using CompareFaceExamDemo.Models;
using CompareFaceExamDemo.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CompareFaceExamDemo
{
    public partial class AddImageSourceForm : Form
    {
        public AddImageSourceForm()
        {
            InitializeComponent();
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

                        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBox1.Image = Image.FromFile(selectedFilePath);
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

            if (File.Exists(destinationPath))
            {
                var resultOverride = MessageBox.Show($"Ảnh của mã sinh viên {textBox1.Text.Trim()} đã tồn tại. Bạn có muốn ghi đè không?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (resultOverride == DialogResult.No)
                {
                    return;
                }
            }


            // Lấy tên file
            var fileName = Path.GetFileName(textBox2.Text);

            // Xác nhận hành động
            var result = MessageBox.Show($"Bạn có chắc chắn muốn thêm ảnh {fileName.Trim()} cho sinh viên {textBox1.Text.Trim()}?",
                                         "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return;
            }

            try
            {
                // Kiểm tra và tạo thư mục nếu cần
                var directory = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Lưu ảnh vào thư mục đích
                using (var image = Image.FromFile(textBox2.Text))
                {
                    image.Save(destinationPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                }

                // Thông báo thành công
                MessageBox.Show($"Ảnh {fileName.Trim()} đã được lưu thành công cho sinh viên {textBox1.Text.Trim()}.",
                                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                // Thông báo lỗi nếu có ngoại lệ xảy ra
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
    }
}

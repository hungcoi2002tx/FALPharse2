using CompareFaceExamDemo.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompareFaceExamDemo
{
    public partial class SourceImageForm : Form
    {
        private readonly Config _config;

        public SourceImageForm()
        {
            InitializeComponent();

            try
            {
                _config = Config.LoadConfig();
                if (string.IsNullOrEmpty(_config.SourceImageDirectory))
                {
                    MessageBox.Show("Không tìm thấy thư mục SourceImageDirectory trong cấu hình.", "Lỗi cấu hình", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close(); // Đóng form nếu thiếu cấu hình
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải cấu hình: {ex.Message}", "Lỗi cấu hình", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtStudentCode.Text.Trim()))
            {
                MessageBox.Show("Vui lòng nhập mã sinh viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var filePath = Path.Combine(_config.SourceImageDirectory, $"{txtStudentCode.Text.Trim()}.jpg");
            if (File.Exists(filePath))
            {
                pictureBoxSourceImage.Image = Image.FromFile(filePath);
            }
            else
            {
                MessageBox.Show("Không tìm thấy ảnh nguồn cho sinh viên này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtStudentCode.Text.Trim()))
            {
                MessageBox.Show("Vui lòng nhập mã sinh viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Đặt phần mở rộng mặc định là .jpg
                    var destinationPath = Path.Combine(_config.SourceImageDirectory, $"{txtStudentCode.Text.Trim()}.jpg");

                    // Đảm bảo ảnh được lưu dưới định dạng .jpg
                    var image = Image.FromFile(openFileDialog.FileName);
                    image.Save(destinationPath, System.Drawing.Imaging.ImageFormat.Jpeg);

                    // Hiển thị ảnh đã lưu trên PictureBox
                    pictureBoxSourceImage.Image = Image.FromFile(destinationPath);
                }
            }
        }



        private void btnDeleteImage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtStudentCode.Text.Trim()))
            {
                MessageBox.Show("Vui lòng nhập mã sinh viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var filePath = Path.Combine(_config.SourceImageDirectory, $"{txtStudentCode.Text.Trim()}.jpg");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                pictureBoxSourceImage.Image = null;
                MessageBox.Show("Đã xóa ảnh thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Không tìm thấy ảnh để xóa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

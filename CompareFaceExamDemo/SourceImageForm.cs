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

namespace CompareFaceExamDemo
{
    public partial class SourceImageForm : Form
    {
        private SettingModel _settingForm;

        public SourceImageForm()
        {
            InitializeComponent();

            try
            {
                _settingForm = Config.GetSetting();

                if (string.IsNullOrEmpty(_settingForm.DirectoryImageSource))
                {
                    MessageBox.Show("Không tìm thấy thư mục DirectoryImageSource trong cấu hình.", "Lỗi cấu hình", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            var filePath = Path.Combine(_settingForm.DirectoryImageSource, $"{txtStudentCode.Text.Trim().ToUpper()}.jpg");
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
            var addImageSourceForm = new AddImageSourceForm();
            addImageSourceForm.Show();
        }

        private void btnDeleteImage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtStudentCode.Text.Trim()))
            {
                MessageBox.Show("Vui lòng nhập mã sinh viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var filePath = Path.Combine(_settingForm.DirectoryImageSource, $"{txtStudentCode.Text.Trim()}.jpg");

            // Xác nhận trước khi xóa ảnh
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa ảnh của sinh viên {txtStudentCode.Text.Trim()}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return; // Dừng hành động nếu chọn "No"
            }

            // Đảm bảo rằng ảnh đã được giải phóng khỏi bộ nhớ trước khi xóa
            if (pictureBoxSourceImage.Image != null)
            {
                pictureBoxSourceImage.Image.Dispose(); // Giải phóng tài nguyên ảnh
                pictureBoxSourceImage.Image = null; // Đặt lại hình ảnh trong PictureBox
            }

            // Kiểm tra xem ảnh có tồn tại không
            if (File.Exists(filePath))
            {
                try
                {
                    // Xóa ảnh
                    File.Delete(filePath);
                    MessageBox.Show("Đã xóa ảnh thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (IOException ex)
                {
                    // Nếu gặp lỗi khi xóa, thông báo cho người dùng
                    MessageBox.Show($"Không thể xóa ảnh. Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Không tìm thấy ảnh để xóa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnImport_Click(object sender, EventArgs e)
        {
            // Cho phép người dùng chọn thư mục
            using (var folderDialog = new FolderBrowserDialog())
            {
                var dialogResult = folderDialog.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    var selectedFolder = folderDialog.SelectedPath;

                    string pattern = @"^[A-Za-z]{2}\d+$"; // 2 chữ cái đầu và sau đó là các chữ số

                    // Lọc tất cả các file ảnh trong thư mục đã chọn và kiểm tra tên đúng định dạng
                    var imageFiles = Directory.GetFiles(selectedFolder, "*.jpg")
                                              .Where(file => Regex.IsMatch(Path.GetFileNameWithoutExtension(file), pattern)) // Kiểm tra tên file theo regex
                                              .ToList();

                    if (imageFiles.Count == 0)
                    {
                        MessageBox.Show("Không tìm thấy ảnh hợp lệ trong thư mục đã chọn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Kiểm tra và sao chép ảnh vào thư mục nguồn
                    int importedCount = 0;
                    int duplicateCount = 0;
                    var errorMessages = new List<string>(); // Danh sách lưu trữ thông báo lỗi

                    foreach (var filePath in imageFiles)
                    {
                        var studentCode = Path.GetFileNameWithoutExtension(filePath);
                        var destinationPath = Path.Combine(_settingForm.DirectoryImageSource, $"{studentCode}.jpg");

                        // Kiểm tra xem ảnh đã tồn tại chưa
                        if (File.Exists(destinationPath))
                        {
                            // Nếu đã tồn tại, ghi đè lên ảnh cũ
                            try
                            {
                                File.Copy(filePath, destinationPath, true); // Tham số `true` cho phép ghi đè
                                duplicateCount++;
                            }
                            catch (Exception ex)
                            {
                                errorMessages.Add($"Lỗi khi ghi đè ảnh {studentCode}: {ex.Message}");
                                continue;
                            }
                        }
                        else
                        {
                            try
                            {
                                // Sao chép ảnh vào thư mục nguồn
                                File.Copy(filePath, destinationPath);
                                importedCount++;
                            }
                            catch (Exception ex)
                            {
                                // Lưu thông báo lỗi vào danh sách
                                errorMessages.Add($"Lỗi khi sao chép ảnh {studentCode}: {ex.Message}");
                            }
                        }
                    }

                    // Thông báo tổng hợp về kết quả import
                    string message = $"Đã import {importedCount} ảnh thành công.";
                    if (duplicateCount > 0)
                    {
                        message += $"\n{duplicateCount} ảnh đã ghi đè.";
                    }

                    // Nếu có lỗi, hiển thị thông báo lỗi
                    if (errorMessages.Any())
                    {
                        message += "\n\nCác lỗi xảy ra:\n" + string.Join("\n", errorMessages);
                    }

                    // Hiển thị thông báo tổng hợp
                    MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnDeleteBulk_Click(object sender, EventArgs e)
        {
            // Mở form BulkDeleteForm để nhập mã sinh viên
            using (var bulkDeleteForm = new BulkDeleteForm())
            {
                if (bulkDeleteForm.ShowDialog() == DialogResult.OK)
                {
                    // Lấy danh sách mã sinh viên từ form
                    var input = bulkDeleteForm.StudentCodes;

                    if (string.IsNullOrEmpty(input))
                    {
                        MessageBox.Show("Vui lòng nhập mã sinh viên để xóa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Tách danh sách mã sinh viên từ chuỗi nhập vào, phân cách bằng dấu xuống dòng
                    var studentCodes = input.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    int deletedCount = 0;
                    int notFoundCount = 0;
                    var errorMessages = new List<string>(); // Danh sách lưu trữ thông báo lỗi

                    foreach (var studentCode in studentCodes)
                    {
                        var studentCodeTrimmed = studentCode.Trim(); // Loại bỏ khoảng trắng dư thừa
                        if (string.IsNullOrEmpty(studentCodeTrimmed)) continue;

                        var filePath = Path.Combine(_settingForm.DirectoryImageSource, $"{studentCodeTrimmed}.jpg");

                        // Kiểm tra xem ảnh có tồn tại không
                        if (File.Exists(filePath))
                        {
                            try
                            {
                                File.Delete(filePath);
                                deletedCount++;
                            }
                            catch (Exception ex)
                            {
                                // Lưu thông báo lỗi vào danh sách
                                errorMessages.Add($"Lỗi khi xóa ảnh {studentCodeTrimmed}: {ex.Message}");
                            }
                        }
                        else
                        {
                            notFoundCount++;
                        }
                    }

                    // Thông báo tổng hợp về kết quả xóa
                    string message = $"Đã xóa {deletedCount} ảnh thành công.";
                    if (notFoundCount > 0)
                    {
                        message += $"\n{notFoundCount} ảnh không tìm thấy.";
                    }

                    // Nếu có lỗi, hiển thị thông báo lỗi
                    if (errorMessages.Any())
                    {
                        message += "\n\nCác lỗi xảy ra:\n" + string.Join("\n", errorMessages);
                    }

                    // Hiển thị thông báo tổng hợp
                    MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void SourceImageForm_Load(object sender, EventArgs e)
        {

        }
    }
}

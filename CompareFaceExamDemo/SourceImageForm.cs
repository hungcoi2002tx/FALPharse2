using AuthenExamCompareFaceExam.Dtos;
using AuthenExamCompareFaceExam.Models;
using AuthenExamCompareFaceExam.Utils;
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

namespace AuthenExamCompareFaceExam
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
            string selectedFolder;
            // Cho phép người dùng chọn thư mục
            using (var folderDialog = new FolderBrowserDialog())
            {
                var dialogResult = folderDialog.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                     selectedFolder = folderDialog.SelectedPath;

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

                    List<UpdateFileResult> importFileResults = new List<UpdateFileResult>();

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
                                UpdateFileResult importFileResult = new UpdateFileResult
                                {
                                    FilePath = filePath,
                                    Message = "ảnh đã được ghi đè",
                                    Status = "SUCCESSOVERWRITE"
                                };
                                importFileResults.Add(importFileResult);
                            }
                            catch (Exception ex)
                            {
                                UpdateFileResult importFileResult = new UpdateFileResult
                                {
                                    FilePath = filePath,
                                    Message = $"Lỗi khi ghi đè ảnh {studentCode}: {ex.Message}",
                                    Status = "FAILOVERWRITE"
                                };
                                importFileResults.Add(importFileResult);
                                continue;
                            }
                        }
                        else
                        {
                            try
                            {
                                // Sao chép ảnh vào thư mục nguồn
                                File.Copy(filePath, destinationPath);
                                UpdateFileResult importFileResult = new UpdateFileResult
                                {
                                    FilePath = filePath,
                                    Message = "ảnh đã được lưu",
                                    Status = "SUCCESSSAVE"
                                };
                                importFileResults.Add(importFileResult);
                            }
                            catch (Exception ex)
                            {
                                // Lưu thông báo lỗi vào danh sách
                                UpdateFileResult importFileResult = new UpdateFileResult
                                {
                                    FilePath = filePath,
                                    Message = $"Lỗi khi lưu ảnh {studentCode}: {ex.Message}",
                                    Status = "FAILSAVE"
                                };
                                importFileResults.Add(importFileResult);
                            }
                        }
                    }

                    try
                    {
                        var fileName = selectedFolder + GenerateTimestampString("//importManyFileResult");
                        // Gọi phương thức Export để xuất danh sách ra file Excel
                        ExcelExporter.ExportListToExcel(importFileResults, fileName);

                        // Thông báo thành công
                        MessageBox.Show($"Đã thêm thành công, check kết quả trong {fileName}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        // Nếu có lỗi, hiển thị thông báo lỗi
                        MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private string GenerateTimestampString(string fileName)
        {
            DateTime now = DateTime.Now;
            string timestamp = now.ToString("yyyyMMdd_HHmmssfff");
            return $"{fileName}_{timestamp}.xlsx";
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

                    List<UpdateFileResult> deleteFileResult = new List<UpdateFileResult>();

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
                                deleteFileResult.Add(new UpdateFileResult
                                {
                                    FilePath = filePath,
                                    Message = "ảnh đã được xóa",
                                    Status = "SUCCESSDELETE"
                                });
                            }
                            catch (Exception ex)
                            {
                                deleteFileResult.Add(new UpdateFileResult
                                {
                                    FilePath = filePath,
                                    Message = "ảnh chưa được xóa",
                                    Status = "FAILDELETE"
                                });
                            }
                        }
                        else
                        {
                            deleteFileResult.Add(new UpdateFileResult
                            {
                                FilePath = filePath,
                                Message = "không tìm thấy ảnh",
                                Status = "FAILFIND"
                            });
                        }
                    }

                    try
                    {
                        var fileName = _settingForm.DirectoryImageSource + GenerateTimestampString("//deleteManyFileResult");
                        // Gọi phương thức Export để xuất danh sách ra file Excel
                        ExcelExporter.ExportListToExcel(deleteFileResult, fileName);

                        // Thông báo thành công
                        MessageBox.Show($"Đã xóa thành công, check kết quả trong {fileName}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        // Nếu có lỗi, hiển thị thông báo lỗi
                        MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SourceImageForm_Load(object sender, EventArgs e)
        {

        }
    }
}

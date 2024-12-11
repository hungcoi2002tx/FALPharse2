using Amazon.Auth.AccessControlPolicy;
using AuthenExamCompareFace.Dtos;
using AuthenExamCompareFace.Entities;
using AuthenExamCompareFace.Models;
using AuthenExamCompareFace.Utils;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
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

namespace AuthenExamCompareFace
{
    public partial class SourceImageForm : Form
    {
        private static readonly object fileLock = new object();
        private BindingSource? source = null;
        private List<ImageSourceDto>? imageSourceData = null;
        private string sourcePath;
        private string pattern = @"^[A-Za-z]{2}\d+$";
        public SourceImageForm()
        {
            InitializeComponent();

            try
            {

                var _settingForm = Config.GetSetting();
                sourcePath = _settingForm.DirectoryImageSource;
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

            var studentCode = txtStudentCode.Text.Trim().ToUpper();

            // Kiểm tra danh sách imageSourceData
            if (imageSourceData != null)
            {
                // Tìm kiếm trong imageSourceData
                var matchedData = imageSourceData.FirstOrDefault(data => data.StudentNumber.ToUpper() == studentCode);
                dataGridViewSourceImage.DataSource = new List<ImageSourceDto> { matchedData ?? new ImageSourceDto() };
                if (matchedData != null)
                {
                    return;
                }
            }

            // Nếu không tìm thấy trong imageSourceData, hiển thị thông báo
            MessageBox.Show("Không tìm thấy sinh viên này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewSourceImage.CurrentCell != null)
                {
                    int rowIndex = dataGridViewSourceImage.CurrentCell.RowIndex; // Lấy dòng hiện tại
                    var selectedRow = dataGridViewSourceImage.Rows[rowIndex];   // Dòng tương ứng

                    if (selectedRow.DataBoundItem is ImageSourceDto selectedResult)
                    {
                        var addImageSourceForm = new AddImageSourceForm
                        {
                            ImageSourceData = selectedResult // Gán dữ liệu qua thuộc tính
                        };
                        addImageSourceForm.Owner = this; // Thiết lập form cha
                        addImageSourceForm.ShowDialog(); // Hiển thị form con dưới dạng modal
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi add lưu ảnh, liên hệ admin", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnDeleteImage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtStudentCode.Text.Trim()))
            {
                MessageBox.Show("Vui lòng nhập mã sinh viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var _settingForm = Config.GetSetting();
            sourcePath = _settingForm.DirectoryImageSource;
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
                    GetImageSourceData();
                    LoadListData();
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

                    List<UpdateFileResultDto> importFileResults = new List<UpdateFileResultDto>();

                    // Lọc tất cả các file ảnh trong thư mục đã chọn và kiểm tra tên đúng định dạng
                    bool hasValidFiles = false; // Biến để theo dõi nếu có file hợp lệ

                    foreach (var filePath in Directory.GetFiles(selectedFolder, "*.jpg"))
                    {
                        var studentCode = Path.GetFileNameWithoutExtension(filePath);

                        // Kiểm tra tên file có đúng định dạng
                        if (!Regex.IsMatch(studentCode, pattern))
                        {
                            UpdateFileResultDto importFileResult = new UpdateFileResultDto
                            {
                                FilePath = filePath,
                                Message = "Tên file không đúng định dạng (phải có 2 chữ cái đầu và số phía sau).",
                                Status = "INVALIDFILENAME",
                                StudentNumber = studentCode
                            };
                            importFileResults.Add(importFileResult);
                            continue; // Bỏ qua file này và tiếp tục với file khác
                        }

                        // Nếu tên file đúng, xử lý bình thường
                        lock (fileLock) // Đảm bảo chỉ một luồng xử lý file này
                        {
                            var _settingForm = Config.GetSetting();
                            sourcePath = _settingForm.DirectoryImageSource;
                            var destinationPath = Path.Combine(_settingForm.DirectoryImageSource, $"{studentCode}.jpg");

                            try
                            {
                                // Nếu file tồn tại, xóa trước khi ghi đè
                                if (File.Exists(destinationPath))
                                {
                                    File.Delete(destinationPath);
                                }

                                // Copy file từ nguồn vào đích
                                File.Copy(filePath, destinationPath);

                                UpdateFileResultDto importFileResult = new UpdateFileResultDto
                                {
                                    FilePath = filePath,
                                    Message = "ảnh đã được lưu thành công",
                                    Status = "SUCCESS",
                                    StudentNumber = studentCode
                                };
                                importFileResults.Add(importFileResult);

                                hasValidFiles = true; // Đánh dấu có ít nhất một file hợp lệ
                            }
                            catch (IOException ioEx)
                            {
                                UpdateFileResultDto importFileResult = new UpdateFileResultDto
                                {
                                    FilePath = filePath,
                                    Message = $"File bị khóa hoặc đang sử dụng: {ioEx.Message}",
                                    Status = "FAIL",
                                    StudentNumber = studentCode
                                };
                                importFileResults.Add(importFileResult);
                            }
                            catch (Exception ex)
                            {
                                UpdateFileResultDto importFileResult = new UpdateFileResultDto
                                {
                                    FilePath = filePath,
                                    Message = $"Lỗi không xác định: {ex.Message}",
                                    Status = "FAIL",
                                    StudentNumber = studentCode
                                };
                                importFileResults.Add(importFileResult);
                            }
                        }
                    }

                    // Sau khi xử lý xong tất cả các file, kiểm tra nếu không có file hợp lệ
                    if (!hasValidFiles)
                    {
                        MessageBox.Show("Không tìm thấy ảnh hợp lệ trong thư mục đã chọn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return; // Thoát khỏi phương thức nếu không có file hợp lệ
                    }



                    try
                    {
                        var fileName = selectedFolder + GenerateTimestampString("//importManyFileResult");
                        // Gọi phương thức Export để xuất danh sách ra file Excel
                        ExcelExporter.ExportListToExcel(importFileResults, fileName);

                        // Thông báo thành công
                        MessageBox.Show($"Đã thêm thành công, check kết quả trong {fileName}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        GetImageSourceData();
                        LoadListData();
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

                    List<UpdateFileResultDto> deleteFileResult = new List<UpdateFileResultDto>();

                    foreach (var studentCode in studentCodes)
                    {
                        var studentCodeTrimmed = studentCode.Trim(); // Loại bỏ khoảng trắng dư thừa
                        if (string.IsNullOrEmpty(studentCodeTrimmed)) continue;

                        var _settingForm = Config.GetSetting();
                        sourcePath = _settingForm.DirectoryImageSource;
                        var filePath = Path.Combine(_settingForm.DirectoryImageSource, $"{studentCodeTrimmed}.jpg");

                        // Kiểm tra xem ảnh có tồn tại không
                        if (File.Exists(filePath))
                        {
                            try
                            {
                                File.Delete(filePath);
                                deleteFileResult.Add(new UpdateFileResultDto
                                {
                                    FilePath = filePath,
                                    Message = "ảnh đã được xóa",
                                    Status = "SUCCESSDELETE",
                                    StudentNumber = studentCodeTrimmed
                                });
                            }
                            catch (Exception ex)
                            {
                                deleteFileResult.Add(new UpdateFileResultDto
                                {
                                    FilePath = filePath,
                                    Message = "ảnh chưa được xóa",
                                    Status = "FAILDELETE",
                                    StudentNumber = studentCodeTrimmed
                                });
                            }
                        }
                        else
                        {
                            deleteFileResult.Add(new UpdateFileResultDto
                            {
                                FilePath = filePath,
                                Message = "không tìm thấy ảnh",
                                Status = "FAILFIND",
                                StudentNumber = studentCodeTrimmed
                            });
                        }
                    }

                    try
                    {
                        var _settingForm = Config.GetSetting();
                        sourcePath = _settingForm.DirectoryImageSource;
                        var fileName = _settingForm.DirectoryImageSource + GenerateTimestampString("//deleteManyFileResult");
                        // Gọi phương thức Export để xuất danh sách ra file Excel
                        ExcelExporter.ExportListToExcel(deleteFileResult, fileName);

                        // Thông báo thành công
                        MessageBox.Show($"Đã xóa thành công, check kết quả trong {fileName}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        GetImageSourceData();
                        LoadListData();
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
            dataGridViewSourceImage.Height = 905;
            pictureBoxSourceImage.Height = 750;
            GetImageSourceData();
            LoadListData();
        }

        public void LoadListData()
        {
            try
            {
                if (source == null)
                {
                    source = new BindingSource();
                }

                if (imageSourceData == null)
                {
                    imageSourceData = new List<ImageSourceDto>();
                }

                dataGridViewSourceImage.DataSource = null;
                dataGridViewSourceImage.Columns.Clear();
                dataGridViewSourceImage.Rows.Clear();
                dataGridViewSourceImage.AllowUserToAddRows = false;
                dataGridViewSourceImage.ScrollBars = ScrollBars.Both; // Hiển thị thanh cuộn ngang và dọc

                source.DataSource = imageSourceData;
                dataGridViewSourceImage.DataSource = source;
            }
            catch (Exception)
            {
                MessageBox.Show($"Liên hệ admin", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void GetImageSourceData()
        {
            try
            {
                var _settingForm = Config.GetSetting();
                sourcePath = _settingForm.DirectoryImageSource;
                List<ImageSourceDto> imageSourceDataClone = new List<ImageSourceDto>();
                var jpgFiles = Directory.GetFiles(sourcePath, "*.jpg");
                if (jpgFiles.Length > 0)
                {
                    foreach (var filePath in Directory.GetFiles(sourcePath, "*.jpg"))
                    {
                        var studentCode = Path.GetFileNameWithoutExtension(filePath);

                        // Kiểm tra tên file có đúng định dạng
                        if (!Regex.IsMatch(studentCode, pattern))
                        {
                            continue; // Bỏ qua file này và tiếp tục với file khác
                        }
                        else
                        {
                            ImageSourceDto isd = new ImageSourceDto
                            {
                                StudentNumber = studentCode,
                                ImagePath = filePath
                            };
                            imageSourceDataClone.Add(isd);
                        }
                    }
                    imageSourceData = imageSourceDataClone;
                }
                else
                {
                    MessageBox.Show($"No .jpg files found in the directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"No .jpg files found in the directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridViewSourceImage_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewSourceImage.CurrentCell != null)
            {
                int rowIndex = dataGridViewSourceImage.CurrentCell.RowIndex; // Lấy dòng hiện tại
                var selectedRow = dataGridViewSourceImage.Rows[rowIndex];   // Dòng tương ứng

                if (selectedRow.DataBoundItem is ImageSourceDto selectedResult)
                {
                    DisplayImages(selectedResult);
                    txtStudentCode.Text = selectedResult.StudentNumber;
                }
            }
            else
            {
                // Xử lý khi không có dòng nào được chọn
                if (pictureBoxSourceImage.Image != null)
                {
                    pictureBoxSourceImage.Image.Dispose();
                    pictureBoxSourceImage.Image = null;
                }
            }
        }

        private void DisplayImages(ImageSourceDto result)
        {
            var sourceImagePath = result.ImagePath;

            // Giải phóng ảnh hiện tại nếu có
            if (pictureBoxSourceImage.Image != null)
            {
                pictureBoxSourceImage.Image.Dispose();
                pictureBoxSourceImage.Image = null;
            }

            // Hiển thị SourceImage
            if (File.Exists(sourceImagePath))
            {
                try
                {
                    using (var stream = new FileStream(sourceImagePath, FileMode.Open, FileAccess.Read))
                    {
                        pictureBoxSourceImage.Image = Image.FromStream(stream); // Load ảnh từ stream
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể tải ảnh: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnLoadDataSource_Click(object sender, EventArgs e)
        {
            GetImageSourceData();
            LoadListData();
        }
    }
}

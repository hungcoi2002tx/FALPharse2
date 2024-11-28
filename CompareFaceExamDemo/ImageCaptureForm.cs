using CompareFaceExamDemo.Dtos;
using CompareFaceExamDemo.ExternalService;
using CompareFaceExamDemo.ExternalService.Recognition;
using CompareFaceExamDemo.Models;
using CompareFaceExamDemo.Utils;
using Share.SystemModel;
using System;
using System.Collections.Concurrent;
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
    public partial class ImageCaptureForm : Form
    {
        private CompareFaceAdapterRecognitionService _compareFaceService;
        private readonly FaceCompareService _faceCompareService;
        private readonly object _logLock = new object();
        private BindingSource? source = null;
        List<ResultCompareFaceDto>? listDataCompare = null;
        private bool isLoadData = false;

        public ImageCaptureForm(CompareFaceAdapterRecognitionService compareFaceService, FaceCompareService faceCompareService)
        {
            InitializeComponent();
            _compareFaceService = compareFaceService;
            _faceCompareService = faceCompareService;
            LoadListData();

        }

        private void LoadListData()
        {
            try
            {
                if (source == null)
                {
                    source = new BindingSource();
                }

                if (listDataCompare == null)
                {
                    listDataCompare = new List<ResultCompareFaceDto>();
                }

                dataGridViewImages.DataSource = null;
                dataGridViewImages.Columns.Clear();
                dataGridViewImages.Rows.Clear();
                dataGridViewImages.AllowUserToAddRows = false;
                dataGridViewImages.ScrollBars = ScrollBars.Both;  // Hiển thị thanh cuộn ngang và dọc

                source.DataSource = listDataCompare;
                dataGridViewImages.DataSource = source;

                if (!isLoadData)
                {
                    AddCheckBoxHeader();
                    isLoadData = true;
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Liên hệ admin", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddCheckBoxHeader()
        {
            // Tạo checkbox header
            CheckBox headerCheckBox = new CheckBox
            {
                Size = new System.Drawing.Size(15, 15)
            };
            headerCheckBox.CheckedChanged += HeaderCheckBox_CheckedChanged;

            // Đặt checkbox vào vị trí của cột đầu tiên
            var cellRectangle = dataGridViewImages.GetCellDisplayRectangle(0, -1, true);
            headerCheckBox.Location = new System.Drawing.Point(cellRectangle.Location.X + 20, cellRectangle.Location.Y + 5);

            // Thêm checkbox vào DataGridView
            dataGridViewImages.Controls.Add(headerCheckBox);
        }

        private void HeaderCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Thay đổi trạng thái tất cả các checkbox
            CheckBox headerCheckBox = (CheckBox)sender;
            foreach (DataGridViewRow row in dataGridViewImages.Rows)
            {
                DataGridViewCheckBoxCell checkBoxCell = (DataGridViewCheckBoxCell)row.Cells["checkBoxColumn"];
                checkBoxCell.Value = headerCheckBox.Checked;
            }
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            // Mở hộp thoại chọn thư mục
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
            {
                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    // Lấy đường dẫn của thư mục được chọn
                    string folderPath = folderBrowser.SelectedPath;
                    // Hiển thị đường dẫn trong TextBox
                    txtFolderPath.Text = folderPath;
                    // Load danh sách file từ thư mục được chọn
                    LoadImagesWithCheckbox(folderPath);
                    LoadListData();
                }
            }
        }

        private void LoadImagesWithCheckbox(string folderPath)
        {
            try
            {
                // Lấy danh sách các file trong thư mục
                string[] files = Directory.GetFiles(folderPath);

                // Regex để kiểm tra định dạng tên file: 2 chữ cái + 4 số
                Regex regex = new Regex(@"^[a-zA-Z]{2}\d{6}\.(jpg|png)$");
             
                // Lấy tên thư mục cuối cùng làm ExamCode
                string[] folderPathParts = folderPath.Split('\\');
                string lastPart = folderPathParts[folderPathParts.Length - 1];
                string examCode = lastPart;


                // Thêm dữ liệu vào danh sách
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                 
                    // Kiểm tra tên file có đúng định dạng không
                    if (regex.IsMatch(fileName))
                    {

                        AddSelectAllCheckBox();

                        ResultCompareFaceDto rcf = new ResultCompareFaceDto
                        {
                            StudentCode = Path.GetFileNameWithoutExtension(fileName),
                            Status = "Pending",
                            Confidence = 0.0,
                            ExamCode = examCode,
                            Time = DateTime.Now,
                            Message = "File loaded",
                        };
                        listDataCompare.Add(rcf);
                    }
                }

                // Hiển thị thông báo nếu không tìm thấy file phù hợp
                if (listDataCompare.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy file nào đáp ứng định dạng trong thư mục!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewImages.Rows)
            {
                DataGridViewCheckBoxCell checkBoxCell = (DataGridViewCheckBoxCell)row.Cells["checkBoxColumn"];
                checkBoxCell.Value = true; // Chọn tất cả checkbox
            }
        }

        private void AddSelectAllCheckBox()
        {
            // Tạo CheckBox
            CheckBox selectAllCheckBox = new CheckBox
            {
                Size = new System.Drawing.Size(15, 15),
                Location = new System.Drawing.Point(5, 5) // Tùy chỉnh vị trí
            };

            // Gắn sự kiện thay đổi trạng thái
            selectAllCheckBox.CheckedChanged += SelectAllCheckBox_CheckedChanged;

            // Đặt CheckBox vào tiêu đề của cột đầu tiên (Select Column)
            var headerCell = dataGridViewImages.GetCellDisplayRectangle(0, -1, true);
            selectAllCheckBox.Location = new System.Drawing.Point(headerCell.Location.X + 20, headerCell.Location.Y + 5);

            // Thêm CheckBox vào DataGridView
            dataGridViewImages.Controls.Add(selectAllCheckBox);
        }

        private void SelectAllCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox headerCheckBox = (CheckBox)sender;

            // Thay đổi trạng thái tất cả các checkbox trong bảng
            foreach (DataGridViewRow row in dataGridViewImages.Rows)
            {
                DataGridViewCheckBoxCell checkBoxCell = (DataGridViewCheckBoxCell)row.Cells["checkBoxColumn"];
                checkBoxCell.Value = headerCheckBox.Checked;
            }
        }

        private void dataGridViewImages_SelectionChanged(object sender, EventArgs e)
        {
            // Kiểm tra nếu có ít nhất một hàng được chọn
            if (dataGridViewImages.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridViewImages.SelectedRows[0];

                // Lấy tên file từ cột FileName
                string fileName = selectedRow.Cells["FileName"].Value?.ToString();

                if (!string.IsNullOrEmpty(fileName))
                {
                    string folderPath = txtFolderPath.Text; // Đường dẫn thư mục
                    string fullPath = Path.Combine(folderPath, fileName);

                    // Kiểm tra file tồn tại và hiển thị
                    if (File.Exists(fullPath))
                    {
                        if (pictureBoxPreview.Image != null)
                        {
                            pictureBoxPreview.Image.Dispose(); // Giải phóng ảnh cũ
                        }
                        pictureBoxPreview.Image = Image.FromFile(fullPath);
                    }
                    else
                    {
                        pictureBoxPreview.Image = null; // Xóa ảnh nếu file không tồn tại
                        MessageBox.Show($"File không tồn tại: {fullPath}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                // Không có hàng nào được chọn
                pictureBoxPreview.Image = null; // Xóa ảnh
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                List<(string, string)> compareImages = new List<(string, string)>();

                var sourceFile = Config.GetSetting();
                var urlSource = sourceFile.DirectoryImageSource;

                foreach (DataGridViewRow row in dataGridViewImages.Rows)
                {
                    // Kiểm tra nếu checkbox được tích
                    bool isChecked = Convert.ToBoolean(row.Cells["checkBoxColumn"].Value);
                    if (isChecked)
                    {
                        string? fullPathImageTarget = "";
                        string? fullPathImageSource = "";

                        try
                        {
                            string fileName = row.Cells["FileName"].Value.ToString() ?? "";
                            string folderPath = txtFolderPath.Text;

                            fullPathImageTarget = Path.Combine(folderPath, fileName);
                            fullPathImageSource = Path.Combine(urlSource, fileName);

                            compareImages.Add((fullPathImageTarget, fullPathImageSource));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi khi đọc file: {fullPathImageTarget}\nChi tiết: {ex.Message}");
                        }
                    }
                }

                if (compareImages.Count > 0)
                {
                    try
                    {
                        int maxDegreeOfParallelism = sourceFile.NumberOfThread;
                        int maxRetries = 3;
                        List<ComparisonResponse> listResults = await GetCompareResult(maxDegreeOfParallelism, compareImages, maxRetries);

                        List<CompareResponseResult> listResultData = listResults
                            .Where(response => response.Data != null)
                            .Select(response => response.Data!)
                            .ToList();

                        string folderPath = txtFolderPath.Text;
                        string[] folderPathParts = folderPath.Split('\\');
                        string lastPart = folderPathParts[folderPathParts.Length - 1];

                        string filePath = folderPath + "/" + DateTime.Now.ToString("ddMMyyyy_HHmmss_fff") + "-" + lastPart + ".xlsx";
                        ExcelExporter.ExportListToExcel(listResultData, filePath);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Không có file nào được chọn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<List<ComparisonResponse>> GetCompareResult(int maxDegreeOfParallelism, List<(string, string)> compareImages, int maxRetries)
        {
            try
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
                ConcurrentBag<ComparisonResponse> results = new ConcurrentBag<ComparisonResponse>();
                List<Task> tasks = new List<Task>();
                string logFilePath = "log.txt";

                foreach (var (targetImage, sourceImage) in compareImages)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();

                        try
                        {
                            bool success = false;
                            int retryCount = 0;
                            //string imgCompareBase64 = ConvertImageToBase64(targetImage);
                            //string imgSourceBase64 = ConvertImageToBase64(sourceImage);
                            ComparisonResponse? response = null;

                            while (!success && retryCount < maxRetries)
                            {
                                response = await _faceCompareService.CompareFacesAsync(sourceImage, targetImage);

                                if (CheckResponseCompare(response))
                                {
                                    results.Add(response);
                                    success = true;
                                }
                                else if (response.Status == 429)
                                {
                                    retryCount++;
                                    await Task.Delay(1000);
                                }
                                else
                                {
                                    LogError(logFilePath, response);
                                    break;
                                }
                            }

                            if (!success && retryCount >= maxRetries)
                            {
                                LogError(logFilePath, response, true);
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                return results.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void LogError(string logFilePath, ComparisonResponse? response, bool isRetryExceeded = false)
        {
            lock (_logLock)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(logFilePath, true))
                    {
                        string logMessage = isRetryExceeded
                            ? $"[{DateTime.Now}] ERROR: Retry limit exceeded. Response: {response?.ToString()}"
                            : $"[{DateTime.Now}] ERROR: Comparison failed. Response: {response?.ToString()}";

                        sw.WriteLine(logMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to log error: {ex.Message}");
                }
            }
        }


        private bool CheckResponseCompare(ComparisonResponse cr)
        {
            if (cr.Status == 200)
            {
                return true;
            }
            else if (cr.Status == 429)
            {
                return false;
            }
            else
            {
                return false;
            }
        }

        private string ConvertImageToBase64(Image image)
        {
            try
            {
                // Tạo một MemoryStream để lưu ảnh tạm thời
                using (MemoryStream ms = new MemoryStream())
                {
                    // Lưu ảnh vào MemoryStream dưới định dạng PNG (hoặc định dạng khác tùy chọn)
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                    // Chuyển nội dung MemoryStream sang mảng byte
                    byte[] imageBytes = ms.ToArray();

                    // Chuyển mảng byte thành chuỗi base64
                    string base64String = Convert.ToBase64String(imageBytes);

                    return base64String;
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                Console.WriteLine($"Lỗi chuyển đổi ảnh sang base64: {ex.Message}");
                return string.Empty;
            }
        }
    }
}

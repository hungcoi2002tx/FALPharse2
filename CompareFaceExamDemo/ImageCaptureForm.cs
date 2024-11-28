using CompareFaceExamDemo.Dtos;
using CompareFaceExamDemo.ExternalService;
using CompareFaceExamDemo.ExternalService.Recognition;
using CompareFaceExamDemo.Models;
using CompareFaceExamDemo.Utils;
using RestSharp;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CompareFaceExamDemo
{
    public partial class ImageCaptureForm : Form
    {
        private CompareFaceAdapterRecognitionService _compareFaceService;
        private readonly FaceCompareService _faceCompareService;
        private readonly object _logLock = new object();
        private BindingSource? source = null;
        private List<ResultCompareFaceDto>? listDataCompare = null;
        private int maxRetries = 3;

        public ImageCaptureForm(CompareFaceAdapterRecognitionService compareFaceService, FaceCompareService faceCompareService)
        {
            InitializeComponent();
            _compareFaceService = compareFaceService;
            _faceCompareService = faceCompareService;

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
                dataGridViewImages.ScrollBars = ScrollBars.Both; // Hiển thị thanh cuộn ngang và dọc
                dataGridViewImages.RowHeadersVisible = false;

                source.DataSource = listDataCompare;
                dataGridViewImages.DataSource = source;
            }
            catch (Exception)
            {
                MessageBox.Show($"Liên hệ admin", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            // Mở hộp thoại chọn thư mục
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
            {
                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    progressBarCompare.Value = 0;
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
                var sourceFile = Config.GetSetting();
                var urlSource = sourceFile.DirectoryImageSource;
                string[] files = Directory.GetFiles(folderPath);
                Regex regex = new Regex(@"^[a-zA-Z]{2}\d{6}\.(jpg|png)$");
                List<ResultCompareFaceDto> listDataCompareGetData = new List<ResultCompareFaceDto>();


                string[] folderPathParts = folderPath.Split('\\');
                string lastPart = folderPathParts[folderPathParts.Length - 1];
                string examCode = lastPart;

                foreach (string file in files)
                {
                    string ImageTagetPath = Path.GetFileName(file);

                    if (regex.IsMatch(ImageTagetPath))
                    {
                        string ImageSourcePath = GetImageSourcePath(urlSource, ImageTagetPath);

                        ResultCompareFaceDto rcf = new ResultCompareFaceDto
                        {
                            ImageSourcePath = ImageSourcePath,
                            ImageTagetPath = file,
                            StudentCode = Path.GetFileNameWithoutExtension(ImageTagetPath),
                            Status = "Pending",
                            Confidence = 0.0,
                            ExamCode = examCode,
                            Time = DateTime.Now,
                            Message = "File loaded",
                        };
                        listDataCompareGetData.Add(rcf);
                    }
                }
                listDataCompare = listDataCompareGetData;

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

        private string GetImageSourcePath(string urlSource, string imageFileName)
        {
            // Lấy tên file không có phần mở rộng
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imageFileName);

            // Đường dẫn cho file .jpg
            string jpgPath = Path.Combine(urlSource, fileNameWithoutExtension + ".jpg");

            if (File.Exists(jpgPath))
            {
                return jpgPath; // Trả về đường dẫn nếu file .jpg tồn tại
            }

            // Đường dẫn cho file .png
            string pngPath = Path.Combine(urlSource, fileNameWithoutExtension + ".png");

            if (File.Exists(pngPath))
            {
                return pngPath; // Trả về đường dẫn nếu file .png tồn tại
            }

            // Nếu cả hai file không tồn tại, trả về null hoặc xử lý khác
            return null;
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (listDataCompare.Count > 0)
                {
                    try
                    {
                        progressBarCompare.Maximum = listDataCompare.Count;
                        var sourceFile = Config.GetSetting();
                        int maxDegreeOfParallelism = sourceFile.NumberOfThread;

                        await GetCompareResult(maxDegreeOfParallelism, maxRetries);

                        string folderPath = txtFolderPath.Text;
                        string[] folderPathParts = folderPath.Split('\\');
                        string lastPart = folderPathParts[folderPathParts.Length - 1];

                        string filePath = folderPath + "/" + DateTime.Now.ToString("ddMMyyyy_HHmmss_fff") + "-" + lastPart + ".xlsx";
                        ExcelExporter.ExportListToExcel(listDataCompare, filePath);
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

        private string GetNote(int confident, double ConfidenceResponse)
        {
            if (ConfidenceResponse >= confident)
            {
                return "khuôn mặt giống nhau";
            }
            else
            {
                return "khuôn mặt KHÁC nhau";
            }
        }

        private async Task GetCompareResult(int maxDegreeOfParallelism, int maxRetries)
        {
            try
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
                List<Task> tasks = new List<Task>();
                var sourceFile = Config.GetSetting();
                var confident = sourceFile.Confident;

                // Đặt giá trị tối đa cho ProgressBar
                progressBarCompare.Invoke(new Action(() =>
                {
                    progressBarCompare.Maximum = listDataCompare.Count;
                    progressBarCompare.Value = 0; // Bắt đầu từ 0
                }));

                int completedCount = 0;

                foreach (var itemCompare in listDataCompare)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();

                        try
                        {
                            bool success = false;
                            int retryCount = 0;
                            ComparisonResponse? response = null;

                            while (!success && retryCount < maxRetries)
                            {
                                if (itemCompare.ImageSourcePath != null)
                                {
                                    response = await _faceCompareService.CompareFacesAsync(itemCompare.ImageSourcePath, itemCompare.ImageTagetPath);

                                    if (CheckResponseCompare(response))
                                    {
                                        success = true;
                                        var ConfidenceResponse = response.Data.Percentage.HasValue ? (double)response.Data.Percentage.Value : 0.0;
                                        itemCompare.Message = response.Data.Message;
                                        itemCompare.Note = GetNote(confident, ConfidenceResponse);
                                        itemCompare.Status = response.Status + " - " + response.Message;
                                        itemCompare.Confidence = ConfidenceResponse;

                                        int rowIndex = listDataCompare.IndexOf(itemCompare);
                                        dataGridViewImages.Invoke(new Action(() =>
                                        {
                                            if (ConfidenceResponse < confident)
                                            {
                                                dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Red;
                                            }
                                            else
                                            {
                                                dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Green;
                                            }
                                            dataGridViewImages.Refresh();
                                        }));
                                    }
                                    else if (response.Status == 429)
                                    {
                                        retryCount++;
                                        itemCompare.Status = $"Đang retry lần {retryCount}";
                                        itemCompare.Message = response.Data.Message;
                                        itemCompare.Status = response.Status + " - " + response.Message;

                                        int rowIndex = listDataCompare.IndexOf(itemCompare);
                                        dataGridViewImages.Invoke(new Action(() =>
                                        {
                                            dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                                            dataGridViewImages.Refresh();
                                        }));
                                        await Task.Delay(1000);
                                    }
                                    else
                                    {
                                        itemCompare.Status = $"Lỗi!";
                                        itemCompare.Message = response.Data.Message;
                                        itemCompare.Status = response.Status + " - " + response.Message;

                                        int rowIndex = listDataCompare.IndexOf(itemCompare);
                                        dataGridViewImages.Invoke(new Action(() =>
                                        {
                                            dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                                            dataGridViewImages.Refresh();
                                        }));
                                        break;
                                    }
                                }
                                else
                                {
                                    itemCompare.Status = "Comparison successful.";
                                    itemCompare.Message = "Không tìm thấy ảnh source!";
                                    itemCompare.Note = "Cần thêm ảnh source vào hệ thống!";

                                    int rowIndex = listDataCompare.IndexOf(itemCompare);
                                    dataGridViewImages.Invoke(new Action(() =>
                                    {
                                        dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                                        dataGridViewImages.Refresh();
                                    }));
                                    break;
                                }
                            }

                            if (!success && retryCount >= maxRetries)
                            {
                                itemCompare.Status = $"Lỗi, đã retry lại {maxRetries} lần.";
                                itemCompare.Message = response.Data.Message;
                                itemCompare.Status = response.Status + " - " + response.Message;

                                int rowIndex = listDataCompare.IndexOf(itemCompare);
                                dataGridViewImages.Invoke(new Action(() =>
                                {
                                    dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                                    dataGridViewImages.Refresh();
                                }));
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                            // Tăng tiến trình một cách an toàn giữa các thread
                            Interlocked.Increment(ref completedCount);

                            // Cập nhật ProgressBar và phần trăm hoàn thành trên giao diện
                            progressBarCompare.Invoke(new Action(() =>
                            {
                                progressBarCompare.Value = completedCount;
                                lblProgress.Text = $"{(completedCount * 100) / listDataCompare.Count}% hoàn thành";
                            }));

                            // Giải phóng Semaphore để các Task khác có thể chạy
                            semaphore.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                MessageBox.Show("Đã có kết quả!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private double ParseConfidence(string percentage)
        {
            // Loại bỏ ký hiệu '%' nếu có và cắt các khoảng trắng
            string cleanedPercentage = percentage?.Replace("%", "").Trim();

            if (double.TryParse(cleanedPercentage, out double confidence))
            {
                return confidence; // Trả về giá trị nếu chuyển đổi thành công
            }
            else
            {
                // Hiển thị thông báo lỗi nếu chuyển đổi thất bại
                MessageBox.Show("Không thể chuyển đổi giá trị phần trăm thành số thực!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0.0; // Trả về giá trị mặc định
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

        private void ImageCaptureForm_Load(object sender, EventArgs e)
        {
           
            progressBarCompare.Minimum = 0;
        }
    }
}

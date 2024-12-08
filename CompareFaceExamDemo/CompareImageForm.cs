using AuthenExamCompareFace.Dtos;
using AuthenExamCompareFace.ExternalService;
using AuthenExamCompareFace.ExternalService.Recognition;
using AuthenExamCompareFace.Models;
using AuthenExamCompareFace.Utils;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace AuthenExamCompareFace
{
    public partial class CompareImageForm : Form
    {
        private CancellationTokenSource _cancellationTokenSource;
        private CompareFaceAdapterRecognitionService _compareFaceService;
        private readonly FaceCompareService _faceCompareService;
        private readonly object _logLock = new object();
        private BindingSource? source = null;
        private List<ResultCompareFaceDto>? listDataCompare = null;
        private bool _isPaused = false;
        private ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);

        public CompareImageForm(CompareFaceAdapterRecognitionService compareFaceService, FaceCompareService faceCompareService)
        {
            InitializeComponent();
            _compareFaceService = compareFaceService;
            _faceCompareService = faceCompareService;
            dataGridViewImages.ReadOnly = true;
            dataGridViewImages.AllowUserToAddRows = false;
            dataGridViewImages.AllowUserToDeleteRows = false;
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
                    lblProgress.Text = "0%";
                    progressBarCompare.Value = 0;
                    // Lấy đường dẫn của thư mục được chọn
                    string folderPath = folderBrowser.SelectedPath;
                    // Hiển thị đường dẫn trong TextBox
                    txtFolderPath.Text = folderPath;
                    // Load danh sách file từ thư mục được chọn
                    LoadImagesWithCheckbox(folderPath);
                    LoadListData();

                    if (listDataCompare != null && listDataCompare.Count() > 0)
                    {
                        btnSend.Enabled = true;
                    }
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

                int idCounter = 1; // Biến đếm bắt đầu từ 1

                foreach (string file in files)
                {
                    string ImageTagetPath = Path.GetFileName(file);

                    if (regex.IsMatch(ImageTagetPath))
                    {
                        string? ImageSourcePath = GetImageSourcePath(urlSource, ImageTagetPath);

                        ResultCompareFaceDto rcf = new ResultCompareFaceDto
                        {
                            Id = idCounter++, // Gán giá trị hiện tại của idCounter, sau đó tăng lên 1
                            ImageSourcePath = ImageSourcePath,
                            ImageTagetPath = file,
                            StudentCode = Path.GetFileNameWithoutExtension(ImageTagetPath),
                            Status = ResultStatus.PROCESSING,
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

        private string? GetImageSourcePath(string urlSource, string imageFileName)
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
            if (listDataCompare == null || listDataCompare.Count == 0)
            {
                MessageBox.Show("Không có file nào được chọn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Khởi tạo CancellationTokenSource
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            try
            {
                // Cấu hình giao diện
                btnSend.Enabled = false;
                button1.Enabled = true;
                btnSelectFolder.Enabled = false;

                progressBarCompare.Maximum = listDataCompare.Count;
                progressBarCompare.Value = 0;

                var sourceFile = Config.GetSetting();
                int maxDegreeOfParallelism = sourceFile.NumberOfThread;
                int maxRetries = sourceFile.NumberOfRetry;

                // Gọi hàm xử lý so sánh
                await GetCompareResult(maxDegreeOfParallelism, maxRetries, cancellationToken);

                // Kiểm tra nếu bị hủy
                cancellationToken.ThrowIfCancellationRequested();

                // Xử lý xuất kết quả
                string folderPath = txtFolderPath.Text;
                string fileBaseName = DateTime.Now.ToString("ddMMyyyy_HHmmss_fff") + "-" + Path.GetFileName(folderPath);
                //string filePathExcel = Path.Combine(folderPath, fileBaseName + ".xlsx");
                string filePathTxt = Path.Combine(folderPath, fileBaseName + ".txt");

                //ExcelExporter.ExportListToExcel(listDataCompare, filePathExcel);

                var resultCompareFaceTxtDtos = GetResultCompareFaceTxtDto(listDataCompare, maxDegreeOfParallelism, cancellationToken)
                    .OrderBy(r => r.Id)
                    .ToList();
                TxtExporter.ExportListToTxt(resultCompareFaceTxtDtos, filePathTxt, "|");
                dataGridViewImages.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);

                // Hiển thị thông báo thành công
                //MessageBox.Show(
                //    $"Kết quả đã được lưu:\n\nExcel: {filePathExcel}\n\nTxt: {filePathTxt}",
                //    "Thông báo",
                //    MessageBoxButtons.OK,
                //    MessageBoxIcon.Information
                //);

                MessageBox.Show(
                  $"Kết quả đã được lưu:\n\nTxt: {filePathTxt}, Bạn có thể sử dụng kết quả txt ở tab result trong ứng dụng!",
                  "Thông báo",
                  MessageBoxButtons.OK,
                  MessageBoxIcon.Information
              );
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                // Xử lý lỗi chung
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Hoàn trả trạng thái giao diện
                btnSend.Enabled = true;
                btnStop.Enabled = false;
                btnSelectFolder.Enabled = true;
            }
        }


        private List<ResultCompareFaceTxtDto> GetResultCompareFaceTxtDto(List<ResultCompareFaceDto> resultCompareFaceDto, int maxDegreeOfParallelism, CancellationToken cancellationToken)
        {
            List<ResultCompareFaceTxtDto> resultCompareFaceTxtDtos = new List<ResultCompareFaceTxtDto>(resultCompareFaceDto.Count);

            try
            {
                var concurrentList = new ConcurrentBag<ResultCompareFaceTxtDto>();
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    CancellationToken = cancellationToken
                };

                Parallel.ForEach(resultCompareFaceDto, parallelOptions, item =>
                {
                    cancellationToken.ThrowIfCancellationRequested(); // Thoát nếu bị hủy
                    ResultCompareFaceTxtDto rcf = new ResultCompareFaceTxtDto
                    {
                        Id = item.Id,
                        StudentCode = item.StudentCode,
                        Status = GetEnumValueDirect(item.Status),
                        Confidence = item.Confidence,
                        ExamCode = item.ExamCode,
                        Time = item.Time,
                        Message = item.Message,
                        Note = item.Note,
                        ImageSourcePath = item.ImageSourcePath,
                        ImageTagetPath = item.ImageTagetPath,
                    };
                    concurrentList.Add(rcf);
                });

                resultCompareFaceTxtDtos = concurrentList.ToList();

                return resultCompareFaceTxtDtos;
            }
            catch (OperationCanceledException)
            {
                // Xử lý khi bị hủy
                return new List<ResultCompareFaceTxtDto>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private int GetEnumValueDirect(ResultStatus status)
        {
            return (int)status;
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

        public static int GetEnumValue(string input)
        {
            if (Enum.TryParse(input, true, out ResultStatus status))
            {
                return (int)status;
            }
            else
            {
                return -1;
            }
        }

        private async Task GetCompareResult(int maxDegreeOfParallelism, int maxRetries, CancellationToken cancellationToken)
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
                    progressBarCompare.Value = 0;
                }));

                int completedCount = 0;

                foreach (var itemCompare in listDataCompare)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync(cancellationToken); // Tôn trọng token khi chờ Semaphore

                        try
                        {
                            bool success = false;
                            int retryCount = 0;
                            ComparisonResponse? response = null;

                            while (!success && retryCount < maxRetries)
                            {
                                // Tạm dừng nếu ở trạng thái Pause
                                while (_isPaused)
                                {
                                    cancellationToken.ThrowIfCancellationRequested(); // Kiểm tra trạng thái hủy
                                    _pauseEvent.Wait(100); // Đợi 100ms trước khi kiểm tra lại
                                }

                                cancellationToken.ThrowIfCancellationRequested(); // Kiểm tra trạng thái hủy

                                if (itemCompare.ImageSourcePath != null)
                                {
                                    response = await _faceCompareService.CompareFacesAsync(itemCompare.ImageSourcePath, itemCompare.ImageTagetPath ?? "");

                                    // Tạm dừng nếu ở trạng thái Pause
                                    while (_isPaused)
                                    {
                                        cancellationToken.ThrowIfCancellationRequested(); // Kiểm tra trạng thái hủy
                                        _pauseEvent.Wait(100); // Đợi 100ms trước khi kiểm tra lại
                                    }

                                    cancellationToken.ThrowIfCancellationRequested(); // Kiểm tra trạng thái hủy

                                    if (CheckResponseCompare(response))
                                    {
                                        success = true;
                                        var ConfidenceResponse = response.Data.Percentage.HasValue ? (double)response.Data.Percentage.Value : 0.0;
                                        itemCompare.Message = response.Status + " - " + response.Message;
                                        itemCompare.Note = GetNote(confident, ConfidenceResponse);
                                        itemCompare.Status = (double)response.Data.Percentage.Value >= confident ? ResultStatus.MATCHED : ResultStatus.NOTMATCHED;
                                        itemCompare.Confidence = ConfidenceResponse;

                                        int rowIndex = listDataCompare.IndexOf(itemCompare);
                                        dataGridViewImages.Invoke(new Action(() =>
                                        {
                                            if (ConfidenceResponse < confident)
                                            {
                                                dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                                            }
                                            else
                                            {
                                                dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                                            }
                                            dataGridViewImages.Refresh();
                                        }));
                                    }
                                    else if (response.Status == 429) // Retry nếu gặp lỗi 429
                                    {
                                        retryCount++;
                                        itemCompare.Message = $"Đang retry lần {retryCount}";

                                        int rowIndex = listDataCompare.IndexOf(itemCompare);
                                        dataGridViewImages.Invoke(new Action(() =>
                                        {
                                            dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                                            dataGridViewImages.Refresh();
                                        }));
                                        await Task.Delay(1000, cancellationToken); // Tôn trọng token khi trì hoãn
                                    }
                                    else if (response.Status == 413)
                                    {
                                        retryCount++;
                                        itemCompare.Message = response.Status + " - " + response.Message;

                                        int rowIndex = listDataCompare.IndexOf(itemCompare);
                                        dataGridViewImages.Invoke(new Action(() =>
                                        {
                                            dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                                            dataGridViewImages.Refresh();
                                        }));
                                        break;
                                    }
                                    else
                                    {
                                        itemCompare.Message = response.Status + " - " + response.Message;

                                        int rowIndex = listDataCompare.IndexOf(itemCompare);
                                        dataGridViewImages.Invoke(new Action(() =>
                                        {
                                            dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                                            dataGridViewImages.Refresh();
                                        }));
                                        break;
                                    }
                                }
                                else
                                {
                                    itemCompare.Message = "Không tìm thấy ảnh source!";
                                    itemCompare.Note = "Cần thêm ảnh source vào hệ thống!";

                                    int rowIndex = listDataCompare.IndexOf(itemCompare);
                                    dataGridViewImages.Invoke(new Action(() =>
                                    {
                                        dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                                        dataGridViewImages.Refresh();
                                    }));
                                    break;
                                }
                            }

                            if (!success && retryCount >= maxRetries)
                            {
                                itemCompare.Message = response?.Status + " - " + response?.Message;

                                int rowIndex = listDataCompare.IndexOf(itemCompare);
                                dataGridViewImages.Invoke(new Action(() =>
                                {
                                    dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                                    dataGridViewImages.Refresh();
                                }));
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // Đánh dấu trạng thái bị hủy
                            int rowIndex = listDataCompare.IndexOf(itemCompare);
                            dataGridViewImages.Invoke(new Action(() =>
                            {
                                dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightGray;
                                dataGridViewImages.Refresh();
                            }));
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                            Interlocked.Increment(ref completedCount);

                            // Cập nhật ProgressBar và phần trăm hoàn thành trên giao diện
                            progressBarCompare.Invoke(new Action(() =>
                            {
                                progressBarCompare.Value = completedCount;
                                lblProgress.Text = $"{(completedCount * 100) / listDataCompare.Count}% hoàn thành";
                            }));

                            semaphore.Release(); // Giải phóng Semaphore
                        }
                    }, cancellationToken)); // Tôn trọng token
                }


                await Task.WhenAll(tasks); // Đợi tất cả Task hoàn thành
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Quá trình so sánh đã bị hủy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void button1_Click(object sender, EventArgs e)
        {
            _isPaused = true;
            btnStop.Enabled = true;
            _pauseEvent.Reset();
            button1.Enabled = false;
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _isPaused = false;
            btnStop.Enabled = false;
            _pauseEvent.Set();
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _isPaused = false;
            _pauseEvent.Set();
            button1.Enabled = false;
            button2.Enabled = false;

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel(); // Kích hoạt hủy
            }
        }
    }
}

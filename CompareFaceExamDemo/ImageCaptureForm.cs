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
using System.Data.Common;
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

        public ImageCaptureForm(CompareFaceAdapterRecognitionService compareFaceService, FaceCompareService faceCompareService)
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
                dataGridViewImages.RowHeadersVisible = false;
                dataGridViewImages.AutoGenerateColumns = false; // Tắt tự động tạo cột

                AddColumn();

                // Liên kết dữ liệu
                source.DataSource = listDataCompare;
                dataGridViewImages.DataSource = source;
            }
            catch (Exception)
            {
                MessageBox.Show($"Liên hệ admin", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddColumn()
        {
            // Định nghĩa các cột cố định
            dataGridViewImages.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id", // Liên kết với thuộc tính trong ResultCompareFaceDto
                Width = 50
            });

            dataGridViewImages.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ExamCode",
                HeaderText = "Mã Bài Thi",
                DataPropertyName = "ExamCode",
                Width = 170
            });

            dataGridViewImages.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "StudentCode",
                HeaderText = "Mã Sinh Viên",
                DataPropertyName = "StudentCode",
                Width = 100
            });

            dataGridViewImages.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Time",
                HeaderText = "Thời Gian",
                DataPropertyName = "Time",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle() { Format = "dd/MM/yyyy HH:mm:ss" } // Format datetime
            });

            dataGridViewImages.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Status",
                HeaderText = "Trạng Thái",
                DataPropertyName = "Status",
                Width = 100
            });

            dataGridViewImages.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Message",
                HeaderText = "Thông Báo",
                DataPropertyName = "Message",
                Width = 265
            });

            dataGridViewImages.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Confidence",
                HeaderText = "Độ Tin Cậy",
                DataPropertyName = "Confidence",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle() { Format = "N2" } // Hiển thị 2 chữ số thập phân
            });

            dataGridViewImages.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Note",
                HeaderText = "Ghi Chú",
                DataPropertyName = "Note",
                Width = 250
            });

            dataGridViewImages.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ImageTagetPath",
                HeaderText = "Ảnh Đích",
                DataPropertyName = "ImageTagetPath",
                Width = 400
            });

            dataGridViewImages.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ImageSourcePath",
                HeaderText = "Ảnh Nguồn",
                DataPropertyName = "ImageSourcePath",
                Width = 350
            });
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
            try
            {
                if (listDataCompare!.Count > 0)
                {
                    try
                    {
                        progressBarCompare.Maximum = listDataCompare.Count;
                        var sourceFile = Config.GetSetting();
                        int maxDegreeOfParallelism = sourceFile.NumberOfThread;
                        int maxRetries = sourceFile.NumberOfRetry;
                        await GetCompareResult(maxDegreeOfParallelism, maxRetries);

                        string folderPath = txtFolderPath.Text;
                        string[] folderPathParts = folderPath.Split('\\');
                        string lastPart = folderPathParts[folderPathParts.Length - 1];

                        string filePathExcel = folderPath + "/" + DateTime.Now.ToString("ddMMyyyy_HHmmss_fff") + "-" + lastPart + ".xlsx";
                        string filePathTxt = folderPath + "/" + DateTime.Now.ToString("ddMMyyyy_HHmmss_fff") + "-" + lastPart + ".txt";

                        ExcelExporter.ExportListToExcel(listDataCompare, filePathExcel);

                        var resultCompareFaceTxtDtos = GetResultCompareFaceTxtDto(listDataCompare, maxDegreeOfParallelism);
                        resultCompareFaceTxtDtos = resultCompareFaceTxtDtos.OrderBy(r => r.Id).ToList();
                        TxtExporter.ExportListToTxt(resultCompareFaceTxtDtos, filePathTxt, "|");

                        if (listDataCompare.Count > 0)
                        {
                            string message = $"Kết quả đã được lưu:\n\nExcel: {filePathExcel}\n\nTxt: {filePathTxt}";

                            MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
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

        private List<ResultCompareFaceTxtDto> GetResultCompareFaceTxtDto(List<ResultCompareFaceDto> resultCompareFaceDto, int maxDegreeOfParallelism)
        {
            List<ResultCompareFaceTxtDto> resultCompareFaceTxtDtos = new List<ResultCompareFaceTxtDto>(resultCompareFaceDto.Count);

            try
            {
                // Sử dụng một đối tượng thread-safe để thêm phần tử song song
                var concurrentList = new ConcurrentBag<ResultCompareFaceTxtDto>();

                // Tạo ParallelOptions để giới hạn số luồng
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism // Số luồng tối đa, tùy chỉnh theo CPU
                };

                Parallel.ForEach(resultCompareFaceDto, parallelOptions, item =>
                {
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

                // Chuyển ConcurrentBag về List
                resultCompareFaceTxtDtos = concurrentList.ToList();

                return resultCompareFaceTxtDtos;
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
                    progressBarCompare.Value = 0;
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
                                    response = await _faceCompareService.CompareFacesAsync(itemCompare.ImageSourcePath, itemCompare.ImageTagetPath ?? "");

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
                                    else if (response.Status == 429)
                                    {
                                        retryCount++;
                                        itemCompare.Message = $"Đang retry lần {retryCount}";

                                        int rowIndex = listDataCompare.IndexOf(itemCompare);
                                        dataGridViewImages.Invoke(new Action(() =>
                                        {
                                            dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                                            dataGridViewImages.Refresh();
                                        }));
                                        await Task.Delay(1000);
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
                                itemCompare.Message = response.Status + " - " + response.Message;

                                int rowIndex = listDataCompare.IndexOf(itemCompare);
                                dataGridViewImages.Invoke(new Action(() =>
                                {
                                    dataGridViewImages.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
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
    }
}

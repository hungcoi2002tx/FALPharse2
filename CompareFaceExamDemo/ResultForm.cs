using CompareFaceExamDemo.DAO;
using CompareFaceExamDemo.Entities;
using CompareFaceExamDemo.Models;
using CompareFaceExamDemo.Utils;

namespace CompareFaceExamDemo
{

    public partial class ResultForm : Form
    {
        private ExamDAO<EOSComparisonResult> examDao;
        private List<EOSComparisonResult> results = [];
        private readonly string _sourceImageFolder;
        private SettingModel _settingForm;
        private string fileDataPath;

        public ResultForm()
        {
            InitializeComponent();

            try
            {
                // Tải cấu hình từ settings.json
                _settingForm = Config.GetSetting();
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"Không tìm thấy file cấu hình: {ex.Message}", "Lỗi cấu hình", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1); // Thoát chương trình nếu không tìm thấy file cấu hình
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Thiếu thông tin cần thiết trong cấu hình: {ex.Message}", "Lỗi cấu hình", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1); // Thoát chương trình nếu thiếu thông tin quan trọng
            }
            catch (DirectoryNotFoundException ex)
            {
                MessageBox.Show($"Thư mục cấu hình không tồn tại: {ex.Message}", "Lỗi cấu hình", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1); // Thoát chương trình nếu thư mục cấu hình không tồn tại
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi không xác định khi tải cấu hình: {ex.Message}", "Lỗi cấu hình", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1); // Thoát chương trình nếu có lỗi khác
            }
            _sourceImageFolder = _settingForm.DirectoryImageSource; // Folder của SourceImage
                                                                    // Folder chứa dữ liệu TargetImage



            // Khởi tạo giá trị cho cmbStatus
            cmbStatus.Items.AddRange(new object[]
            {
            "PROCESSING",
            "MATCHED",
            "NOTMATCHED"
            });

            // Khởi tạo giá trị cho cmbSortField
            cmbSortField.Items.AddRange(new object[]
            {
            "StudentCode",
            "Time",
            "Status",
            "Confidence"
            });

            // Chọn giá trị mặc định
            cmbStatus.SelectedIndex = -1;
            cmbSortField.SelectedIndex = -1;

            // Đăng ký sự kiện để hiển thị ảnh khi chọn hàng mới
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // Lấy tên file được chọn từ ComboBox
            var selectedFile = cmbFileList.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedFile))
            {
                MessageBox.Show("Please select a file from the list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kết hợp đường dẫn thư mục với tên file
            fileDataPath = Path.Combine(txtDataFolder.Text, selectedFile.EndsWith(".txt") ? selectedFile : selectedFile + ".txt");


            // Kiểm tra file tồn tại
            if (!File.Exists(fileDataPath))
            {
                MessageBox.Show($"File {fileDataPath} does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Lấy các điều kiện lọc từ giao diện người dùng
            var filters = GetFilters(); // Lấy các bộ lọc từ các trường giao diện

            // Lấy danh sách kết quả từ file và áp dụng bộ lọc
            results = GetFilteredResults(fileDataPath, filters);

            // Cập nhật DataGridView với kết quả
            dataGridView1.DataSource = results;
        }


        private Dictionary<string, object> GetFilters()
        {
            var filters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(txtStudentCode.Text))
                filters.Add("StudentCode", txtStudentCode.Text);

            if (!string.IsNullOrEmpty(txtExamCode.Text))
                filters.Add("ExamCode", txtExamCode.Text);

            if (!string.IsNullOrEmpty(cmbStatus.Text))
                filters.Add("Status", Enum.Parse(typeof(ResultStatus), cmbStatus.Text));

            if (double.TryParse(txtMinConfidence.Value.ToString(), out double minConfidence))
                filters.Add("MinConfidence", minConfidence);

            if (double.TryParse(txtMaxConfidence.Value.ToString(), out double maxConfidence))
                filters.Add("MaxConfidence", maxConfidence);

            return filters;
        }

        private List<EOSComparisonResult> GetFilteredResults(string examCode, Dictionary<string, object> filters)
        {
            // Đọc toàn bộ dữ liệu từ file theo ExamCode
            var results = examDao.GetAll(examCode);

            // Lọc dữ liệu theo các điều kiện
            if (filters.ContainsKey("StudentCode"))
                results = results.Where(r => r.StudentCode == (string)filters["StudentCode"]).ToList();

            if (filters.ContainsKey("ExamCode"))
                results = results.Where(r => r.ExamCode == (string)filters["ExamCode"]).ToList();

            if (filters.ContainsKey("Status"))
                results = results.Where(r => r.Status == (ResultStatus)filters["Status"]).ToList();

            if (filters.ContainsKey("MinConfidence"))
                results = results.Where(r => r.Confidence >= (double)filters["MinConfidence"]).ToList();

            if (filters.ContainsKey("MaxConfidence"))
                results = results.Where(r => r.Confidence <= (double)filters["MaxConfidence"]).ToList();

            // Sắp xếp kết quả theo trường và thứ tự được chọn
            if (!string.IsNullOrEmpty(cmbSortField.Text))
            {
                var sortDescending = chkSortDesc.Checked;

                results = cmbSortField.Text switch
                {
                    "StudentCode" => sortDescending
                        ? results.OrderByDescending(r => r.StudentCode).ToList()
                        : results.OrderBy(r => r.StudentCode).ToList(),
                    "Confidence" => sortDescending
                        ? results.OrderByDescending(r => r.Confidence).ToList()
                        : results.OrderBy(r => r.Confidence).ToList(),
                    "Time" => sortDescending
                        ? results.OrderByDescending(r => r.Time).ToList()
                        : results.OrderBy(r => r.Time).ToList(),
                    _ => results // Mặc định không sắp xếp nếu trường không hợp lệ
                };
            }

            return results;
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedResult = (EOSComparisonResult)dataGridView1.SelectedRows[0].DataBoundItem;
                DisplayImages(selectedResult);
            }
        }

        private void DisplayImages(EOSComparisonResult result)
        {
            var sourceImagePath = Path.Combine(_sourceImageFolder, $"{result.StudentCode}.jpg");
            var targetImagePath = Path.Combine(txtDataFolder.Text, $"{result.ExamCode}", $"{result.StudentCode}.jpg");

            // Hiển thị SourceImage
            if (File.Exists(sourceImagePath))
            {
                // Giải phóng ảnh hiện tại nếu có
                if (pictureBoxSourceImage.Image != null)
                {
                    pictureBoxSourceImage.Image.Dispose();
                    pictureBoxSourceImage.Image = null;
                }

                // Tải ảnh mới
                pictureBoxSourceImage.Image = Image.FromFile(sourceImagePath);
            }
            else
            {
                // Gán giá trị null nếu không tìm thấy ảnh
                if (pictureBoxSourceImage.Image != null)
                {
                    pictureBoxSourceImage.Image.Dispose();
                    pictureBoxSourceImage.Image = null;
                }
            }

            // Hiển thị TargetImage
            if (File.Exists(targetImagePath))
            {
                // Giải phóng ảnh hiện tại nếu có
                if (pictureBoxTargetImage.Image != null)
                {
                    pictureBoxTargetImage.Image.Dispose();
                    pictureBoxTargetImage.Image = null;
                }

                // Tải ảnh mới
                pictureBoxTargetImage.Image = Image.FromFile(targetImagePath);
            }
            else
            {
                // Gán giá trị null nếu không tìm thấy ảnh
                if (pictureBoxTargetImage.Image != null)
                {
                    pictureBoxTargetImage.Image.Dispose();
                    pictureBoxTargetImage.Image = null;
                }
            }
        }


        private void Main_Load(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Tạo một instance của form SourceImageForm
            var sourceImageForm = new SourceImageForm();

            // Hiển thị form SourceImageForm dưới dạng hộp thoại (modal dialog)
            sourceImageForm.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Select the folder containing data files";
                folderBrowserDialog.ShowNewFolderButton = false;

                // Hiển thị hộp thoại chọn thư mục
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    // Gán đường dẫn thư mục vào txtDataFolder
                    txtDataFolder.Text = folderBrowserDialog.SelectedPath;
                    examDao = new ExamDAO<EOSComparisonResult>(folderBrowserDialog.SelectedPath);

                    // Lấy danh sách file trong thư mục và đưa vào ComboBox
                    var files = Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.txt");
                    cmbFileList.Items.Clear(); // Xóa các mục cũ
                    cmbFileList.Items.AddRange(files.Select(file => Path.GetFileNameWithoutExtension(file)!).ToArray());
                    // Thêm tên file vào ComboBox

                    if (cmbFileList.Items.Count > 0)
                    {
                        cmbFileList.SelectedIndex = 0; // Chọn file đầu tiên trong danh sách
                    }
                    // Sử dụng đường dẫn từ file cấu hình
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            // Tạo SaveFileDialog để cho người dùng chọn file lưu
            using (var saveFileDialog = new SaveFileDialog())
            {
                // Thiết lập filter để chỉ chọn file Excel
                saveFileDialog.Filter = "Excel Files|*.xlsx";
                saveFileDialog.Title = "Lưu File Excel";

                string defaultFileName = $"output-{cmbFileList.Text}-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.xlsx";
                saveFileDialog.FileName = defaultFileName;
                // Hiển thị hộp thoại và kiểm tra nếu người dùng chọn file
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Lấy đường dẫn file đã chọn
                    string filePath = saveFileDialog.FileName;

                    try
                    {

                        // Gọi phương thức Export để xuất danh sách ra file Excel
                        ExcelExporter.ExportListToExcel(results, filePath);

                        // Thông báo thành công
                        MessageBox.Show("Dữ liệu đã được xuất thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        // Nếu có lỗi, hiển thị thông báo lỗi
                        MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

    }
}
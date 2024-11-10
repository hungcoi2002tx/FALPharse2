using CompareFaceExamDemo.DAO;
using CompareFaceExamDemo.Dtos;
using CompareFaceExamDemo.Entities;
using CompareFaceExamDemo.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompareFaceExamDemo
{

    public partial class Main : Form
    {
        private readonly ExamDAO<EOSComparisonResult> _examDao;
        private readonly string _sourceImageFolder;
        private readonly string _dataFolder;
        private readonly Config _config;

        public Main()
        {
            InitializeComponent();

            try
            {
                // Tải cấu hình từ settings.json
                _config = Config.LoadConfig();
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
            _sourceImageFolder = _config.SourceImageDirectory!; // Folder của SourceImage
            _dataFolder = _config.DataDirectory!; // Folder chứa dữ liệu TargetImage

            // Sử dụng đường dẫn từ file cấu hình
            _examDao = new ExamDAO<EOSComparisonResult>(_dataFolder);

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
            // Lấy các điều kiện lọc từ giao diện người dùng
            var examDate = dtpExamDate.Value.ToString("yyyy-MM-dd"); // Ngày thi
            var shift = int.Parse(txtShift.Value.ToString()); // Ca thi
            var filters = GetFilters(); // Lấy các bộ lọc từ các trường giao diện

            // Lấy danh sách kết quả từ file và áp dụng bộ lọc
            var results = GetFilteredResults(examDate, shift, filters);

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

        private List<EOSComparisonResult> GetFilteredResults(string examDate, int shift, Dictionary<string, object> filters)
        {
            // Đọc toàn bộ dữ liệu từ file cho đợt thi và ca thi cụ thể
            var results = _examDao.GetAll(examDate, shift);

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
            var examDate = dtpExamDate.Value.ToString("yyyy-MM-dd"); // Ngày thi
            var shift = int.Parse(txtShift.Value.ToString()); // Ca thi
            var sourceImagePath = Path.Combine(_sourceImageFolder, $"{result.StudentCode}.jpg"); // Hoặc .png nếu cần
            var targetImagePath = Path.Combine(_dataFolder, examDate, $"shift{shift}", "Images", $"{result.StudentCode}.jpg");

            // Hiển thị SourceImage
            if (File.Exists(sourceImagePath))
            {
                pictureBoxSourceImage.Image = Image.FromFile(sourceImagePath);
            }
            else
            {
                pictureBoxSourceImage.Image = null;
            }

            // Hiển thị TargetImage
            if (File.Exists(targetImagePath))
            {
                pictureBoxTargetImage.Image = Image.FromFile(targetImagePath);
            }
            else
            {
                pictureBoxTargetImage.Image = null;
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            dtpExamDate.Value = DateTime.Now;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Tạo một instance của form SourceImageForm
            var sourceImageForm = new SourceImageForm();

            // Hiển thị form SourceImageForm dưới dạng hộp thoại (modal dialog)
            sourceImageForm.ShowDialog();
        }
    }
}
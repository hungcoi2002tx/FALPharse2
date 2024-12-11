using AuthenExamCompareFaceExam.DAO;
using AuthenExamCompareFaceExam.Entities;
using AuthenExamCompareFaceExam.Models;
using AuthenExamCompareFaceExam.Utils;
using System.Data.Common;

namespace AuthenExamCompareFaceExam
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

            cmbFileList.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSortField.DropDownStyle = ComboBoxStyle.DropDownList;

            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            //dataGridView1.AutoGenerateColumns = false; // Tắt tự động tạo cột
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
                "ALL",
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
            var selectedFile = cmbFileList.SelectedItem as string;



            if (string.IsNullOrEmpty(selectedFile))
            {
                MessageBox.Show("Please select a file from the list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            fileDataPath = Path.Combine(txtDataFolder.Text, selectedFile.EndsWith(".txt") ? selectedFile : selectedFile + ".txt");

            if (!File.Exists(fileDataPath))
            {
                MessageBox.Show($"File {fileDataPath} does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var filters = GetFilters(); // Lấy các bộ lọc từ các trường giao diện
            results = GetFilteredResults(fileDataPath, filters);
            CheckConfidenceSetting();
            //AddColumn();
            // Cập nhật DataGridView
            dataGridView1.DataSource = results;
            // Đăng ký sự kiện RowPrePaint để định màu sắc
            dataGridView1.RowPrePaint += DataGridView1_RowPrePaint;

            if (results.Count <= 0)
            {
                MessageBox.Show($"File {fileDataPath} is blank!", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (pictureBoxSourceImage.Image != null)
                {
                    pictureBoxSourceImage.Image.Dispose();
                    pictureBoxSourceImage.Image = null;
                }

                if (pictureBoxTargetImage.Image != null)
                {
                    pictureBoxTargetImage.Image.Dispose();
                    pictureBoxTargetImage.Image = null;
                }

                return;
            }
        }

        private void CheckConfidenceSetting()
        {
            var sourceFile = Config.GetSetting();
            var confidence = sourceFile.Confident;

            foreach (var item in results)
            {
                if (item.Confidence == 0.0 && item.Status == ResultStatus.PROCESSING)
                {
                    item.Status = ResultStatus.PROCESSING;
                }
                else if (item.Confidence >= confidence)
                {
                    item.Status = ResultStatus.MATCHED;
                    item.Note = "khuôn mặt giống nhau";
                }
                else
                {
                    item.Status = ResultStatus.NOTMATCHED;
                    item.Note = "khuôn mặt KHÁC nhau";
                }
            }
        }
        private void DataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].DataBoundItem is EOSComparisonResult result)
            {
                // Thay đổi màu nền của dòng dựa trên ResultStatus
                switch (result.Status)
                {
                    case ResultStatus.PROCESSING:
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow; // Màu vàng nhạt
                        break;

                    case ResultStatus.MATCHED:
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen; // Màu xanh nhạt
                        break;

                    case ResultStatus.NOTMATCHED:
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightCoral; // Màu đỏ nhạt
                        break;

                    default:
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White; // Màu trắng mặc định
                        break;
                }
            }
        }


        private Dictionary<string, object> GetFilters()
        {
            var filters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(txtStudentCode.Text))
                filters.Add("StudentCode", txtStudentCode.Text.ToUpper());

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

            if (filters.ContainsKey("Status") && (ResultStatus)filters["Status"] != ResultStatus.ALL)
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
            var sourceImagePath = result.ImageSourcePath;
            var targetImagePath = result.ImageTagetPath;

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

        private void AddColumn()
        {
            // Định nghĩa các cột cố định
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id", // Liên kết với thuộc tính trong ResultCompareFaceDto
                Width = 50
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ExamCode",
                HeaderText = "Mã Bài Thi",
                DataPropertyName = "ExamCode",
                Width = 170
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "StudentCode",
                HeaderText = "Mã Sinh Viên",
                DataPropertyName = "StudentCode",
                Width = 100
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Time",
                HeaderText = "Thời Gian",
                DataPropertyName = "Time",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle() { Format = "dd/MM/yyyy HH:mm:ss" } // Format datetime
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Status",
                HeaderText = "Trạng Thái",
                DataPropertyName = "Status",
                Width = 100
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Message",
                HeaderText = "Thông Báo",
                DataPropertyName = "Message",
                Width = 265
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Confidence",
                HeaderText = "Độ Tin Cậy",
                DataPropertyName = "Confidence",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle() { Format = "N2" } // Hiển thị 2 chữ số thập phân
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Note",
                HeaderText = "Ghi Chú",
                DataPropertyName = "Note",
                Width = 250
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ImageTagetPath",
                HeaderText = "Ảnh Đích",
                DataPropertyName = "ImageTagetPath",
                Width = 400
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ImageSourcePath",
                HeaderText = "Ảnh Nguồn",
                DataPropertyName = "ImageSourcePath",
                Width = 350
            });
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
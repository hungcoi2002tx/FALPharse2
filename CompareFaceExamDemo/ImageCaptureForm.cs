using CompareFaceExamDemo.ExternalService.Recognition;
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
    public partial class ImageCaptureForm : Form
    {
        private CompareFaceAdapterRecognitionService _compareFaceService;
        public ImageCaptureForm(CompareFaceAdapterRecognitionService compareFaceService)
        {
            InitializeComponent();
            _compareFaceService = compareFaceService;
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
                Regex regex = new Regex(@"^[a-zA-Z]{2}\d{6}\.jpg$");

                // Dọn sạch DataGridView
                dataGridViewImages.Columns.Clear();
                dataGridViewImages.Rows.Clear();

                // Thêm cột checkbox
                DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn
                {
                    HeaderText = "",
                    Name = "checkBoxColumn",
                    Width = 50
                };
                dataGridViewImages.Columns.Add(checkBoxColumn);
                DataGridViewTextBoxColumn fileNameColumn = new DataGridViewTextBoxColumn
                {
                    HeaderText = "File Name",
                    Name = "FileName",
                    ReadOnly = true // Không cho phép sửa tên file
                };
                // Thêm cột tên file
                dataGridViewImages.Columns.Add(fileNameColumn);

                // Thêm dữ liệu vào DataGridView
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);

                    // Kiểm tra tên file có đúng định dạng không
                    if (regex.IsMatch(fileName))
                    {
                        dataGridViewImages.Rows.Add(false, fileName); // False mặc định checkbox không tích
                    }
                }

                // Hiển thị thông báo nếu không tìm thấy file phù hợp
                if (dataGridViewImages.Rows.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy file nào đáp ứng định dạng trong thư mục!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                AddSelectAllCheckBox();
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

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                // Tạo danh sách chứa đường dẫn các file đã được chọn
                List<string> selectedFiles = new List<string>();
                List<string> sourceFiles = new List<string>();

                foreach (DataGridViewRow row in dataGridViewImages.Rows)
                {
                    // Kiểm tra nếu checkbox được tích
                    bool isChecked = Convert.ToBoolean(row.Cells["checkBoxColumn"].Value);
                    if (isChecked)
                    {
                        // Lấy tên file từ cột FileName
                        string fileName = row.Cells["FileName"].Value.ToString();

                        // Kết hợp với đường dẫn thư mục để tạo đường dẫn đầy đủ
                        string folderPath = txtFolderPath.Text; // Đường dẫn thư mục được chọn
                        string fullPath = Path.Combine(folderPath, fileName);

                        // Thêm đường dẫn đầy đủ vào danh sách
                        selectedFiles.Add(fullPath);
                    }
                }

                var sourceFile = Config.GetSetting();
                var urlSource = sourceFile.DirectoryImageSource;
                // Kết hợp với thư mục nguồn để tạo đường dẫn đầy đủ
                foreach (DataGridViewRow row in dataGridViewImages.Rows)
                {
                    // Kiểm tra nếu checkbox được tích
                    bool isChecked = Convert.ToBoolean(row.Cells["checkBoxColumn"].Value);
                    if (isChecked)
                    {
                        // Lấy tên file từ cột FileName
                        string fileName = row.Cells["FileName"].Value.ToString();

                        // Kết hợp với đường dẫn thư mục để tạo đường dẫn đầy đủ
                        string folderPath = txtFolderPath.Text; // Đường dẫn thư mục được chọn
                        string fullPath = Path.Combine(urlSource, fileName);

                        // Thêm đường dẫn đầy đủ vào danh sách
                        if (File.Exists(fullPath))
                        {
                            sourceFiles.Add(fullPath);
                        }
                        else
                        {
                            MessageBox.Show($"File không tồn tại trong thư mục nguồn: {fileName}", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }

                // Hiển thị danh sách file đã được chọn (nếu cần)
                if (selectedFiles.Count > 0)
                {
                    string message = "Các file được chọn:\n" + string.Join("\n", selectedFiles);
                    MessageBox.Show(message, "File Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}

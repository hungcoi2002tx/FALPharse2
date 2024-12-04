using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AuthenExamCompareFaceExam
{
    public partial class BulkDeleteForm : Form
    {
        public string StudentCodes { get; private set; }  // Lưu trữ dữ liệu mã sinh viên nhập vào

        public BulkDeleteForm()
        {
            InitializeComponent();
            this.Text = "Nhập mã sinh viên để xóa";
            txtStudentCodes.ScrollBars = ScrollBars.Vertical; // Thêm thanh cuộn dọc
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // Lấy giá trị mã sinh viên từ TextBox
            StudentCodes = txtStudentCodes.Text.Trim();
            this.DialogResult = DialogResult.OK;  // Đóng form và trả về DialogResult.OK
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;  // Đóng form nếu chọn "Cancel"
            this.Close();
        }
    }

}

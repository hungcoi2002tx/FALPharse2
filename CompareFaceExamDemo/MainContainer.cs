using AuthenExamCompareFace.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AuthenExamCompareFace
{
    public partial class MainContainer : Form
    {
        private CompareImageForm compareImageForm;
        private SourceImageForm imageSourceForm;
        private SettingForm settingForm;
        private ResultForm mainForm;

        public MainContainer(CompareImageForm compareImageForm,
            SourceImageForm imageSourceForm,
            SettingForm settingForm,
            ResultForm mainForm)
        {
            InitializeComponent();
            // Cố định chiều rộng và chiều cao của form
            //this.Size = new Size(1920, 1080); // Thay đổi kích thước theo ý muốn

            // Đặt kiểu viền cố định
            //  this.FormBorderStyle = FormBorderStyle.FixedSingle;

            // Tắt nút phóng to
            //this.MaximizeBox = false;

            // Cho phép thu nhỏ (nếu cần)
            //this.MinimizeBox = true;

            // Tắt khả năng thay đổi kích thước
            //this.MaximumSize = this.Size;
            //this.MinimumSize = this.Size;
            this.compareImageForm = compareImageForm;
            this.imageSourceForm = imageSourceForm;
            this.settingForm = settingForm;
            this.mainForm = mainForm;
            compareImageForm.BringToFront();
        }

        private void ImageSourceButtonClick(object sender, EventArgs e)
        {
            try
            {
                OpenChildForm(imageSourceForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ImageCaptureButtonClick(object sender, EventArgs e)
        {
            try
            {
                OpenChildForm(compareImageForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SettingButtonClick(object sender, EventArgs e)
        {
            try
            {
                OpenChildForm(settingForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OpenChildForm(Form childForm)
        {
            try
            {
                childForm.FormBorderStyle = FormBorderStyle.None;
                childForm.MdiParent = this; // Thiết lập MDI Parent
                childForm.Dock = DockStyle.Fill; // Dock vào form cha
                childForm.Show();
                childForm.BringToFront();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void ViewResultButtonClick(object sender, EventArgs e)
        {
            try
            {
                OpenChildForm(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExitButtonClick(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Bạn có chắc muốn thoát ứng dụng?", "Thoát ứng dụng", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region NOT DELETE THESE METHOD

        private void viewImageCaptureToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void viewResultToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        #endregion


        private async void MainContainer_Load(object sender, EventArgs e)
        {
            try
            {
                await CheckAndHandleLicenseKeyAsync();
                OpenChildForm(compareImageForm); // Mở form con nếu giấy phép hợp lệ
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi kiểm tra quyền.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit(); // Đóng ứng dụng khi xảy ra lỗi nghiêm trọng
            }
        }

        private async Task CheckAndHandleLicenseKeyAsync()
        {
            string licenseKey = LicenseKeyGenerator.GetLicenseKey();
            var isValidLicenseKey = await LicenseChecker.CheckLicenseKeyAsync(licenseKey);

            if (!isValidLicenseKey)
            {
                if (!string.IsNullOrEmpty(licenseKey))
                {
                    Clipboard.SetText(licenseKey);
                }
                else
                {
                    MessageBox.Show("Không có nội dung để sao chép.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                MessageBox.Show(
                    $"Chưa được cấp quyền sử dụng.\nKey của bạn: {licenseKey}\nĐã copy key vào clipboard, gửi key cho admin để sử dụng.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                // Tắt ứng dụng nếu không hợp lệ
                Application.Exit();
            }
        }
    }
}

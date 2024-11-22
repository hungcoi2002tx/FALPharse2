using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompareFaceExamDemo
{
    public partial class MainContainer : Form
    {
        private ImageCaptureForm imageCaptureForm;
        private ImageSourceForm imageSourceForm;
        private SettingForm settingForm;
        private Main mainForm;
        public MainContainer()
        {
            InitializeComponent();
            // Cố định chiều rộng và chiều cao của form
            this.Size = new Size(1200, 800); // Thay đổi kích thước theo ý muốn

            // Đặt kiểu viền cố định
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            // Tắt nút phóng to
            this.MaximizeBox = false;

            // Cho phép thu nhỏ (nếu cần)
            this.MinimizeBox = true;

            // Tắt khả năng thay đổi kích thước
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
        }

        private void ImageSourceButtonClick(object sender, EventArgs e)
        {
            try
            {
                OpenChildForm(new ImageSourceForm());
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
                OpenChildForm(new ImageCaptureForm());
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
                OpenChildForm(new SettingForm());
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
                // Kiểm tra xem form đã được khởi tạo hay chưa
                if (childForm is ImageSourceForm)
                {
                    if (imageSourceForm == null || imageSourceForm.IsDisposed)
                    {
                        imageSourceForm = new ImageSourceForm();
                        imageSourceForm.FormBorderStyle = FormBorderStyle.None;
                        imageSourceForm.MdiParent = this; // Thiết lập MDI Parent
                        imageSourceForm.Dock = DockStyle.Fill; // Dock vào form cha
                        imageSourceForm.Show(); // Hiển thị form
                    }
                    else
                    {
                        imageSourceForm.BringToFront(); // Mang form đã mở lên phía trước
                    }
                }
                else if (childForm is ImageCaptureForm)
                {
                    if (imageCaptureForm == null || imageCaptureForm.IsDisposed)
                    {
                        imageCaptureForm = new ImageCaptureForm();
                        imageCaptureForm.FormBorderStyle = FormBorderStyle.None;
                        imageCaptureForm.MdiParent = this; // Thiết lập MDI Parent
                        imageCaptureForm.Dock = DockStyle.Fill; // Dock vào form cha
                        imageCaptureForm.Show(); // Hiển thị form
                    }
                    else
                    {
                        imageCaptureForm.BringToFront(); // Mang form đã mở lên phía trước
                    }
                }
                else if (childForm is SettingForm)
                {
                    if (settingForm == null || settingForm.IsDisposed)
                    {
                        settingForm = new SettingForm();
                        settingForm.FormBorderStyle = FormBorderStyle.None;
                        settingForm.MdiParent = this; // Thiết lập MDI Parent
                        settingForm.Dock = DockStyle.Fill; // Dock vào form cha
                        settingForm.Show(); // Hiển thị form
                    }
                    else
                    {
                        settingForm.BringToFront(); // Mang form đã mở lên phía trước
                    }
                }
                else if (childForm is Main)
                {
                    if (mainForm == null || mainForm.IsDisposed)
                    {
                        mainForm = new Main();
                        mainForm.FormBorderStyle = FormBorderStyle.None;
                        mainForm.MdiParent = this; // Thiết lập MDI Parent
                        mainForm.Dock = DockStyle.Fill; // Dock vào form cha
                        mainForm.Show(); // Hiển thị form
                    }
                    else
                    {
                        mainForm.BringToFront(); // Mang form đã mở lên phía trước
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private void ViewResultButtonClick(object sender, EventArgs e)
        {
            try
            {
                OpenChildForm(new Main());
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
        private void viewImageSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void viewImageCaptureToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void viewResultToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void settingToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        #endregion


    }
}

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
        public MainContainer(ImageCaptureForm imageCaptureForm,
            ImageSourceForm imageSourceForm, 
            SettingForm settingForm, 
            Main mainForm)
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
            this.imageCaptureForm = imageCaptureForm;
            this.imageSourceForm = imageSourceForm;
            this.settingForm = settingForm;
            this.mainForm = mainForm;
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
                OpenChildForm(imageCaptureForm);
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


    }
}

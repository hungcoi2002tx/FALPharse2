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
        public MainContainer()
        {
            InitializeComponent();
        }

        private void viewImageSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ImageSourceForm sourceForm = new ImageSourceForm();
                sourceForm.MdiParent = this;
                sourceForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private void viewImageCaptureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ImageCaptureForm captureForm = new ImageCaptureForm();
                captureForm.MdiParent = this;
                captureForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private void viewResultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void settingToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {

                throw;
            }
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
                childForm.MdiParent = this;
                childForm.Show();
            }
            catch (Exception ex)
            {
                throw;
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
    }
}

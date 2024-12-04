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
    public partial class Test : Form
    {
        public Test()
        {
            InitializeComponent();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Select the folder to save files";
                folderBrowserDialog.ShowNewFolderButton = true;

                // Hiển thị hộp thoại chọn thư mục
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    // Gán đường dẫn thư mục vào TextBox
                    textBoxDirectory.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Generating code for the directory: {textBoxDirectory.Text}");
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

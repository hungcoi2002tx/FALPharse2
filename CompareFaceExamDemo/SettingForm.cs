using CompareFaceExamDemo.Models;
using CompareFaceExamDemo.Utils;
using System;
using System.Text.Json;
using System.Windows.Forms;

namespace CompareFaceExamDemo
{
    public partial class SettingForm : Form
    {
        private SettingModel _settingModel;
        public SettingForm()
        {
            InitializeComponent();
            LoadSettings();
            BindingTextBox();
        }

        private void BindingTextBox()
        {
            try
            {
                numberConfident.Value = _settingModel.Confident;
                numberThread.Value = _settingModel.NumberOfThread;
                txtDirectorySource.Text = _settingModel.DirectoryImageSource;
                txtDiretoryCapture.Text = _settingModel.DirectoryImageCapture;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadSettings()
        {
            try
            {
                _settingModel = Config.GetSetting();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnGenerateCode_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Generating code for all tables...");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Btn_Browse_Capture_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.Description = "Select the folder to save files";
                    folderBrowserDialog.ShowNewFolderButton = true;

                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Set the selected path to the textbox
                        txtDiretoryCapture.Text = folderBrowserDialog.SelectedPath;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_Browse_Source_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.Description = "Select the folder to save files";
                    folderBrowserDialog.ShowNewFolderButton = true;

                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Set the selected path to the textbox
                        txtDirectorySource.Text = folderBrowserDialog.SelectedPath;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region NOT DELETE
        private void labelDirectory_Click(object sender, EventArgs e)
        {

        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {

        }

        private void textBoxDirectorySource_TextChanged(object sender, EventArgs e)
        {

        }
        #endregion

        private void Btn_Save_Click(object sender, EventArgs e)
        {
            try
            {
                GetData();
                string jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

                // Chuyển đổi đối tượng thành chuỗi JSON
                string jsonContent = JsonSerializer.Serialize(_settingModel, new JsonSerializerOptions
                {
                    WriteIndented = true // Tùy chọn format JSON cho đẹp (dạng thụt dòng)
                });

                // Ghi nội dung JSON vào file
                File.WriteAllText(jsonFilePath, jsonContent);

                MessageBox.Show("AppSettings has been saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void GetData()
        {
            try
            {
                _settingModel.Confident = Convert.ToInt32(numberConfident.Value);
                _settingModel.NumberOfThread = Convert.ToInt32(numberThread.Value);
                _settingModel.DirectoryImageSource = txtDirectorySource.Text;
                _settingModel.DirectoryImageCapture = txtDiretoryCapture.Text;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

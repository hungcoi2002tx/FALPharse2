using AuthenExamCompareFace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AuthenExamCompareFace.Utils
{
    public class Config
    {
        public string? DataDirectory { get; set; }
        public string SourceImageDirectory { get; set; } = string.Empty;

        public static Config LoadConfig()
        {
            var configPath = "appsettings.json";
            var configContent = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<Config>(configContent);

            // Nếu DataDirectory là null hoặc rỗng, thiết lập đường dẫn mặc định
            if (string.IsNullOrEmpty(config?.DataDirectory))
            {
                var defaultDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

                // Tạo thư mục mặc định nếu chưa tồn tại
                if (!Directory.Exists(defaultDirectory))
                {
                    Directory.CreateDirectory(defaultDirectory);
                }

                config!.DataDirectory = defaultDirectory;
            }
            else
            {
                // Nếu DataDirectory có giá trị nhưng thư mục chưa tồn tại, tạo thư mục đó
                if (!Directory.Exists(config.DataDirectory))
                {
                    Directory.CreateDirectory(config.DataDirectory);
                }
            }

            // Kiểm tra SourceImageDirectory, nếu không có thì báo lỗi
            if (string.IsNullOrEmpty(config?.SourceImageDirectory))
            {
                throw new InvalidOperationException("Cấu hình 'SourceImageDirectory' không được tìm thấy trong 'settings.json'.");
            }
            else if (!Directory.Exists(config.SourceImageDirectory))
            {
                throw new DirectoryNotFoundException($"Thư mục 'SourceImageDirectory' được chỉ định ({config.SourceImageDirectory}) không tồn tại.");
            }

            return config;
        }

        public static SettingModel GetSetting()
        {
            try
            {
                SettingModel _settingModel = new SettingModel();
                // Đường dẫn đến file appsettings.json
                string jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

                if (File.Exists(jsonFilePath))
                {
                    // Đọc nội dung file JSON
                    string jsonContent = File.ReadAllText(jsonFilePath);

                    // Chuyển đổi JSON thành đối tượng AppSettings
                    _settingModel = JsonSerializer.Deserialize<SettingModel>(jsonContent);
                }
                else
                {
                    MessageBox.Show("File appsettings.json không tồn tại!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return _settingModel;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void ValidateAndCreateDirectories(SettingModel settingModel)
        {
            // Đường dẫn mặc định
            string baseFolder = Path.Combine("C:\\", "EOSFPTUFR");
            string imageSourceFolder = Path.Combine(baseFolder, "ImageSource");
            string imageCaptureFolder = Path.Combine(baseFolder, "ImageCapture");

            bool needToUpdateJson = false;

            // Kiểm tra và tạo thư mục DirectoryImageSource
            if (string.IsNullOrEmpty(settingModel.DirectoryImageSource) || !Directory.Exists(settingModel.DirectoryImageSource))
            {
                Directory.CreateDirectory(imageSourceFolder); // Tạo thư mục mặc định
                settingModel.DirectoryImageSource = imageSourceFolder; // Cập nhật đường dẫn
                needToUpdateJson = true;
            }

            // Nếu cần cập nhật JSON, ghi lại file
            if (needToUpdateJson)
            {
                SaveSetting(settingModel);
            }
        }

        private static void SaveSetting(SettingModel settingModel)
        {
            try
            {
                string jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

                // Chuyển đổi đối tượng SettingModel thành JSON
                string jsonContent = JsonSerializer.Serialize(settingModel, new JsonSerializerOptions
                {
                    WriteIndented = true // Format JSON đẹp hơn
                });

                // Ghi nội dung JSON vào file
                File.WriteAllText(jsonFilePath, jsonContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu cài đặt: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

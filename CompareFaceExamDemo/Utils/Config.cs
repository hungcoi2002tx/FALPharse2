using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CompareFaceExamDemo.Utils
{
    internal class Config
    {
        public string? DataDirectory { get; set; }
        public string SourceImageDirectory { get; set; } = string.Empty;

        public static Config LoadConfig()
        {
            var configPath = "settings.json";
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
    }
}

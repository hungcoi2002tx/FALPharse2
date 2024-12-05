using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenExamCompareFace.Utils
{
    public class TxtExporter
    {
        public static void ExportListToTxt<T>(List<T> list, string filePath, string delimiter = ",")
        {
            if (list == null || !list.Any())
            {
                MessageBox.Show("Danh sách dữ liệu rỗng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Thoát khỏi hàm để tránh lỗi tiếp theo
            }

            // Lấy thông tin các thuộc tính của class
            var properties = typeof(T).GetProperties();

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var item in list)
                {
                    var values = properties.Select(p => p.GetValue(item, null)?.ToString() ?? string.Empty);
                    writer.WriteLine(string.Join(delimiter, values));
                }
            }
        }
    }
}

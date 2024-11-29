using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareFaceExamDemo.Utils
{
    public class TxtExporter
    {
        public static void ExportListToTxt<T>(List<T> list, string filePath, string delimiter = ",")
        {
            if (list == null || !list.Any())
                throw new InvalidOperationException("Danh sách trống hoặc null.");

            // Lấy thông tin các thuộc tính của class
            var properties = typeof(T).GetProperties();

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Ghi dòng tiêu đề (header)
                writer.WriteLine(string.Join(delimiter, properties.Select(p => p.Name)));

                // Ghi từng dòng dữ liệu
                foreach (var item in list)
                {
                    var values = properties.Select(p => p.GetValue(item, null)?.ToString() ?? string.Empty);
                    writer.WriteLine(string.Join(delimiter, values));
                }
            }
        }

    }
}

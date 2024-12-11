using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenExamCompareFaceExam.Utils
{
    public class ExcelExporter
    {
        /// <summary>
        /// Xuất danh sách đối tượng sang file Excel.
        /// </summary>
        /// <typeparam name="T">Loại đối tượng trong danh sách.</typeparam>
        /// <param name="data">Danh sách cần xuất.</param>
        /// <param name="filePath">Đường dẫn file Excel cần lưu.</param>
        /// <param name="sheetName">Tên của worksheet (mặc định là Sheet1).</param>
        public static void ExportListToExcel<T>(List<T> data, string filePath, string sheetName = "Sheet1")
        {
            if (data == null || data.Count == 0)
            {
                MessageBox.Show("Danh sách dữ liệu rỗng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Thoát khỏi hàm để tránh lỗi tiếp theo
            }
             
            using (var package = new ExcelPackage())
            {
                // Tạo worksheet
                var worksheet = package.Workbook.Worksheets.Add(sheetName);

                // Lấy thông tin thuộc tính của đối tượng T
                var properties = typeof(T).GetProperties();

                // Ghi tiêu đề (header)
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[1, col + 1].Value = properties[col].Name;
                    worksheet.Cells[1, col + 1].Style.Font.Bold = true; // Làm đậm tiêu đề
                }

                // Ghi dữ liệu
                for (int row = 0; row < data.Count; row++)
                {
                    for (int col = 0; col < properties.Length; col++)
                    {
                        worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(data[row]);
                    }
                }

                // Lưu file
                var fileInfo = new FileInfo(filePath);
                package.SaveAs(fileInfo);
            }
        }
    }
}
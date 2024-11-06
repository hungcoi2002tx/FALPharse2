using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompareFaceExamDemo
{
    public partial class Form1 : Form
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Tạo URL API với các bộ lọc từ giao diện người dùng
            var url = BuildApiUrl();

            // Gọi API và cập nhật danh sách kết quả
            var results = await GetResultsFromApi(url);

            // Cập nhật DataGridView với kết quả
            dataGridView1.DataSource = results;
        }

        private string BuildApiUrl()
        {
            var baseUrl = "https://localhost:7031/api/Results";
            var queryParams = new List<string>();

            // Thêm các tham số lọc (nếu có)
            if (!string.IsNullOrEmpty(txtStudentCode.Text))
            {
                queryParams.Add($"studentCode={txtStudentCode.Text}");
            }
            if (!string.IsNullOrEmpty(txtExamCode.Text))
            {
                queryParams.Add($"studentCode={txtExamCode.Text}");
            }

            if (!string.IsNullOrEmpty(cmbStatus.Text))
            {
                queryParams.Add($"status={cmbStatus.Text}");
            }

            if (dtpStartDate.Checked)
            {
                queryParams.Add($"startDate={dtpStartDate.Value:yyyy-MM-dd}");
            }

            if (dtpEndDate.Checked)
            {
                queryParams.Add($"endDate={dtpEndDate.Value:yyyy-MM-dd}");
            }

            //if (double.TryParse(txtMinConfidence.Text, out double minConfidence))
            //{
            //    queryParams.Add($"minConfidence={minConfidence}");
            //}

            //if (double.TryParse(txtMaxConfidence.Text, out double maxConfidence))
            //{
            //    queryParams.Add($"maxConfidence={maxConfidence}");
            //}

            if (!string.IsNullOrEmpty(cmbSortField.Text))
            {
                queryParams.Add($"sortField={cmbSortField.Text}");
                queryParams.Add($"sortDesc={chkSortDesc.Checked}");
            }

            // Kết hợp các query parameters thành một chuỗi
            return queryParams.Count > 0 ? $"{baseUrl}?{string.Join("&", queryParams)}" : baseUrl;
        }

        private async Task<List<ResultCompareFaceDto>> GetResultsFromApi(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ResultCompareFaceDto>>(responseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}");
                return new List<ResultCompareFaceDto>();
            }
        }
    }

    public class ResultCompareFaceDto
    {
        public string? StudentCode { get; set; }
        public string? Status { get; set; }
        public double Confidence { get; set; }
        public string? ExamCode { get; set; }
        public DateTime Time { get; set; }
        public string? Note { get; set; }
        public string? Message { get; set; }
    }
}
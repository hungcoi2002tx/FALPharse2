using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using AuthenExamCompareFaceExam.Utils;

namespace AuthenExam.UnitTests
{
    [TestFixture]
    public class ExcelExporterTests
    {
        private string _testDirectory;
        private string _testFilePath;

        [SetUp]
        public void Setup()
        {
            // Thiết lập thư mục tạm để lưu file Excel test
            _testDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestFiles");
            if (!Directory.Exists(_testDirectory))
            {
                Directory.CreateDirectory(_testDirectory);
            }

            _testFilePath = Path.Combine(_testDirectory, "test.xlsx");
        }

        [TearDown]
        public void Teardown()
        {
            // Xóa file sau khi test
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [Test]
        public void ExportListToExcel_ShouldCreateExcelFileWithCorrectData()
        {
            // Arrange
            var testData = new List<TestData>
            {
                new TestData { Id = 1, Name = "Alice", Age = 25 },
                new TestData { Id = 2, Name = "Bob", Age = 30 }
            };

            // Act
            ExcelExporter.ExportListToExcel(testData, _testFilePath);

            // Assert
            Assert.IsTrue(File.Exists(_testFilePath), "Expected file was not created.");

            // Mở file Excel để kiểm tra nội dung
            using (var package = new ExcelPackage(new FileInfo(_testFilePath)))
            {
                var worksheet = package.Workbook.Worksheets["Sheet1"];
                Assert.NotNull(worksheet, "Worksheet was not created.");

                // Kiểm tra tiêu đề
                Assert.AreEqual("Id", worksheet.Cells[1, 1].Value.ToString());
                Assert.AreEqual("Name", worksheet.Cells[1, 2].Value.ToString());
                Assert.AreEqual("Age", worksheet.Cells[1, 3].Value.ToString());

                // Kiểm tra dữ liệu
                Assert.AreEqual(1, Convert.ToInt32(worksheet.Cells[2, 1].Value));
                Assert.AreEqual("Alice", worksheet.Cells[2, 2].Value.ToString());
                Assert.AreEqual(25, Convert.ToInt32(worksheet.Cells[2, 3].Value));

                Assert.AreEqual(2, Convert.ToInt32(worksheet.Cells[3, 1].Value));
                Assert.AreEqual("Bob", worksheet.Cells[3, 2].Value.ToString());
                Assert.AreEqual(30, Convert.ToInt32(worksheet.Cells[3, 3].Value));
            }
        }

        [Test]
        public void ExportListToExcel_ShouldHandleEmptyDataGracefully()
        {
            // Arrange
            var testData = new List<TestData>();

            // Act
            ExcelExporter.ExportListToExcel(testData, _testFilePath);

            // Assert
            Assert.IsFalse(File.Exists(_testFilePath), "File should not be created for empty data.");
        }

        [Test]
        public void ExportListToExcel_ShouldThrowExceptionForInvalidFilePath()
        {
            // Arrange
            var testData = new List<TestData>
            {
                new TestData { Id = 1, Name = "Alice", Age = 25 }
            };
            var invalidFilePath = "Z:\\InvalidPath\\test.xlsx";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ExcelExporter.ExportListToExcel(testData, invalidFilePath)
            );
            Assert.IsNotNull(exception, "Expected an IOException for invalid file path.");
        }

        // Class mẫu cho test
        private class TestData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}

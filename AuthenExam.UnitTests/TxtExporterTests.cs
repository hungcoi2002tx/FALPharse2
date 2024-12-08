using NUnit.Framework;
using AuthenExamCompareFaceExam.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AuthenExamCompareFaceExam;

namespace AuthenExam.UnitTests
{
    [TestFixture]
    public class TxtExporterTests
    {
        private string _testFilePath;

        [SetUp]
        public void SetUp()
        {
            // Đặt đường dẫn file tạm thời cho test
            _testFilePath = Path.Combine(Path.GetTempPath(), "test_export.txt");
        }

        [TearDown]
        public void TearDown()
        {
            // Xóa file test nếu tồn tại
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [Test]
        public void ExportListToTxt_NullList_ShowsWarningMessage()
        {
            // Arrange
            List<object> nullList = null;

            // Act & Assert
            Assert.DoesNotThrow(() => TxtExporter.ExportListToTxt(nullList, _testFilePath));
            Assert.IsFalse(File.Exists(_testFilePath), "File should not be created for null list.");
        }

        [Test]
        public void ExportListToTxt_EmptyList_ShowsWarningMessage()
        {
            // Arrange
            var emptyList = new List<object>();

            // Act
            TxtExporter.ExportListToTxt(emptyList, _testFilePath);

            // Assert
            Assert.IsFalse(File.Exists(_testFilePath), "File should not be created for empty list.");
        }

        [Test]
        public void ExportListToTxt_ValidList_CreatesFileWithCorrectContent()
        {
            // Arrange
            var testList = new List<TestData>
            {
                new TestData { Id = 1, Name = "Alice" },
                new TestData { Id = 2, Name = "Bob" }
            };
            var expectedContent = "1,Alice\r\n2,Bob\r\n";

            // Act
            TxtExporter.ExportListToTxt(testList, _testFilePath);

            // Assert
            Assert.IsTrue(File.Exists(_testFilePath), "File should be created for valid list.");
            var actualContent = File.ReadAllText(_testFilePath);
            Assert.AreEqual(expectedContent, actualContent, "File content should match expected content.");
        }

        [Test]
        public void ExportListToTxt_InvalidFilePath_ThrowsException()
        {
            // Arrange
            var testList = new List<TestData>
            {
                new TestData { Id = 1, Name = "Alice" }
            };
            var invalidPath = "Z:\\InvalidPath\\test.txt";

            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(() =>
                TxtExporter.ExportListToTxt(testList, invalidPath), "Expected exception for invalid file path.");
        }

        private class TestData
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}

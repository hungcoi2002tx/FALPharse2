using AuthenExamCompareFaceExam.DAO;
using AuthenExamCompareFaceExam.Entity.Interface;
using NUnit.Framework;
using System;
using System.IO;

namespace AuthenExam.UnitTests
{
    [TestFixture]
    public class ExamDAOTests
    {
        private string _testDirectory;
        private ExamDAO<MockEntity> _examDAO;
        private string _examCode;
        private string _testFilePath;

        [SetUp]
        public void Setup()
        {
            // Tạo thư mục tạm thời
            _testDirectory = Path.Combine(Path.GetTempPath(), "ExamDAO_Test");
            if (!Directory.Exists(_testDirectory))
            {
                Directory.CreateDirectory(_testDirectory);
            }

            // Khởi tạo đối tượng ExamDAO
            _examCode = "TestExam";
            _examDAO = new ExamDAO<MockEntity>(_testDirectory);

            // Đường dẫn file .txt
            _testFilePath = Path.Combine(_testDirectory, $"{_examCode}.txt");

            // Tạo file dữ liệu mẫu trước khi test
            using (var writer = new StreamWriter(_testFilePath, append: false))
            {
                writer.WriteLine("1|Existing Entity 1");
                writer.WriteLine("2|Existing Entity 2");
            }
        }

        [TearDown]
        public void Cleanup()
        {
            // Xóa file test sau khi test xong
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }

            // Xóa thư mục nếu không còn file
            if (Directory.Exists(_testDirectory) && Directory.GetFiles(_testDirectory).Length == 0)
            {
                Directory.Delete(_testDirectory);
            }
        }

        [Test]
        public void Add_ShouldAddEntityToFile()
        {
            // Arrange
            var entity = new MockEntity { Id = 3, Name = "Test Name 3" };

            // Act: Thêm entity vào file
            _examDAO.Add(_testFilePath, entity);

            // Assert: Kiểm tra file tồn tại và chứa entity mới
            var lines = File.ReadAllLines(_testFilePath);
            Assert.IsTrue(lines.Length == 3, "File should have 3 lines after adding an entity.");
            Assert.AreEqual("3|Test Name 3", lines[2], "Added entity does not match expected format.");
        }

        [Test]
        public void GetAll_ShouldReturnCorrectEntities()
        {
            // Act: Lấy danh sách entities từ file
            var entities = _examDAO.GetAll(_testFilePath);

            // Assert: Kiểm tra số lượng và nội dung của các entities
            Assert.AreEqual(2, entities.Count, "Initial file should contain 2 entities.");
            Assert.AreEqual(1, entities[0].Id, "Entity 1 ID does not match.");
            Assert.AreEqual("Existing Entity 1", entities[0].Name, "Entity 1 Name does not match.");
        }

        [Test]
        public void Delete_ShouldRemoveEntityFromFile()
        {
            // Act: Xóa entity có ID = 1
            _examDAO.Delete(_testFilePath, 1);

            // Assert: Kiểm tra entity đã bị xóa
            var entities = _examDAO.GetAll(_testFilePath);
            Assert.AreEqual(1, entities.Count, "File should have 1 entity after deletion.");
            Assert.AreEqual(2, entities[0].Id, "Remaining entity ID does not match.");
        }

        [Test]
        public void Update_ShouldModifyEntityInFile()
        {
            // Arrange
            var updatedEntity = new MockEntity { Id = 1, Name = "Updated Entity Name" };

            // Act: Cập nhật entity trong file
            _examDAO.Update(_testFilePath, updatedEntity);

            // Assert: Kiểm tra entity đã được cập nhật
            var entities = _examDAO.GetAll(_testFilePath);
            Assert.AreEqual(2, entities.Count, "File should still have 2 entities after update.");
            Assert.AreEqual("Updated Entity Name", entities[0].Name, "Entity 1 Name was not updated.");
        }
    }

    // Mock Entity class for testing purposes
    public class MockEntity : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public void FromText(string text)
        {
            var parts = text.Split('|');
            Id = int.Parse(parts[0]);
            Name = parts[1];
        }

        public string ToText()
        {
            return $"{Id}|{Name}";
        }

        public int GetId()
        {
            return Id;
        }
    }
}

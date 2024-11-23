using CompareFaceExamDemo.Entity.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CompareFaceExamDemo.DAO
{
    public class ExamDAO<T> where T : IEntity, new()
    {
        private readonly string _baseDirectory;

        public ExamDAO(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        // Tạo đường dẫn đến file cho từng đợt thi và ca thi
        private string GetFilePath(string examDate, int shift)
        {
            string directory = Path.Combine(_baseDirectory, examDate, $"shift{shift}");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);  // Tạo thư mục nếu chưa tồn tại
            }
            return Path.Combine(directory, "exam.txt");
        }

        // Đọc toàn bộ đối tượng từ file theo đợt thi và ca thi
        public List<T> GetAll(string examDate, int shift)
        {
            var entities = new List<T>();
            var filePath = GetFilePath(examDate, shift);

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var entity = new T();
                        entity.FromText(line);
                        entities.Add(entity);
                    }
                }
            }

            return entities;
        }

        // Thêm đối tượng mới vào file theo đợt thi và ca thi
        public void Add(string examDate, int shift, T entity)
        {
            var filePath = GetFilePath(examDate, shift);

            using (var writer = new StreamWriter(filePath, append: true))
            {
                writer.WriteLine(entity.ToText());
            }
        }

        // Cập nhật đối tượng trong file theo đợt thi và ca thi
        public void Update(string examDate, int shift, T entity)
        {
            var entities = GetAll(examDate, shift);
            var index = entities.FindIndex(e => e.GetId() == entity.GetId());

            if (index >= 0)
            {
                entities[index] = entity;
                SaveAll(examDate, shift, entities);
            }
        }

        // Xóa đối tượng khỏi file theo đợt thi và ca thi
        public void Delete(string examDate, int shift, int id)
        {
            var entities = GetAll(examDate, shift);
            entities.RemoveAll(e => e.GetId() == id);
            SaveAll(examDate, shift, entities);
        } 

        // Lưu toàn bộ danh sách đối tượng vào file theo đợt thi và ca thi
        private void SaveAll(string examDate, int shift, List<T> entities)
        {
            var filePath = GetFilePath(examDate, shift);

            using (var writer = new StreamWriter(filePath, append: false))
            {
                foreach (var entity in entities)
                {
                    writer.WriteLine(entity.ToText());
                }
            }
        }
    }
}

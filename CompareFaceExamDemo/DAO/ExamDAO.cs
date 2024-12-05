using AuthenExamCompareFace.Entity.Interface;
using System;
using System.Collections.Generic;
using System.IO;

namespace AuthenExamCompareFace.DAO
{
    public class ExamDAO<T> where T : IEntity, new()
    {
        private readonly string _baseDirectory;

        public ExamDAO(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        // Tạo đường dẫn đến file theo ExamCode
        private string GetFilePath(string examCode)
        {
            // Tạo file có dạng ExamCode.txt
            //string filePath = Path.Combine(_baseDirectory, $"{examCode}.txt");
            return examCode;
        }

        // Đọc toàn bộ đối tượng từ file theo ExamCode
        public List<T> GetAll(string examCode)
        {
            var entities = new List<T>();
            var filePath = GetFilePath(examCode);

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

        // Thêm đối tượng mới vào file theo ExamCode
        public void Add(string examCode, T entity)
        {
            var filePath = GetFilePath(examCode);

            using (var writer = new StreamWriter(filePath, append: true))
            {
                writer.WriteLine(entity.ToText());
            }
        }

        // Cập nhật đối tượng trong file theo ExamCode
        public void Update(string examCode, T entity)
        {
            var entities = GetAll(examCode);
            var index = entities.FindIndex(e => e.GetId() == entity.GetId());

            if (index >= 0)
            {
                entities[index] = entity;
                SaveAll(examCode, entities);
            }
        }

        // Xóa đối tượng khỏi file theo ExamCode
        public void Delete(string examCode, int id)
        {
            var entities = GetAll(examCode);
            entities.RemoveAll(e => e.GetId() == id);
            SaveAll(examCode, entities);
        }

        // Lưu toàn bộ danh sách đối tượng vào file theo ExamCode
        private void SaveAll(string examCode, List<T> entities)
        {
            var filePath = GetFilePath(examCode);

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

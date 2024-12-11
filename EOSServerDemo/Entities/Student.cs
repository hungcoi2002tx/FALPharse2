using AuthenExamCompareFaceExam.Entity.Interface;
using System;

namespace AuthenExamCompareFaceExam.Entities
{
    public partial class Student : IEntity
    {
        public string StudentCode { get; set; } = null!;
        public string ImagePath { get; set; } = null!;

        // Chuyển đối tượng thành chuỗi văn bản để lưu vào file
        public string ToText()
        {
            return $"{StudentCode}|{ImagePath}";
        }

        // Chuyển chuỗi văn bản từ file thành đối tượng
        public void FromText(string text)
        {
            var parts = text.Split('|');

            if (parts.Length < 2)
                throw new FormatException("Invalid text format for Student");

            StudentCode = parts[0];
            ImagePath = parts[1];
        }

        // Trả về mã duy nhất của đối tượng
        public int GetId()
        {
            // Giả định rằng StudentCode là mã định danh duy nhất.
            return StudentCode.GetHashCode();
        }
    }
}

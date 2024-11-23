using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareFaceExamDemo.Entity.Interface
{
    public interface IEntity
    {
        string ToText();             // Chuyển đối tượng thành chuỗi để lưu trữ
        void FromText(string text);  // Chuyển chuỗi thành đối tượng
        int GetId();                 // Lấy ID của đối tượng
    }
}

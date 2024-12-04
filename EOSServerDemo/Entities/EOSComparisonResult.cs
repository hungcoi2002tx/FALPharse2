using AuthenExamCompareFaceExam.Entity.Interface;
using System;
using System.Collections.Generic;

namespace AuthenExamCompareFaceExam.Entities;
public partial class EOSComparisonResult : IEntity
{
    public int Id { get; set; }
    public string StudentCode { get; set; } = null!;
    public DateTime Time { get; set; }
    public ResultStatus Status { get; set; }
    public double Confidence { get; set; }
    public string? ExamCode { get; set; }
    public string? Note { get; set; }
    public string? Message { get; set; }

    // Chuyển đối tượng thành chuỗi văn bản để lưu vào file
    public string ToText()
    {
        return $"{Id}|{StudentCode}|{Time:O}|{(int)Status}|{Confidence}|{ExamCode}|{Note}|{Message}";
    }

    // Chuyển chuỗi văn bản từ file thành đối tượng
    public void FromText(string text)
    {
        var parts = text.Split('|');

        if (parts.Length < 7)
            throw new FormatException("Invalid text format for EOSComparisonResult");

        Id = int.Parse(parts[0]);
        StudentCode = parts[1];
        Time = DateTime.Parse(parts[2], null, System.Globalization.DateTimeStyles.RoundtripKind);
        Status = (ResultStatus)int.Parse(parts[3]);
        Confidence = double.Parse(parts[4]);
        ExamCode = parts[5];
        Note = parts[6];
        Message = parts.Length > 7 ? parts[7] : null;
    }

    public int GetId() => Id;
}


public enum ResultStatus
{
    PROCESSING,  // Đang xử lý
    MATCHED,     // Khớp
    NOTMATCHED   // Không khớp
}

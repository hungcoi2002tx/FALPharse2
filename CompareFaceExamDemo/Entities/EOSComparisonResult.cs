using AuthenExamCompareFace.Entity.Interface;
using System;
using System.Collections.Generic;

namespace AuthenExamCompareFace.Entities;
public partial class EOSComparisonResult : IEntity
{
    public int Id { get; set; }
    public string? ExamCode { get; set; }
    public string? StudentCode { get; set; }
    public DateTime Time { get; set; } = DateTime.Now;
    public ResultStatus Status { get; set; }
    public string? Message { get; set; }
    public double Confidence { get; set; } = 0;
    public string? Note { get; set; }
    public string? ImageTagetPath { get; set; } = null;
    public string? ImageSourcePath { get; set; } = null;

    // Chuyển đối tượng thành chuỗi văn bản để lưu vào file
    public string ToText()
    {
        return $"{Id}|{StudentCode}|{Time:O}|{(int)Status}|{Confidence}|{ExamCode}|{Note}|{Message}|{ImageTagetPath}|{ImageSourcePath}";
    }

    // Chuyển chuỗi văn bản từ file thành đối tượng
    public void FromText(string text)
    {
        var parts = text.Split('|');

        if (parts.Length < 9)
            throw new FormatException("Invalid text format for EOSComparisonResult");

        Id = int.Parse(parts[0]);
        ExamCode = parts[1];
        StudentCode = parts[2];
        Time = DateTime.Parse(parts[3], null, System.Globalization.DateTimeStyles.RoundtripKind);
        Status = (ResultStatus)int.Parse(parts[4]);
        Message =  parts[5];
        Confidence = double.Parse(parts[6]);
        Note = parts[7];
        ImageTagetPath = parts[8];
        ImageSourcePath = parts.Length > 9 ? parts[9] : null;
    }

    public int GetId() => Id;
}


public enum ResultStatus
{
  
    PROCESSING,  // Đang xử lý
    MATCHED,     // Khớp
    NOTMATCHED,   // Không khớp
          ALL
}

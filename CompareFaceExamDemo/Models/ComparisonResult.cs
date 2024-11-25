namespace CompareFaceExamDemo.Models
{
    public class ComparisonResult
    {
        public string? SourceImageUrl { get; set; }
        public string? TargetImageUrl { get; set; }
        public float? Similarity { get; set; }
        public int? ResultId { get; set; }
    }
}

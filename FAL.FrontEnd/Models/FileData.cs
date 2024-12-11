namespace FAL.FrontEnd.Models
{
    public class FileData
    {
        public string FileName { get; set; }
        public List<Face> RegisteredFaces { get; set; }
        public List<Face> UnregisteredFaces { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Key { get; set; }
    }

}

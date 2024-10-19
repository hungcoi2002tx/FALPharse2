namespace FAL.Models
{
    public class Permission
    {
        public string Resource { get; set; }
        public List<string> Actions { get; set; }
    }
}

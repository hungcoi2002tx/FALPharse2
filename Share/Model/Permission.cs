namespace Share.Model
{
    public class Permission
    {
        public string Resource { get; set; }
        public List<string> Actions { get; set; }
    }
}

namespace Share.DTO
{
    public class GroupedRequestData
    {
        public string RequestType { get; set; }
        public List<ClientRequest> Requests { get; set; } = new List<ClientRequest>();
    }
}
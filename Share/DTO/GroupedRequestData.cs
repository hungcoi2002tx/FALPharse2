namespace Share.DTO
{
    public class GroupedRequestData
    {
        public string RequestType { get; set; }
        public List<ClientRequest> Requests { get; set; } = new List<ClientRequest>();
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
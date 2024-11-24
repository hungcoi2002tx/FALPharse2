using System.Net;

namespace Demo_Eos_Api.DTOs
{
    public class ResponseResult
    {
        public bool? Success { get; set; }
        /// <summary>
        /// API data 
        /// </summary>
        public object? Data { get; set; }
        /// <summary>
        /// api status Code
        /// </summary>
        public HttpStatusCode? StatusCode { get; set; }
        /// <summary>
        /// Developer message
        /// </summary>
        public string? DevMsg { get; set; }
        /// <summary>
        /// user message
        /// </summary>
        public string? UserMsg { get; set; }
    }
}

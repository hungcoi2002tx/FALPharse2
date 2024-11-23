using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareFaceExamDemo.ExternalService.Recognition
{
    public class CompareFaceAdapterRecognitionService : BaseRecognitionServices<CompareFaceAdapterRecognitionService>
    {
        public CompareFaceAdapterRecognitionService(IMemoryCache memoryCache, 
            IRecognitionRestClient client) : 
            base(memoryCache, client)
        {
        }

        public async Task TestAsync()
        {
            try
            {
                var request = new RestRequest("/");
                request.Method = Method.Put;
                request.AddQueryParameter("employee_identification_key", "abc");
                var response = await ExecuteWithHandlerAsync<LoginResultModel>(_client.Instance(), request);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}

using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenExamCompareFaceExam.ExternalService.Recognition
{
    public class RecognitionRestClient : IRecognitionRestClient, IDisposable
    {
        readonly RestClient _client;
        public RecognitionRestClient(RestClient client)
        {
            _client = client;
        }

        public RestClient Instance() => _client;

        public void Dispose()
        {
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

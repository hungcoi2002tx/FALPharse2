using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenExamCompareFace.ExternalService.Recognition
{
    public interface IRecognitionRestClient
    {
        RestClient Instance();
    }
}

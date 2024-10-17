using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.Utils
{
    public class JwtService
    {
        public async Task<HttpRequestMessage> CreateAuthenticatedRequestAsync(string url, HttpMethod method, string jwtToken)
        {
            var request = new HttpRequestMessage(method, url);

            if (!string.IsNullOrEmpty(jwtToken))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
            }

            return request;
        }
    }
}

using AuthenExamCompareFaceExam.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AuthenExamCompareFaceExam.ExternalService.Recognition
{
    public class BaseRecognitionServices<T> where T : class
    {
        private readonly IMemoryCache _memoryCache;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private JsonSerializerOptions jsonSerializerOptions;

        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        protected readonly IRecognitionRestClient _client;
        protected string UserName { get; init; }
        protected string Password { get; init; }
        public BaseRecognitionServices(
            IMemoryCache memoryCache,
            IRecognitionRestClient client)
        {
            _client = client;
            _memoryCache = memoryCache;
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            UserName = "string";
            Password = EncryptionHelper.Decrypt("L4srREdu8JqEJ23JI65+xQ==", GlobalVarians.PrivateKey); ;
            jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<string> GetAuthTokenAsync(bool forceNew = false)
        {
            if (forceNew)
            {
                _memoryCache.Remove(GlobalVarians.Token);
            }
            var token = _memoryCache.Get(GlobalVarians.Token) as string;
            if (token != null)
            {
                return token;
            }

            await semaphoreSlim.WaitAsync();

            try
            {
                token = _memoryCache.Get(GlobalVarians.Token) as string;

                if (token != null)
                {
                    return token;
                }

                token = await SignInAsync();
                if (token.IsNotNullOrEmpty())
                {
                    var jwtSecurityToken = _jwtSecurityTokenHandler.ReadJwtToken(token);
                    var expirationTime = jwtSecurityToken.ValidTo.AddMinutes(-5).ToLocalTime();
                    _memoryCache.Set(GlobalVarians.Token, token, expirationTime);
                }
                return token;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<string> SignInAsync()
        {
            try
            {
                LoginRequestModel loginModel = new LoginRequestModel
                {
                    username = UserName,
                    password = Password
                };
                var request = new RestRequest("/api/Auth/login").AddJsonBody(loginModel);
                request.Method = Method.Post;
                var response = await _client.Instance().ExecuteAsync<LoginResultModel>(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return response.Data.token;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        public void RemoveAuthToken()
        {
            _memoryCache.Remove(GlobalVarians.Token);
        }

        protected async Task<RestResponse<TModel>> ExecuteWithHandlerAsync<TModel>(RestClient client, RestRequest request, bool rejectIfFailed = false)
        {
            try
            {
                var token = await GetAuthTokenAsync();
                request.AddOrUpdateHeader("Authorization", $"Bearer {token}");
                var response = await client.ExecuteAsync<TModel>(request);
                if (!response.IsSuccessful)
                {
                    if (rejectIfFailed)
                    {
                        return response;
                    }
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        RemoveAuthToken();
                        return await ExecuteWithHandlerAsync<TModel>(client, request, true);
                    }
                }
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

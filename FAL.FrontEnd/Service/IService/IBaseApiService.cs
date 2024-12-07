using RestSharp;

namespace FAL.FrontEnd.Service.IService
{
    public interface IBaseApiService
    {
        Task<RestResponse> CallApiAsync(string endpoint, Method method, Dictionary<string, string>? queryParams = null, object? bodyParams = null);
    }
}

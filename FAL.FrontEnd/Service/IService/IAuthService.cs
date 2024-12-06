using FAL.FrontEnd.Pages.Auth;
using Share.Model;

namespace FAL.FrontEnd.Service.IService
{
    public interface IAuthService
    {
        Task<string> GetTokenAsync(string username, string password);
    }
}

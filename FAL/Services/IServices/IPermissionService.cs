using FAL.Models;

namespace FAL.Services.IServices
{
    public interface IPermissionService
    {
        Task<Role> GetRoleByUsername(string username);
        bool HasPermission(Role role, string apiEndpoint, string method);
    }

}

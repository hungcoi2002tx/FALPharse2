using FAL.Models;
using System.Security.Claims;

namespace FAL.Services.IServices
{
    public interface IPermissionService
    {
        //Task<Role> GetRoleByUsername(string username);
        bool HasPermission(ClaimsPrincipal user, string? resource, string? action);
    }

}

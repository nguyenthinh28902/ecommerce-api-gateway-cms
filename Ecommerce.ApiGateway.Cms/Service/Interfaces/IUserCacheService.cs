using Ecommerce.ApiGateway.Cms.Models.Auths;

namespace Ecommerce.ApiGateway.Cms.Service.Interfaces
{
    public interface IUserCacheService
    {
        Task<UserInternalInfo?> GetUserInfoAsync(string userId);
    }
}

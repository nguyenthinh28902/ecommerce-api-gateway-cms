using Ecommerce.ApiGateway.Cms.Models.Auths;

namespace Ecommerce.ApiGateway.Cms.Service.Interfaces
{
    public interface IUserService
    {
        public Task<UserInternalInfo?> GetUserInfoAsync(string sub);
    }
}

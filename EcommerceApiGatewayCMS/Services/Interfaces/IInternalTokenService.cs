using EcommerceApiGatewayCMS.Models.DTOs;

namespace EcommerceApiGatewayCMS.Services.Interfaces
{
    public interface IInternalTokenService
    {
        public Task<string> GetSystemTokenAsync();
        public Task<string> GetUserScopedTokenAsync(SignInResponseDto? signInResponseDto);
    }
}

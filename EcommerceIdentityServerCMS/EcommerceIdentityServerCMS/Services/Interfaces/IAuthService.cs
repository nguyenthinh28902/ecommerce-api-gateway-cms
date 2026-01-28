using EcommerceApiGatewayCMS.Models.DTOs;
using EcommerceApiGatewayCMS.Models.ViewModels.Accounts;

namespace EcommerceApiGatewayCMS.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<SignInResponseDto> AuthenticateInternal(SignInViewModel signInViewModel);
        public Task SignIn(SignInResponseDto signInResponseDto);
    }
}

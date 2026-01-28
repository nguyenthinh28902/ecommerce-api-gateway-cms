using EcommerceIdentityServerCMS.Models;
using EcommerceIdentityServerCMS.Models.DTOs.SignIn;
using EcommerceIdentityServerCMS.Models.ViewModels.Accounts;

namespace EcommerceIdentityServerCMS.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Xác thực thông tin đăng nhập với IdentityCMSService thông qua System Token.
        /// </summary>
        /// <param name="signInViewModel">Thông tin UserID và Password từ người dùng.</param>
        /// <returns>Thông tin User đã được xác thực dưới dạng SignInResponseDto.</returns>
        Task<SignInResponseDto?> AuthenticateInternal(SignInViewModel signInViewModel);

        /// <summary>
        /// Thực hiện lưu trữ phiên đăng nhập vào IdentityServer Cookie.
        /// Thiết lập các Claims quan trọng như sub, email, wid và roles.
        /// </summary>
        /// <param name="user">Dữ liệu người dùng từ hệ thống nội bộ.</param>
        Task SignInIdentityUserAsync(SignInResponseDto user);

        /// <summary>
        /// Đổi mã Authorization Code lấy Token thực thi bên ngoài (External Token).
        /// Sử dụng ExchangeRequest chứa Code và CodeVerifier cho luồng PKCE.
        /// </summary>
        /// <param name="exchangeRequest">Model chứa Code, CodeVerifier và các thông tin liên quan.</param>
        /// <returns>Result bao gồm TokenResponseDto nếu thành công.</returns>
        Task<Result<TokenResponseDto?>> ExchangeCodeForExternalToken(ExchangeRequest exchangeRequest);
    }
}

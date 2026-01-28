using EcommerceIdentityServerCMS.Models.DTOs.SignIn;

namespace EcommerceIdentityServerCMS.Services.Interfaces
{
    public interface IInternalTokenService
    {
        /// <summary>
        /// Lấy token hệ thống (Client Credentials) cho một service cụ thể.
        /// Thường dùng cho các tác vụ background hoặc server-to-server không có user.
        /// </summary>
        Task<TokenResponseDto?> GetSystemTokenAsync(string serviceName);

        /// <summary>
        /// Lấy token có chứa ngữ cảnh người dùng (User Payload).
        /// Dùng để truyền thông tin user hiện tại xuống các service hạ tầng.
        /// </summary>
        Task<TokenResponseDto?> GetUserScopedTokenAsync(string serviceName, SignInResponseDto userContext);

        /// <summary>
        /// Đổi Authorization Code lấy bộ Access Token và Refresh Token từ IdentityServer.
        /// </summary>
        Task<TokenResponseDto?> ExchangeAuthorizationCodeAsync(
            string serviceName,
            ExchangeRequest exchangeRequest
            );
    }
}

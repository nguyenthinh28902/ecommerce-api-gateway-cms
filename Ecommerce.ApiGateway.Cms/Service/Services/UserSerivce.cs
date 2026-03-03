using Ecommerce.ApiGateway.Cms.Models.Auths;
using Ecommerce.ApiGateway.Cms.Service.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Net;

namespace Ecommerce.ApiGateway.Cms.Service.Services
{
    public class UserSerivce : IUserService
    {
        private readonly ILogger<UserSerivce> _logger;
        private readonly IUserCacheService _userCacheService;
        private readonly HttpClient _httpClient;
        private readonly ITokenClientService _tokenClientService;

        public UserSerivce(
            ILogger<UserSerivce> logger,
            IUserCacheService userCacheService,
            HttpClient httpClient,
            ITokenClientService tokenClientService)
        {
            _logger = logger;
            _userCacheService = userCacheService;
            _httpClient = httpClient;
            _tokenClientService = tokenClientService;
        }

        public async Task<UserInternalInfo?> GetUserInfoAsync(string sub)
        {
            // 1. Kiểm tra Cache trước
            var userInfo = await _userCacheService.GetUserInfoAsync(sub);
            if (userInfo != null) return userInfo;

            _logger.LogInformation(">>> Cache miss cho user {sub}, bắt đầu gọi API lấy dữ liệu.", sub);

            try
            {
                // 2. Lấy System Token để gọi Service-to-Service
                var systemToken = await _tokenClientService.GetSystemTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", systemToken);
                _httpClient.DefaultRequestHeaders.Add("X-User-Id", sub);

                // 3. Call API sang Customer Service (ví dụ route: /api/internal/user/{sub})
                var response = await _httpClient.GetAsync($"/api/xac-thuc/thong-tin-xac-thuc-nhan-su");

                if (response.IsSuccessStatusCode)
                {
                    // Sử dụng Result<T> ný đã định nghĩa để map dữ liệu
                    var result = await response.Content.ReadFromJsonAsync<UserInternalInfo>();

                    if (result != null)
                    {
                        return result;
                    }
                }

                _logger.LogWarning(">>> Không tìm thấy thông tin user {sub} từ API.", sub);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ">>> Lỗi khi call API lấy thông tin user cho {sub}", sub);
                return null;
            }
        }
    }
}

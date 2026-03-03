using Ecommerce.ApiGateway.Cms.Models;
using Ecommerce.ApiGateway.Cms.Models.Auths;
using Ecommerce.ApiGateway.Cms.Service.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Ecommerce.ApiGateway.Cms.Service.Services
{
    public class UserCacheService : IUserCacheService
    {
        private readonly ILogger<UserCacheService> _logger;
        private readonly IDistributedCache _cache;
        // Đây là "vùng tên" riêng cho Identity để không lẫn với UserSession của Gateway
        private const string IDENTITY_INTERNAL_PREFIX = "InternalWebAuth:";

        public UserCacheService(IDistributedCache cache, ILogger<UserCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<UserInternalInfo?> GetUserInfoAsync(string userId)
        {
            try
            {
                // Key trong Redis: ví dụ "user_info:123"
                var cacheKey = $"{IDENTITY_INTERNAL_PREFIX}{AuthCacheOptions.CacheUserInfor}{userId}";
                var jsonData = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(jsonData)) return null;

                return JsonSerializer.Deserialize<UserInternalInfo>(jsonData);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user info from cache for userId: {UserId}", userId);
                return null;
            }
        }
    }
}
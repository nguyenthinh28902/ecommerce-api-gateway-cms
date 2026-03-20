using Ecommerce.ApiGateway.Cms.Models;
using Ecommerce.ApiGateway.Cms.Models.Auths;
using Ecommerce.ApiGateway.Cms.Models.Settings;
using Ecommerce.ApiGateway.Cms.Service.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Ecommerce.ApiGateway.Cms.Service.Services
{
    public class UserCacheService : IUserCacheService
    {
        private readonly ILogger<UserCacheService> _logger;
        private readonly IDistributedCache _cache;
        private readonly RedisConnection _redisConnection;
        // Đây là "vùng tên" riêng cho Identity để không lẫn với UserSession của Gateway
        private const string IDENTITY_INTERNAL_PREFIX = "InternalWebAuth:";

        public UserCacheService(IDistributedCache cache, ILogger<UserCacheService> logger, IOptions<RedisConnection> options)
        {
            _cache = cache;
            _logger = logger;
            _redisConnection = options.Value;
        }

        public async Task<UserInternalInfo?> GetUserInfoAsync(string userId)
        {
            if(!_redisConnection.Enabled) return null;
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
using Ecommerce.ApiGateway.Cms.Models.Auths;
using Ecommerce.ApiGateway.Cms.Service.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Ecommerce.ApiGateway.Cms.Service.Services
{
    public class UserCacheService : IUserCacheService
    {
        private readonly IDistributedCache _cache;

        public UserCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<UserInternalInfo?> GetUserInfoAsync(string userId)
        {
            // Key trong Redis: ví dụ "user_info:123"
            var cacheKey = $"user_info:{userId}";
            var jsonData = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(jsonData)) return null;

            return JsonSerializer.Deserialize<UserInternalInfo>(jsonData);
        }
    }
}
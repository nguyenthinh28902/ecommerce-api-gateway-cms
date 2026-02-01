using Ecommerce.ApiGateway.Cms.Models.Settings;
using Ecommerce.ApiGateway.Cms.Service.Interfaces;
using Ecommerce.ApiGateway.Cms.Service.Services;

namespace Ecommerce.ApiGateway.Cms.Common.Helpers
{
    public static class StackExchangeRedisCacheExtensions
    {
        public static IServiceCollection AddStackExchangeRedis(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Mapping từ Section "RedisConnection" trong appsettings vào Model
            var redisSettings = configuration.GetSection("RedisConfig").Get<RedisConnection>()
                ?? throw new InvalidOperationException("RedisConfig configuration is missing.");

            // 2. Đăng ký StackExchangeRedis với thông số từ Model
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisSettings.RedisConnectionString;
                options.InstanceName = redisSettings.GatewayInstance;
            });

            // Đăng ký Service xử lý cache user (như đã bàn ở bước trước)
            services.AddScoped<IUserCacheService, UserCacheService>();
            // Đăng ký Service của bạn
            services.AddScoped<ITokenClientService, TokenClientService>();
            return services;
        }
    }
}

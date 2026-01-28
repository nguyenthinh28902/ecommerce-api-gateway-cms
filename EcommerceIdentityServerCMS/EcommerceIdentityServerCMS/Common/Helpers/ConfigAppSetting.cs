using EcommerceApiGatewayCMS.Models.Settings;

namespace EcommerceApiGatewayCMS.Common.Helpers
{
    public static class ConfigAppSetting
    {
        public static IServiceCollection AddConfigAppSetting(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(
             configuration.GetSection("JwtSettings"));
            return services;
        }
    }
}

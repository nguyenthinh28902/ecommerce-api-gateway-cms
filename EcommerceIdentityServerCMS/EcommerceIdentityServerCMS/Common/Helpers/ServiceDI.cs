using EcommerceIdentityServerCMS.Services.Interfaces;
using EcommerceIdentityServerCMS.Services.Services;

namespace EcommerceIdentityServerCMS.Common.Helpers
{
    public static class ServiceDI
    {
        public static IServiceCollection AddServiceDI(this IServiceCollection services, IConfiguration configuration)
        {
            // 2. Cấu hình cho InternalTokenService
            services.AddHttpClient<IInternalTokenService, InternalTokenService>(client =>
            {
                client.BaseAddress = new Uri(configuration["InternalAuth:TokenEndpoint"] ?? string.Empty);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler // TẠO MỚI Ở ĐÂY
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            });

            // 3. Cấu hình cho AuthService
            services.AddHttpClient<IAuthService, AuthService>(client =>
            {
                client.BaseAddress = new Uri(configuration["IdentityCMSService:BaseUrl"] ?? string.Empty);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler // TẠO MỚI Ở ĐÂY
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            });
            return services;
        }
    }
}

using EcommerceApiGatewayCMS.Services.Interfaces;
using EcommerceApiGatewayCMS.Services.Services;

namespace EcommerceApiGatewayCMS.Common.Helpers
{
    public static class ServiceDI
    {
        public static IServiceCollection AddServiceDI(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Cấu hình Handler dùng chung để bỏ qua SSL (Dành cho môi trường Dev/K8s nội bộ)
            var insecureHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };

            // 2. Cấu hình cho InternalTokenService (Sửa lỗi SSL khi gọi chính mình qua HTTPS)
            services.AddHttpClient<IInternalTokenService, InternalTokenService>(client =>
            {
                // Đảm bảo trỏ tới https://gateway-service:443/connect/token
                client.BaseAddress = new Uri(configuration["InternalAuth:TokenEndpoint"] ?? string.Empty);
            })
            .ConfigurePrimaryHttpMessageHandler(() => insecureHandler);

            services.AddHttpClient<IAuthService, AuthService>(client =>
            {
                // Đảm bảo trỏ tới http://customer-service:80
                client.BaseAddress = new Uri(configuration["IdentityCMSService:BaseUrl"] ?? string.Empty);
            })
            .ConfigurePrimaryHttpMessageHandler(() => insecureHandler);
            return services;
        }
    }
}

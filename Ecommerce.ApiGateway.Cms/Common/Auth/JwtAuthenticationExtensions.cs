using Ecommerce.ApiGateway.Cms.Models.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.ApiGateway.Cms.Common.Auth
{
    public static class JwtAuthenticationExtensions
    {
        public static IServiceCollection AddGatewayAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1. Lấy cấu hình Jwt từ appsettings.reverseproxy.identity.json
            var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
                ?? throw new InvalidOperationException("JwtSettings missing in configuration");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // IdentityServer URL
                    options.Authority = jwtSettings.Issuer;
                    options.RequireHttpsMetadata = false; // Dev mode

                    // BẮT BUỘC: Lưu token để dùng trong AddTransforms (Token Relay)
                    options.SaveToken = true;

                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,

                        // Thường Gateway không cần check Audience của các service con 
                        // nhưng cần check Audience của chính nó nếu có cấu hình
                        ValidateAudience = false,

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero // Khớp thời gian chính xác giữa Gateway và IdentityServer
                    };
                });

            // 2. Cấu hình Policy sử dụng cho Routes trong proxy-config.yaml
            services.AddAuthorization(options =>
            {
                // Policy yêu cầu quyền đọc thông tin user
                options.AddPolicy("UserReadPolicy", policy =>
                    policy.RequireClaim("scope", "user.read", "user.internal"));

                // Policy yêu cầu quyền ghi (tạo/sửa) user
                options.AddPolicy("UserWritePolicy", policy =>
                    policy.RequireClaim("scope", "user.write", "user.internal"));

                // Policy nội bộ (Internal) chỉ dành cho các service gọi nhau
                options.AddPolicy("InternalPolicy", policy =>
                    policy.RequireClaim("scope", "user.internal"));
            });

            return services;
        }
    }
}

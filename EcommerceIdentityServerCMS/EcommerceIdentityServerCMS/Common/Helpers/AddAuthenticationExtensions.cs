using EcommerceIdentityServerCMS.Models;
using EcommerceIdentityServerCMS.Models.Enums;
using EcommerceIdentityServerCMS.Models.Settings;

namespace EcommerceIdentityServerCMS.Common.Helpers
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthenticationExtensions(this IServiceCollection services, IConfiguration configuration)
        {


            // 1. Kiểm tra cấu hình rõ ràng hơn
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
                ?? throw new InvalidOperationException("Config Error: 'JwtSettings' is missing in appsettings.json");

            // 2. Cấu hình CORS (Giữ nguyên vì Nuxt cần cái này)
            services.AddCors(options =>
            {
                options.AddPolicy(AuthEnum.AllowNuxtCMS.ToString(), policy =>
                {
                    var nuxtUrl = configuration["EcommerceMVCCMS:BaseUrl"];
                    if (!string.IsNullOrEmpty(nuxtUrl))
                    {
                        policy.WithOrigins(nuxtUrl)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    }
                });
            });
            var hours = (int)ExpireTimeSpanSignIn.Medium; // hours = 8
            // 3. Cấu hình Cookie (Để IdentityServer tương tác tốt với trình duyệt hiện đại)
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "identity_auth_session";
                options.Cookie.SameSite = SameSiteMode.None; // Cho phép Cross-site (Nuxt gọi Identity)
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Bắt buộc HTTPS
                options.ExpireTimeSpan = TimeSpan.FromHours(hours); // Phiên đăng nhập sống trong 8 giờ
                options.SlidingExpiration = true; // Tự động gia hạn nếu user còn hoạt động
            });

            return services;
        }
    }
}

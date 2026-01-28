using Duende.IdentityServer;
using EcommerceIdentityServerCMS.Models;
using EcommerceIdentityServerCMS.Models.Settings;

namespace EcommerceIdentityServerCMS.Common.Helpers
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthenticationExtensions(this IServiceCollection services, IConfiguration configuration)
        {

            var _jwtSettings = configuration
         .GetSection("Jwt")
         .Get<JwtSettings>()
         ?? throw new InvalidOperationException("JwtSettings missing");


            var _internalAuth = configuration
         .GetSection("InternalAuth")
         .Get<InternalAuth>()
         ?? throw new InvalidOperationException("JwtSettings missing");

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityServerConstants.DefaultCookieAuthenticationScheme;
                options.DefaultAuthenticateScheme = IdentityServerConstants.DefaultCookieAuthenticationScheme;
                options.DefaultChallengeScheme = IdentityServerConstants.DefaultCookieAuthenticationScheme;
            }).AddCookie("Cookies", options =>
            {
                options.Cookie.Name = "auth_cookie";
                options.Cookie.SameSite = SameSiteMode.None; // Quan trọng
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Quan trọng
            }).AddJwtBearer("Bearer", options =>
            {
                options.RequireHttpsMetadata = false;
                // URL IdentityServer
                options.Authority = _jwtSettings.Issuer;
                options.Audience = _internalAuth.ClientId;
                options.RequireHttpsMetadata = true;

                // Nếu muốn check audience
                options.TokenValidationParameters.ValidateAudience = false;
            });
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.None; // Bắt buộc cho cross-site HTTPS
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Chỉ gửi qua HTTPS
            });
            services.AddCors(options =>
            {
                options.AddPolicy(AuthEnum.AllowNuxtCMS.ToString(), policy =>
                {
                    policy.WithOrigins(configuration["EcommerceVueCMS:BaseUrl"] ?? string.Empty) // URL của Nuxt
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // BẮT BUỘC phải có cái này để gửi Cookie
                });
            });
            return services;
        }
    }
}

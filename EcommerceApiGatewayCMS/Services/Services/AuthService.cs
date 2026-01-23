using Duende.IdentityServer;
using EcommerceApiGatewayCMS.Models.DTOs;
using EcommerceApiGatewayCMS.Models.Settings;
using EcommerceApiGatewayCMS.Models.ViewModels.Accounts;
using EcommerceApiGatewayCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using static Duende.IdentityServer.Models.IdentityResources;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EcommerceApiGatewayCMS.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IInternalTokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ILogger<AuthService> _logger;
        public AuthService(HttpClient httpClient,
            IInternalTokenService tokenService, 
            IConfiguration configuration,
            ILogger<AuthService> logger, IOptions<JwtSettings> jwtSettings, 
            IHttpContextAccessor httpContextAccessor) {
            _configuration = configuration;
            _httpClient = httpClient;
            _tokenService = tokenService;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SignInResponseDto?> AuthenticateInternal(SignInViewModel signInViewModel)
        {
            var token = await _tokenService.GetSystemTokenAsync();
            _logger.LogInformation($"token {token}");
            _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {
                Id = signInViewModel.UserId,
                Password = signInViewModel.Password
            };
            var response = await _httpClient.PostAsJsonAsync(
                   $"{_configuration["IdentityCMSService:BaseUrl"]}/api/Auth/xac-thuc-noi-bo",
                   payload);
            response.EnsureSuccessStatusCode();
           var result =  await response.Content.ReadFromJsonAsync<SignInResponseDto>();
            return result;
        }


        public async Task SignIn(SignInResponseDto signInResponseDto)
        {

           
            var claims = new[]
           {
            new Claim(JwtRegisteredClaimNames.Sub, signInResponseDto.Id.ToString()), // ID người dùng
            new Claim(JwtRegisteredClaimNames.Email, signInResponseDto.Email),               // Email
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Mã định danh token duy nhất
            new Claim("Role", signInResponseDto.Role)
            };

            var identity = new ClaimsIdentity(claims, IdentityServerConstants.DefaultCookieAuthenticationScheme);

            var options = new CookieOptions
            {
                HttpOnly = false, // QUAN TRỌNG: Để Vue đọc được
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                Path = "/",
                SameSite = SameSiteMode.Lax
            };
            // 1. Cookie trạng thái đăng nhập
            _httpContextAccessor.HttpContext.Response.Cookies.Append("is_logged_in", "true", options);

            // 2. Cookie thời gian hết hạn (Unix Timestamp)
            var expireTimestamp = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes).ToUnixTimeSeconds().ToString();
            _httpContextAccessor.HttpContext.Response.Cookies.Append("auth_expires", expireTimestamp, options);

            await _httpContextAccessor.HttpContext.SignInAsync(
         IdentityServerConstants.DefaultCookieAuthenticationScheme,
         new ClaimsPrincipal(identity));
        }

    }
}

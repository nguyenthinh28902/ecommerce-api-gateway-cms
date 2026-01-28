using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using EcommerceIdentityServerCMS.Common.Exceptions;
using EcommerceIdentityServerCMS.Models;
using EcommerceIdentityServerCMS.Models.DTOs.SignIn;
using EcommerceIdentityServerCMS.Models.Enums;
using EcommerceIdentityServerCMS.Models.Settings;
using EcommerceIdentityServerCMS.Models.ViewModels.Accounts;
using EcommerceIdentityServerCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using static Duende.IdentityServer.Models.IdentityResources;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EcommerceIdentityServerCMS.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IInternalTokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _cache;

        private readonly ILogger<AuthService> _logger;
        public AuthService(HttpClient httpClient,
            IInternalTokenService tokenService,
            IConfiguration configuration,
            ILogger<AuthService> logger, IOptions<JwtSettings> jwtSettings,
            IHttpContextAccessor httpContextAccessor,
            IDistributedCache cache)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _tokenService = tokenService;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
        }

        public async Task<SignInResponseDto?> AuthenticateInternal(SignInViewModel signInViewModel)
        {
            var token = await _tokenService.GetSystemTokenAsync(ServiceAuth.APIGatewayCMSService.ToString());
            _logger.LogInformation($"token {token}");
            if (token == null) throw new UnauthorizedException("Yêu cầu không được chấp nhận");
            _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var payload = new
            {
                Id = signInViewModel.UserId,
                Password = signInViewModel.Password
            };
            var response = await _httpClient.PostAsJsonAsync(
                   $"{_configuration["IdentityCMSService:BaseUrl"]}{ConfigApi.ApiAuthenticateInternal}",
                   payload);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<SignInResponseDto>();
            return result;
        }

        public async Task SignInIdentityUserAsync(SignInResponseDto user)
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new UnauthorizedException("Không có HttpContext");

            var claims = new List<Claim>
    {
        // Duende yêu cầu Claim "sub" để định danh user
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new Claim("wid", user.WorkplaceId.ToString() ?? ""), // Đơn giản hóa key nếu được
    };

            if (user.Roles != null)
            {
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var identity = new ClaimsIdentity(
                claims,
                IdentityServerConstants.DefaultCookieAuthenticationScheme,
                JwtRegisteredClaimNames.Name, // Cấu hình cho thuộc tính Name
                ClaimTypes.Role               // Cấu hình cho thuộc tính Role
            );

            var principal = new ClaimsPrincipal(identity);

            // QUAN TRỌNG: Thiết lập Properties để giữ phiên làm việc ổn định
            var props = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
            };

            await httpContext.SignInAsync(
                IdentityServerConstants.DefaultCookieAuthenticationScheme,
                principal,
                props // Đưa properties vào đây
            );
        }

        public async Task<Result<TokenResponseDto?>> ExchangeCodeForExternalToken(ExchangeRequest exchangeRequest)
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new UnauthorizedException("Không có HttpContext");

            var appName = httpContext.Request.Headers["X-App-Name"].ToString();
            if (string.IsNullOrEmpty(appName))
                throw new UnauthorizedException("Thiếu X-App-Name");

            

            // 🔥 Exchange authorization_code → access_token (IdentityServer)
            var token = await _tokenService.ExchangeAuthorizationCodeAsync(
                appName,
              exchangeRequest
            );

            if (token == null || string.IsNullOrEmpty(token.AccessToken))
                return Result<TokenResponseDto?>.Failure("Exchange token thất bại");

            return Result<TokenResponseDto?>.Success(
                token,
                "Thông tin token được cấp phát thành công"
            );
        }
    }
}

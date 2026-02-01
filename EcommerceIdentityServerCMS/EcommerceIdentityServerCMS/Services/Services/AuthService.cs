using Duende.IdentityServer;
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
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

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
            _logger.LogInformation($"token {token.AccessToken}");
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
            // 1. Chỉ giữ lại những Claim tối thiểu để định danh
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            };

            // 2. Đóng gói toàn bộ thông tin User thành JSON để lưu Cache
            // UserCacheModel là class chứa đầy đủ: Id, Roles, WorkplaceId, Permissions, v.v...
            var userCache = new UserCacheModel {
                Id = user.Id,
                Email = user.Email,
                Roles = user.Roles,
                WorkplaceId = user.WorkplaceId,
                // Có thể thêm nhiều thông tin khác ở đây mà không sợ nặng Token
            };

            var cacheKey = $"user_info:{user.Id}"; // Phải khớp với Key mà Gateway sẽ đọc
            var jsonProvider = JsonSerializer.Serialize(userCache);

            // Lưu vào Redis (Set thời gian hết hạn bằng hoặc dài hơn Token một chút)
            var hours = (int)ExpireTimeSpanSignIn.Medium;
            await _cache.SetStringAsync(cacheKey, jsonProvider, new DistributedCacheEntryOptions {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(hours)
            });

            // 3. Thực hiện SignIn với bộ Claim tối thiểu
            var isUser = new IdentityServerUser(user.Id.ToString()) {
                DisplayName = user.Id.ToString(),
                AdditionalClaims = claims
            };

            await _httpContextAccessor.HttpContext.SignInAsync(isUser);
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

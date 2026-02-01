using EcommerceIdentityServerCMS.Common.Exceptions;
using EcommerceIdentityServerCMS.Models.DTOs.SignIn;
using EcommerceIdentityServerCMS.Models.Settings;
using EcommerceIdentityServerCMS.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EcommerceIdentityServerCMS.Services.Services
{
    public class InternalTokenService : IInternalTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IDictionary<string, ServiceAuthOptions> _configs;
        private readonly ILogger<InternalTokenService> _logger;

        public InternalTokenService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<InternalTokenService> logger,
            IOptions<Dictionary<string, ServiceAuthOptions>> options)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _configs = options.Value;
        }

        // =========================
        // PUBLIC API
        // =========================

        public Task<TokenResponseDto?> GetSystemTokenAsync(string serviceName)
        {
            return RequestTokenAsync(serviceName, null);
        }

        public Task<TokenResponseDto?> GetUserScopedTokenAsync(
            string serviceName,
            SignInResponseDto userContext)
        {
            return RequestTokenAsync(serviceName, userContext);
        }
        public async Task<TokenResponseDto?> ExchangeAuthorizationCodeAsync(
            string serviceName,
            ExchangeRequest exchangeRequest
            )
        {
            if (string.IsNullOrWhiteSpace(exchangeRequest.Code))
                throw new UnauthorizedException("Thiếu authorization_code");

            if (!_configs.TryGetValue(serviceName, out var cfg))
                throw new UnauthorizedException($"Không có cấu hình OAuth cho {serviceName}");

            var form = new Dictionary<string, string> {
                ["grant_type"] = "authorization_code",
                ["client_id"] = cfg.ClientId,
                ["client_secret"] = cfg.ClientSecret,
                ["code"] = exchangeRequest.Code,
                ["code_verifier"] = exchangeRequest.CodeVerifier,
                ["redirect_uri"] = exchangeRequest.RedirectUri
            };

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(
                    _configuration["InternalAuth:TokenEndpoint"],
                    new FormUrlEncodedContent(form)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to exchange authorization_code for service {Service}",
                    serviceName);
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Token endpoint rejected code for {Service}: {Error}",
                    serviceName,
                    error);

                throw new UnauthorizedException("authorization_code không hợp lệ hoặc đã hết hạn");
            }

            var json = await response.Content.ReadAsStringAsync();

            var token = JsonSerializer.Deserialize<TokenResponseDto>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (!string.IsNullOrEmpty(token?.AccessToken))
                token.IsLogged = true;

            return token;
        }


        // =========================
        // CORE
        // =========================

        private async Task<TokenResponseDto?> RequestTokenAsync(
            string serviceName,
            SignInResponseDto? userContext)
        {
            if (!_configs.TryGetValue(serviceName, out var cfg))
                throw new InvalidOperationException($"No auth config for service: {serviceName}");

            var form = BuildTokenRequestForm(cfg, userContext);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(
                    _configuration["InternalAuth:TokenEndpoint"],
                    new FormUrlEncodedContent(form));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token request failed for service {Service}", serviceName);
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Token endpoint rejected request for {Service}: {Error}",
                    serviceName,
                    error);

                throw new ForbiddenException("Không có quyền truy cập service này");
            }

            var content = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<TokenResponseDto>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!string.IsNullOrEmpty(token?.AccessToken))
                token.IsLogged = true;

            return token;
        }

        // =========================
        // HELPERS
        // =========================

        private static Dictionary<string, string> BuildTokenRequestForm(
         ServiceAuthOptions cfg,
         SignInResponseDto? userContext)
        {
            var form = new Dictionary<string, string> {
                ["grant_type"] = cfg.GrantType,
                ["client_id"] = cfg.ClientId,
                ["client_secret"] = cfg.ClientSecret
            };

            // TRƯỜNG HỢP 1: System-to-System (S2S) - Giữ nguyên
            if (userContext == null)
            {
                if (!string.IsNullOrEmpty(cfg.Scope))
                    form["scope"] = cfg.Scope;

            }
            return form;
        }
    }
}

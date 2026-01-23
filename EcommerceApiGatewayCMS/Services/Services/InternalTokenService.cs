
using EcommerceApiGatewayCMS.Models.DTOs;
using EcommerceApiGatewayCMS.Services.Interfaces;
using System.Text.Json;

namespace EcommerceApiGatewayCMS.Services.Services
{
    public class InternalTokenService : IInternalTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InternalTokenService> _logger;

        public InternalTokenService(HttpClient httpClient, IConfiguration configuration, ILogger<InternalTokenService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        // Hàm cũ: Lấy token hệ thống thuần túy
        public async Task<string> GetSystemTokenAsync()
        {
            return await RequestTokenAsync(null);
        }

        // Hàm mới: Lấy token có định danh User
        public async Task<string> GetUserScopedTokenAsync(SignInResponseDto? signInResponseDto)
        {
            // Truyền userId vào để đính kèm tham số tùy chỉnh
            return await RequestTokenAsync(signInResponseDto);
        }

        // Hàm dùng chung để tránh lặp code
        private async Task<string> RequestTokenAsync(SignInResponseDto? signInResponseDto)
        {
            try
            {
                _logger.LogInformation("thông tin test {a1}", _configuration["InternalAuth:TokenEndpoint"]);
                _logger.LogInformation($"thong tin test {_configuration["InternalAuth:TokenEndpoint"]}");
                _logger.LogInformation("kết thúc");
                var form = new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = "APIEcommerceIdentityCMS.internal",
                    ["client_secret"] = "secret",
                    ["scope"] = "EcommerceIdentityCMS.internal"
                };

                // Nếu có userId, đính kèm vào tham số 'user_id' 
                // để CustomTokenRequestValidator bên IdentityServer có thể đọc được
                if (signInResponseDto != null)
                {
                    form["custom_user_id"] = signInResponseDto.Id.ToString();
                    form["custom_email"] = signInResponseDto.Email.ToString();
                    form["custom_role"] = signInResponseDto.Role.ToString();
                    form["custom_WorkplaceId"] = signInResponseDto.WorkplaceId.ToString();
                }

                var response = await _httpClient.PostAsync(
                    _configuration["InternalAuth:TokenEndpoint"],
                    new FormUrlEncodedContent(form));

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Lấy Token thất bại: {error}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var token = JsonSerializer.Deserialize<TokenResponseDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return token?.AccessToken ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                        "HTTP request failed when calling {Service}",
                        "CustomerService");
                throw;
            }


        }
    }
}

using System.Text.Json.Serialization;

namespace EcommerceIdentityServerCMS.Models.DTOs.SignIn
{
    public class TokenResponseDto
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty; // Trường bạn cần để reset/refresh token
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        public bool IsLogged {  get; set; }
    }
}

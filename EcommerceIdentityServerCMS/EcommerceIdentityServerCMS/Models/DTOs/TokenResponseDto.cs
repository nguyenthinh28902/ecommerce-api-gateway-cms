using System.Text.Json.Serialization;

namespace EcommerceApiGatewayCMS.Models.DTOs
{
    public class TokenResponseDto
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}

namespace EcommerceIdentityServerCMS.Models.DTOs.SignIn
{
    public class ExchangeRequest
    {

        public string Code { get; set; } = string.Empty;

        // Code Verifier để kiểm tra tính hợp lệ của Code (PKCE)
        public string CodeVerifier { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
    }
}

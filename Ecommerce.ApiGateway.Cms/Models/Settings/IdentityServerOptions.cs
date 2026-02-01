namespace Ecommerce.ApiGateway.Cms.Models.Settings
{
    public class IdentityServerOptions
    {
        public string Authority { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Scopes { get; set; } = string.Empty;
    }
}

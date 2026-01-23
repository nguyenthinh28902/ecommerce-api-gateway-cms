namespace EcommerceApiGatewayCMS.Models.Settings
{
    public class InternalAuth
    {
        public string TokenEndpoint { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
    }
}

namespace EcommerceIdentityServerCMS.Models.Enums
{
    public enum ServiceAuth
    {
        APIGatewayCMSService,
        IdentityServer,
        cms_admin_client
    }

    public enum ExpireTimeSpanSignIn
    {
        Short = 1, // 1 hour
        Medium = 8, // 8 hours
        Long = 24 // 24 hours
    }
}

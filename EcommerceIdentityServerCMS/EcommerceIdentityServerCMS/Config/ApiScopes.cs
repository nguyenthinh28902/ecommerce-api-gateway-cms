using Duende.IdentityServer.Models;

namespace EcommerceIdentityServerCMS.Config
{
    public static class ApiScopes
    {
        public static IEnumerable<ApiScope> Get()
        {
            return new[]
            {
            new ApiScope("GatwayIdentityserverCMS.internal", "Gatway"),
            new ApiScope("EcommerceIdentityCMS.internal", "Ecommerce user CMS"),
            };
        }
    }
}

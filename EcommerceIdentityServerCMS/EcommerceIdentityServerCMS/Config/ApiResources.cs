using Duende.IdentityServer.Models;

namespace EcommerceApiGatewayCMS.Config
{
    public static class ApiResources
    {
        public static IEnumerable<ApiResource> Get()
        {
            return new[]
            {
                // Resource mới dành riêng cho Customer Service
                new ApiResource("EcommerceIdentityCMS.internal", "Identity CMS Service Resource")
                {
                    Scopes = { "EcommerceIdentityCMS.internal" }
                }

            };
        }
    }
}

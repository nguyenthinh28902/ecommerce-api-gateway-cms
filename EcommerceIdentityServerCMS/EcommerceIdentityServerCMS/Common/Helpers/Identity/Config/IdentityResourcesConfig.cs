using Duende.IdentityServer.Models;

namespace EcommerceIdentityServerCMS.Common.Helpers.Identity.Config
{
    public static class IdentityResourcesConfig
    {
        public static IEnumerable<IdentityResource> Get()
        {
            return new IdentityResource[]
            {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
            };
        }
    }
}

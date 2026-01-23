using Duende.IdentityServer.Models;

namespace EcommerceApiGatewayCMS.Config
{
    public static class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new[]
            {
               
                new Client
                {
                    ClientId = "APIGatwayCMS.internal",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "GatwayIdentityserverCMS.internal" },
                    AlwaysSendClientClaims = true,
                    ClientClaimsPrefix = ""
                },
                 new Client
                {
                    ClientId = "APIEcommerceIdentityCMS.internal",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "EcommerceIdentityCMS.internal" },
                    AlwaysSendClientClaims = true,
                    ClientClaimsPrefix = ""
                }

            };
        }
    }
}

using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using EcommerceIdentityServerCMS.Models.Enums;

namespace EcommerceIdentityServerCMS.Config
{
    public static class Clients
    {
        public static IEnumerable<Client> Get(string baseUrl)
        {
            return new[]
            {
                new Client
                {
                    ClientId = "APIGatewayCMS.internal",
                    ClientSecrets = { new Secret("gateway-secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientClaimsPrefix = "",
                    AllowedScopes =
                    {
                        "user.internal",
                        "user.read",
                        "user.write",
                        "product.read",
                        "product.write",
                    },
                    AccessTokenLifetime = 5 * 60 // ⏱️ 5 phút là quá đủ
                },

                new Client
                {
                    ClientId = ServiceAuth.NuxtWebEcomCMSApp.ToString(),
                    ClientName = "Nuxt Vue Client",
            
                    // Dùng AuthorizationCode để bảo mật tối đa cho Web
                    AllowedGrantTypes = GrantTypes.Code,
            
                    // Secret này chỉ Nitro Server (Backend của Nuxt) được biết
                    ClientSecrets = { new Secret("nuxt_secret_key_123".Sha256()) },

                    // Nơi .NET Identity sẽ redirect về sau khi login thành công
                    RedirectUris = { $"{baseUrl}/auth/callback" },
            
                    // Nơi redirect về sau khi logout
                    PostLogoutRedirectUris = { baseUrl },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "user.read", // Scope của service bạn muốn gọi
                        "user.write"
                    },

                    // Cho phép lấy Refresh Token để duy trì đăng nhập
                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 3600,
                    // PKCE là bắt buộc cho các ứng dụng hiện đại
                    RequirePkce = true,
                    AllowPlainTextPkce = false,
                    RequireClientSecret = true
                }

            };
        }
    }

}

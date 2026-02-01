using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using EcommerceIdentityServerCMS.Models.Enums;

namespace EcommerceIdentityServerCMS.Common.Helpers.Identity.Config
{
    public static class Clients
    {
        public static IEnumerable<Client> Get(string baseUrl)
        {
            return new[]
            {
                new Client
                {
                    ClientId = "IdentityServer",
                    ClientSecrets = { new Secret("IdentityServer-secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientClaimsPrefix = "",
                    AllowedScopes =
                    {
                        // Chỉ add các quyền "Internal" để Gateway có quyền quản trị cao nhất khi gọi Service
                        "user.internal",
                    },
                    AccessTokenLifetime = 5 * 60 // ⏱️ 5 phút là quá đủ
                },
                new Client
                {
                    ClientId = "APIGatewayCMS.internal",
                    ClientSecrets = { new Secret("gateway-secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientClaimsPrefix = "",
                    AllowedScopes =
                    {
                        // Chỉ add các quyền "Internal" để Gateway có quyền quản trị cao nhất khi gọi Service
                        "user.internal",
                        "product.internal",
                        "order.internal",
                        "stock.internal",
                        "payment.internal"
                    },
                    AccessTokenLifetime = 5 * 60 // ⏱️ 5 phút là quá đủ
                },

                new Client
                {
                    ClientId = ServiceAuth.cms_admin_client.ToString(),
                    ClientName = "DotNet MVC Client",

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true, // Chuẩn bảo mật cao nhất hiện nay
                    RequireClientSecret = true,

                    ClientSecrets = { new Secret("netmvc_secret_key_123".Sha256()) },

                    RedirectUris = { $"{baseUrl}/auth/callback" },
                    PostLogoutRedirectUris = { baseUrl },
    
                    // Cho phép Nuxt nhận thông báo đăng xuất qua front-channel
                    FrontChannelLogoutUri = $"{baseUrl}/auth/dang-xuat",
                    // Quan trọng cho UX
                    RequireConsent = false,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "user.read",
                        "user.write",
                        "product.read",
                        "order.read",
                        "order.write"
                    },

                    AllowOfflineAccess = true, // Cho phép dùng Refresh Token
                    RefreshTokenUsage = TokenUsage.OneTimeOnly, // Bảo mật: mỗi refresh token chỉ dùng 1 lần
                    RefreshTokenExpiration = TokenExpiration.Sliding, // Gia hạn thời gian logout khi user còn hoạt động
    
                    AccessTokenLifetime = 3600, // 1 giờ
                    IdentityTokenLifetime = 3600,
                    UpdateAccessTokenClaimsOnRefresh = true
                }

            };
        }
    }

}

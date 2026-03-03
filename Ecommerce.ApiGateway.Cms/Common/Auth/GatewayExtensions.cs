using Ecommerce.ApiGateway.Cms.Models.Settings;
using Ecommerce.ApiGateway.Cms.Service.Interfaces;
using System.Security.Claims;
using Yarp.ReverseProxy.Transforms;
namespace Ecommerce.ApiGateway.Cms.Common.Auth
{
    public static class GatewayExtensions
    {
        public static IServiceCollection AddCustomAppSettings(
        this IServiceCollection services,
        ConfigurationManager configuration
        )
        {
            // Nạp file cấu hình Reverse Proxy và Identity

            configuration.AddYamlFile("proxy-config-user-service.yaml", optional: false, reloadOnChange: true);
            configuration.AddYamlFile("proxy-config-product-service.yaml", optional: false, reloadOnChange: true);
            return services;
        }

        public static IServiceCollection AddGatewayProxy(this IServiceCollection services, IConfiguration configuration)
        {
            var authSettings = configuration.GetSection("InternalAuthHeader").Get<InternalAuthHeader>();
            services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddTransforms(builderContext =>
            {
                builderContext.AddRequestTransform(async transformContext =>
                {

                    //service get token service-to-service 
                    var tokenService = transformContext.HttpContext.RequestServices.GetRequiredService<ITokenClientService>();
                    var systemToken = string.Empty;
                    var loggerFactory = transformContext.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("GatewayAuthTransform");
                    // 1. Lấy sub (User ID) từ Token ban đầu
                    var claim = transformContext.HttpContext.User;
                    var clientId = claim.FindFirst("client_id")?.Value;
                    if (clientId == "IdentityServer")
                    {
                        logger.LogInformation(">>> [GATEWAY] Internal call detected from {ClientId}. Bypassing transform.", clientId);
                        return; 
                    }
                    var sub = claim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(sub))
                    {
                        //Get thông tin người dùng từ Service (hoặc cache nếu có) để truyền xuống Service phía sau qua Header
                        var userSerivce = transformContext.HttpContext.RequestServices.GetRequiredService<IUserService>();
                        var userInfo = await userSerivce.GetUserInfoAsync(sub);
                        if (userInfo == null)
                        {
                            logger.LogInformation(">>> [GATEWAY] Cache miss for user {Sub}. Fetching from Service...", sub);
                            transformContext.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

                            // 2. Tùy chọn: Trả về một thông điệp JSON để Client (Nuxt/Mobile) dễ xử lý
                            transformContext.HttpContext.Response.ContentType = "application/json";
                            await transformContext.HttpContext.Response.WriteAsJsonAsync(new
                            {
                                error = "Unauthorized",
                                message = "Không tìm thấy thông tin người dùng trong hệ thống hoặc đã hết hạn."
                            });
                            transformContext.HttpContext.Items["Yarp.ReverseProxy.Model.IHttpProxyFeature"] = null;

                          return;
                        }
                        // 3. Truyền xuống Service qua Header (Không truyền ngược vào Token để giữ Token gọn)
                        transformContext.ProxyRequest.Headers.Add("X-User-Id", sub);

                        if (!string.IsNullOrEmpty(userInfo?.Email))
                        {
                            transformContext.ProxyRequest.Headers.Add("X-User-Email", userInfo.Email);
                        }
                        if (userInfo?.Roles != null && userInfo.Roles.Any())
                        {
                            // Chuyển List<string> thành "Admin,Manager,Editor"
                            var rolesString = string.Join(",", userInfo.Roles);
                            var scopesString = string.Join(",", userInfo.Scopes);

                            // Sử dụng TryAddWithoutValidation để tránh lỗi format header
                            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Roles", rolesString);
                            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Scopes", scopesString);

                        } 
                        if (userInfo?.WorkplaceId != null)
                        {
                            transformContext.ProxyRequest.Headers.Add("X-User-WorkplaceId", userInfo.WorkplaceId.ToString());
                        }
                        // --- PHẦN 2: XIN TOKEN MỚI (SERVICE-TO-SERVICE) ---
                        // Gateway dùng danh nghĩa "hệ thống" để gọi các service phía sau

                        systemToken = await tokenService.GetSystemTokenAsync();
                        logger.LogInformation("New System Token (Service-to-Service): Bearer {Token}", systemToken);
                        // Ghi đè hoặc thêm Token hệ thống vào Header Authorization
                        transformContext.ProxyRequest.Headers.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", systemToken);
                    }
                });
            });
            return services;
        }

    }
}

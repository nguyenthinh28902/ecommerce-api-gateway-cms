using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EcommerceIdentityServerCMS.Services.Services
{
    public class GatewayUserProfileService : IProfileService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<GatewayUserProfileService> _logger;

        public GatewayUserProfileService(IDistributedCache cache, ILogger<GatewayUserProfileService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // 1. Lấy thông tin User từ Database/Principal
            var sub = context.Subject.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(sub)) return;

            context.IssuedClaims = new List<Claim>
             {
                    new Claim(JwtRegisteredClaimNames.Sub, sub)
             };
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
        }
    }
}

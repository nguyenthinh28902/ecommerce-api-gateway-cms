using EcommerceApiGatewayCMS.Config;
using EcommerceApiGatewayCMS.Services.Services;

namespace EcommerceApiGatewayCMS.Common.Helpers
{
    public static class AuthenticationIdentityServer
    {
        public static IServiceCollection AddAuthenticationIdentityServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityServer(options =>
            {
                options.KeyManagement.Enabled = false;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            .AddInMemoryIdentityResources(IdentityResourcesConfig.Get())
            .AddInMemoryApiScopes(ApiScopes.Get())
            .AddInMemoryApiResources(ApiResources.Get())
            .AddInMemoryClients(Clients.Get())
            .AddCustomTokenRequestValidator<CustomTokenRequestValidator>()
            .AddDeveloperSigningCredential();


            return services;
        }
    }
}

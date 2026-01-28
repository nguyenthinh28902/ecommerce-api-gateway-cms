using EcommerceIdentityServerCMS.Config;
using EcommerceIdentityServerCMS.Services.Services;

namespace EcommerceIdentityServerCMS.Common.Helpers.Identity
{
    public static class AuthenticationIdentityServer
    {
        public static IServiceCollection AddAuthenticationIdentityServer(this IServiceCollection services, IConfiguration configuration)
        {
            var EcommerceVueCMS = configuration["EcommerceVueCMS:BaseUrl"];
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
            .AddInMemoryClients(Clients.Get(EcommerceVueCMS))
            .AddCustomTokenRequestValidator<CustomTokenRequestValidator>()
            .AddDeveloperSigningCredential();


            return services;
        }
    }
}

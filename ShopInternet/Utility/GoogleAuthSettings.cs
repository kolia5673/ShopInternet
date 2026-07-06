using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace ShopInternet.Utility;

public static class GoogleAuthSettings
{
    public static IServiceCollection AddGoogleAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = configuration["GoogleAuthentication:ClientId"] ??
                                   throw new InvalidOperationException("Google ClientID missing in appsettings.json");
                options.ClientSecret = configuration["GoogleAuthentication:ClientSecret"] ??
                                   throw new InvalidOperationException(("Google Client Secret missing in appsettings.json"));
            });
        return services;
    }
}
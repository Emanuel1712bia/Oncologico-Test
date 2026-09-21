using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderProcessing.Infrastructure.Options;

namespace OrderProcessing.Infrastructure.Security;

public static class KeycloakAuthenticationExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var keycloakOptions = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>()
            ?? new KeycloakOptions();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = keycloakOptions.Authority;
                options.Audience = keycloakOptions.Audience;
                options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
                options.MapInboundClaims = false;

                if (!string.IsNullOrWhiteSpace(keycloakOptions.MetadataAddress))
                {
                    options.MetadataAddress = keycloakOptions.MetadataAddress;
                }

                options.TokenValidationParameters.ValidIssuer = keycloakOptions.Authority;
            });

        services.AddAuthorization();

        return services;
    }
}

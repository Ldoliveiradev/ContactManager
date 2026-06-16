using ContactManager.Application.Abstractions;
using ContactManager.Infrastructure.Persistence;
using ContactManager.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ContactManager.Infrastructure;

/// <summary>
/// Composition root helpers for the Infrastructure layer. The API calls these from its
/// DI container; this is the only place the API touches Infrastructure types.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing connection string 'Postgres'.");

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section '{JwtOptions.SectionName}'.");

        services.AddSingleton(jwtOptions);
        services.AddScoped<IUserRepository>(_ => new UserRepository(connectionString));
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator>(_ => new JwtTokenGenerator(jwtOptions));

        return services;
    }
}

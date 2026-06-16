using System.Text;
using ContactManager.Application.Contacts.Interfaces;
using ContactManager.Application.Contacts.Services;
using ContactManager.Domain.Interfaces;
using ContactManager.Infrastructure.Data;
using ContactManager.Infrastructure.Identity.Interfaces;
using ContactManager.Infrastructure.Identity.Security;
using ContactManager.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ContactManager.Infrastructure.IoC;

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

        // Data
        services.AddScoped<IContactRepository>(_ => new ContactRepository(connectionString));
        services.AddScoped<IAccountRepository>(_ => new AccountRepository(connectionString));

        // Identity
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator>(_ => new JwtTokenGenerator(jwtOptions));
        services.AddScoped<IAuthService, AuthService>();

        // Application
        services.AddScoped<IContactService, ContactService>();

        // JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();

        return services;
    }
}

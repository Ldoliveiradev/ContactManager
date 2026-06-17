using System.Text;
using ContactManager.Application.Accounts.Interfaces;
using ContactManager.Application.Accounts.Services;
using ContactManager.Application.Auth.Interfaces;
using ContactManager.Application.Auth.Services;
using ContactManager.Application.Contacts.Interfaces;
using ContactManager.Application.Contacts.Services;
using ContactManager.Domain.Interfaces;
using ContactManager.Infrastructure.Data.Repositories;
using ContactManager.Infrastructure.Identity.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        // Business data repositories
        services.AddScoped<IContactRepository>(_ => new ContactRepository(connectionString));
        services.AddScoped<IAccountRepository>(_ => new AccountRepository(connectionString));

        // Auth identity persistence (kept separate from the business repositories)
        services.AddScoped<IUserRepository>(_ => new UserRepository(connectionString));

        // Identity crypto/token (no DB)
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenGenerator>(sp =>
            new JwtTokenGenerator(jwtOptions, sp.GetRequiredService<TimeProvider>()));

        // Application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
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

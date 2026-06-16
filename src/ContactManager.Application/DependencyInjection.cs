using ContactManager.Application.Auth;
using ContactManager.Application.Contacts;
using Microsoft.Extensions.DependencyInjection;

namespace ContactManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IContactService, ContactService>();
        return services;
    }
}

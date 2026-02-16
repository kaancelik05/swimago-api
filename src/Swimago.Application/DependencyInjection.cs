using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Swimago.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Services
        services.AddScoped<Interfaces.IUserService, Services.UserService>();
        services.AddScoped<Interfaces.IDestinationService, Services.DestinationService>();
        services.AddScoped<Interfaces.IBoatTourService, Services.BoatTourService>();
        services.AddScoped<Interfaces.IFavoriteService, Services.FavoriteService>();
        services.AddScoped<Interfaces.IPaymentMethodService, Services.PaymentMethodService>();
        services.AddScoped<Interfaces.INewsletterService, Services.NewsletterService>();
        services.AddScoped<Interfaces.IHostService, Services.HostService>();
        services.AddScoped<Interfaces.IAdminService, Services.AdminService>();

        return services;
    }
}

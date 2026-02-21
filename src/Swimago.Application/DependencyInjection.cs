using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Swimago.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(Common.Behaviors.ValidationBehavior<,>));
        });
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
        
        services.AddScoped<Interfaces.IReservationService, Services.ReservationService>();
        services.AddScoped<Interfaces.IPricingService, Services.PricingService>();
        services.AddScoped<Interfaces.IReviewService, Services.ReviewService>();
        services.AddScoped<Interfaces.ISearchService, Services.SearchService>();
        services.AddScoped<Interfaces.IBlogService, Services.BlogService>();

        return services;
    }
}

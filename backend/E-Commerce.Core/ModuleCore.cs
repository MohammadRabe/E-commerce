using E_commerce.Core.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_commerce.Core;

public static class ModuleCore
{
    public static IServiceCollection RegisterModuleCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(ModuleCore).Assembly);
            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(ModuleCore).Assembly);
        return services;
    }
}

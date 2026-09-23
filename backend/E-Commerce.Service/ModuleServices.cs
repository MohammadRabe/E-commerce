using E_commerce.Service.Abstraction;
using E_commerce.Service.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_commerce.Service;

public static class ModuleServices
{
    public static IServiceCollection RegisterModuleServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddSingleton<HttpClient>();
        services.AddScoped<IImageUploadService, CloudinaryImageUploadService>();
        return services;
    }
}

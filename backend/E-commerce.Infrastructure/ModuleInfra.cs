using E_commerce.Data.Entities;
using E_commerce.Infrastructure.Data;
using E_commerce.Infrastructure.Abstraction;
using E_commerce.Infrastructure.uow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_commerce.Infrastructure;

public static class ModuleInfra
{
    public static IServiceCollection RegisterModuleInfra(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("ConnectionStrings:Default").Value
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddHttpContextAccessor();
        services.AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();
        services.AddScoped<Microsoft.AspNetCore.Identity.SignInManager<User>>();

        services.AddTransient<IUOW, UOW>();

        return services;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace E_commerce.Data
{
    public static class ModuleData
    {
        public static IServiceCollection RegisterModuleData(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            return services;
        }
    }
}

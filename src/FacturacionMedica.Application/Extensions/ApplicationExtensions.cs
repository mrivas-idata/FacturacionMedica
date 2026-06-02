using FacturacionMedica.Application.Interfaces;
using FacturacionMedica.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FacturacionMedica.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IFacturacionService, FacturacionService>();
        return services;
    }
}

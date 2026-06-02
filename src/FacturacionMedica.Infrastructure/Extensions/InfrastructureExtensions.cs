using FacturacionMedica.Application.Interfaces;
using FacturacionMedica.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FacturacionMedica.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IFacturacionRepository, FacturacionRepository>();
        return services;
    }
}

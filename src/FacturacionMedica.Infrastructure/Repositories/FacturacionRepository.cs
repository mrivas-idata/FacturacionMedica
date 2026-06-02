using System.Text.Json;
using FacturacionMedica.Application.Interfaces;
using FacturacionMedica.Domain.Entities;
using FacturacionMedica.Infrastructure.Converters;

namespace FacturacionMedica.Infrastructure.Repositories;

public class FacturacionRepository : IFacturacionRepository
{
    private readonly List<Factura> _facturas;

    public FacturacionRepository()
    {
        var assembly = typeof(FacturacionRepository).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .Single(n => n.EndsWith("JsonEjemplo.json"));

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");

        var options = new JsonSerializerOptions
        {
            Converters = { new IntFromDecimalConverter() }
        };

        _facturas = JsonSerializer.Deserialize<List<Factura>>(stream, options) ?? [];
    }

    public Task<IEnumerable<Factura>> ObtenerTodasAsync()
    {
        return Task.FromResult<IEnumerable<Factura>>(_facturas);
    }
}

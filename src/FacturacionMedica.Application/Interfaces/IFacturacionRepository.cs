using FacturacionMedica.Domain.Entities;

namespace FacturacionMedica.Application.Interfaces;

public interface IFacturacionRepository
{
    Task<IEnumerable<Factura>> ObtenerTodasAsync();
}

using FacturacionMedica.Application.DTOs.Responses;

namespace FacturacionMedica.Application.Interfaces;

public interface IFacturacionService
{
    Task<IEnumerable<FacturaDto>> ObtenerTodasLasFacturasAsync();
    Task<IEnumerable<FacturaDto>> ObtenerFacturasPorCompradorAsync(int rut);
    Task<CompradorTotalDto?> ObtenerCompradorConMayorGastoAsync();
    Task<IEnumerable<CompradorTotalDto>> ObtenerTotalesPorCompradorAsync();
    Task<IEnumerable<FacturasPorComunaDto>> ObtenerFacturasAgrupadasPorComunaAsync();
    Task<IEnumerable<FacturaDto>> ObtenerFacturasPorComunaAsync(int comunaId);
}

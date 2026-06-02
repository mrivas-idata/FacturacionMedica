using FacturacionMedica.Application.DTOs.Responses;
using FacturacionMedica.Application.Interfaces;
using FacturacionMedica.Domain.Entities;

namespace FacturacionMedica.Application.Services;

public class FacturacionService : IFacturacionService
{
    private readonly IFacturacionRepository _repository;

    public FacturacionService(IFacturacionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<FacturaDto>> ObtenerTodasLasFacturasAsync()
    {
        var facturas = await _repository.ObtenerTodasAsync();
        return facturas.Select(MapToDto);
    }

    public async Task<IEnumerable<FacturaDto>> ObtenerFacturasPorCompradorAsync(int rut)
    {
        var facturas = await _repository.ObtenerTodasAsync();
        return facturas
            .Where(f => f.RutComprador == rut)
            .Select(MapToDto);
    }

    public async Task<CompradorTotalDto?> ObtenerCompradorConMayorGastoAsync()
    {
        var facturas = await _repository.ObtenerTodasAsync();
        return facturas
            .GroupBy(f => new { f.RutComprador, f.DvComprador })
            .Select(g => new CompradorTotalDto
            {
                RutComprador = g.Key.RutComprador,
                DvComprador = g.Key.DvComprador,
                TotalCompras = g.Sum(f => f.DetalleFactura.Sum(d => d.TotalProducto))
            })
            .OrderByDescending(c => c.TotalCompras)
            .FirstOrDefault();
    }

    public async Task<IEnumerable<CompradorTotalDto>> ObtenerTotalesPorCompradorAsync()
    {
        var facturas = await _repository.ObtenerTodasAsync();
        return facturas
            .GroupBy(f => new { f.RutComprador, f.DvComprador })
            .Select(g => new CompradorTotalDto
            {
                RutComprador = g.Key.RutComprador,
                DvComprador = g.Key.DvComprador,
                TotalCompras = g.Sum(f => f.DetalleFactura.Sum(d => d.TotalProducto))
            })
            .OrderByDescending(c => c.TotalCompras);
    }

    public async Task<IEnumerable<FacturasPorComunaDto>> ObtenerFacturasAgrupadasPorComunaAsync()
    {
        var facturas = await _repository.ObtenerTodasAsync();
        return facturas
            .GroupBy(f => f.ComunaComprador)
            .Select(g => new FacturasPorComunaDto
            {
                ComunaComprador = g.Key,
                Facturas = g.Select(MapToDto).ToList()
            })
            .OrderBy(c => c.ComunaComprador);
    }

    public async Task<IEnumerable<FacturaDto>> ObtenerFacturasPorComunaAsync(int comunaId)
    {
        var facturas = await _repository.ObtenerTodasAsync();
        return facturas
            .Where(f => f.ComunaComprador == comunaId)
            .Select(MapToDto);
    }

    private static FacturaDto MapToDto(Factura factura)
    {
        var total = factura.DetalleFactura.Sum(d => d.TotalProducto);
        return new FacturaDto
        {
            NumeroDocumento = factura.NumeroDocumento,
            RutVendedor = factura.RutVendedor,
            DvVendedor = factura.DvVendedor,
            RutComprador = factura.RutComprador,
            DvComprador = factura.DvComprador,
            DireccionComprador = factura.DireccionComprador,
            ComunaComprador = factura.ComunaComprador,
            RegionComprador = factura.RegionComprador,
            TotalFactura = total,
            DetalleFactura = factura.DetalleFactura.Select(d => new DetalleFacturaDto
            {
                CantidadProducto = d.CantidadProducto,
                Producto = new ProductoDto
                {
                    Descripcion = d.Producto.Descripcion,
                    Valor = d.Producto.Valor
                },
                TotalProducto = d.TotalProducto
            }).ToList()
        };
    }
}

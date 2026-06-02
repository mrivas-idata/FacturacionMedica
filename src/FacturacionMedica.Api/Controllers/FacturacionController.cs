using FacturacionMedica.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionMedica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturacionController : ControllerBase
{
    private readonly IFacturacionService _service;

    public FacturacionController(IFacturacionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodasLasFacturas()
    {
        var facturas = await _service.ObtenerTodasLasFacturasAsync();
        return Ok(facturas);
    }

    [HttpGet("comprador/top-gasto")]
    public async Task<IActionResult> ObtenerCompradorConMayorGasto()
    {
        var comprador = await _service.ObtenerCompradorConMayorGastoAsync();
        if (comprador is null)
            return NotFound(new { message = "No se encontraron compradores registrados." });

        return Ok(comprador);
    }

    [HttpGet("comprador/{rut:int}")]
    public async Task<IActionResult> ObtenerFacturasPorComprador(int rut)
    {
        var facturas = await _service.ObtenerFacturasPorCompradorAsync(rut);
        var lista = facturas.ToList();

        if (lista.Count == 0)
            return NotFound(new { message = $"No se encontraron facturas para el RUT {rut}." });

        return Ok(lista);
    }

    [HttpGet("compradores/totales")]
    public async Task<IActionResult> ObtenerTotalesPorComprador()
    {
        var totales = await _service.ObtenerTotalesPorCompradorAsync();
        return Ok(totales);
    }

    [HttpGet("por-comuna")]
    public async Task<IActionResult> ObtenerFacturasAgrupadasPorComuna()
    {
        var agrupadas = await _service.ObtenerFacturasAgrupadasPorComunaAsync();
        return Ok(agrupadas);
    }

    [HttpGet("por-comuna/{comunaId:int}")]
    public async Task<IActionResult> ObtenerFacturasPorComuna(int comunaId)
    {
        var facturas = await _service.ObtenerFacturasPorComunaAsync(comunaId);
        var lista = facturas.ToList();

        if (lista.Count == 0)
            return NotFound(new { message = $"No se encontraron facturas para la comuna {comunaId}." });

        return Ok(lista);
    }
}

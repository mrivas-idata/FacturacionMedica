using FacturacionMedica.Api.Controllers;
using FacturacionMedica.Application.DTOs.Responses;
using FacturacionMedica.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FacturacionMedica.Test.Controllers;

public class FacturacionControllerTests
{
    private readonly Mock<IFacturacionService> _serviceMock;
    private readonly FacturacionController _controller;

    public FacturacionControllerTests()
    {
        _serviceMock = new Mock<IFacturacionService>();
        _controller = new FacturacionController(_serviceMock.Object);
    }

    // ─── ObtenerTodasLasFacturas ──────────────────────────────────────────────

    [Fact]
    public async Task ObtenerTodasLasFacturas_RetornaOkConFacturas()
    {
        var facturas = new List<FacturaDto> { new() { NumeroDocumento = 1 } };
        _serviceMock.Setup(s => s.ObtenerTodasLasFacturasAsync()).ReturnsAsync(facturas);

        var result = await _controller.ObtenerTodasLasFacturas();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(facturas, ok.Value);
    }

    // ─── ObtenerFacturasPorComprador ─────────────────────────────────────────

    [Fact]
    public async Task ObtenerFacturasPorComprador_RutExistente_RetornaOk()
    {
        var facturas = new List<FacturaDto> { new() { RutComprador = 11111111 } };
        _serviceMock.Setup(s => s.ObtenerFacturasPorCompradorAsync(11111111)).ReturnsAsync(facturas);

        var result = await _controller.ObtenerFacturasPorComprador(11111111);

        var ok = Assert.IsType<OkObjectResult>(result);
        var lista = Assert.IsType<List<FacturaDto>>(ok.Value);
        Assert.Single(lista);
    }

    [Fact]
    public async Task ObtenerFacturasPorComprador_RutInexistente_RetornaNotFound()
    {
        _serviceMock.Setup(s => s.ObtenerFacturasPorCompradorAsync(99999999))
            .ReturnsAsync([]);

        var result = await _controller.ObtenerFacturasPorComprador(99999999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ─── ObtenerCompradorConMayorGasto ────────────────────────────────────────

    [Fact]
    public async Task ObtenerCompradorConMayorGasto_ExistenFacturas_RetornaOk()
    {
        var comprador = new CompradorTotalDto { RutComprador = 11111111, TotalCompras = 6000 };
        _serviceMock.Setup(s => s.ObtenerCompradorConMayorGastoAsync()).ReturnsAsync(comprador);

        var result = await _controller.ObtenerCompradorConMayorGasto();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(comprador, ok.Value);
    }

    [Fact]
    public async Task ObtenerCompradorConMayorGasto_SinFacturas_RetornaNotFound()
    {
        _serviceMock.Setup(s => s.ObtenerCompradorConMayorGastoAsync())
            .ReturnsAsync((CompradorTotalDto?)null);

        var result = await _controller.ObtenerCompradorConMayorGasto();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ─── ObtenerTotalesPorComprador ───────────────────────────────────────────

    [Fact]
    public async Task ObtenerTotalesPorComprador_RetornaOkConLista()
    {
        var totales = new List<CompradorTotalDto>
        {
            new() { RutComprador = 11111111, TotalCompras = 6000 },
            new() { RutComprador = 22222222, TotalCompras = 5000 }
        };
        _serviceMock.Setup(s => s.ObtenerTotalesPorCompradorAsync()).ReturnsAsync(totales);

        var result = await _controller.ObtenerTotalesPorComprador();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(totales, ok.Value);
    }

    // ─── ObtenerFacturasAgrupadasPorComuna ────────────────────────────────────

    [Fact]
    public async Task ObtenerFacturasAgrupadasPorComuna_RetornaOkConAgrupacion()
    {
        var agrupadas = new List<FacturasPorComunaDto>
        {
            new() { ComunaComprador = 65, Facturas = [new() { NumeroDocumento = 1 }] }
        };
        _serviceMock.Setup(s => s.ObtenerFacturasAgrupadasPorComunaAsync()).ReturnsAsync(agrupadas);

        var result = await _controller.ObtenerFacturasAgrupadasPorComuna();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(agrupadas, ok.Value);
    }

    // ─── ObtenerFacturasPorComuna ─────────────────────────────────────────────

    [Fact]
    public async Task ObtenerFacturasPorComuna_ComunaExistente_RetornaOk()
    {
        var facturas = new List<FacturaDto> { new() { ComunaComprador = 65 } };
        _serviceMock.Setup(s => s.ObtenerFacturasPorComunaAsync(65)).ReturnsAsync(facturas);

        var result = await _controller.ObtenerFacturasPorComuna(65);

        var ok = Assert.IsType<OkObjectResult>(result);
        var lista = Assert.IsType<List<FacturaDto>>(ok.Value);
        Assert.Single(lista);
    }

    [Fact]
    public async Task ObtenerFacturasPorComuna_ComunaInexistente_RetornaNotFound()
    {
        _serviceMock.Setup(s => s.ObtenerFacturasPorComunaAsync(999)).ReturnsAsync([]);

        var result = await _controller.ObtenerFacturasPorComuna(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}

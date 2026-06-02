using FacturacionMedica.Application.Interfaces;
using FacturacionMedica.Application.Services;
using FacturacionMedica.Domain.Entities;
using Moq;

namespace FacturacionMedica.Test.Services;

public class FacturacionServiceTests
{
    private readonly Mock<IFacturacionRepository> _repoMock;
    private readonly FacturacionService _service;

    public FacturacionServiceTests()
    {
        _repoMock = new Mock<IFacturacionRepository>();
        _service = new FacturacionService(_repoMock.Object);
    }

    // ─── datos de prueba ───────────────────────────────────────────────────────

    private static List<Factura> GetTestFacturas() =>
    [
        new Factura
        {
            NumeroDocumento = 1,
            RutVendedor = 99000000,
            DvVendedor = "0",
            RutComprador = 11111111,
            DvComprador = "1",
            DireccionComprador = "Calle A",
            ComunaComprador = 65,
            RegionComprador = 13,
            TotalFactura = 0,       // deliberadamente 0; el servicio lo debe calcular
            DetalleFactura =
            [
                new DetalleFactura { CantidadProducto = 2, TotalProducto = 1000, Producto = new Producto { Descripcion = "P1", Valor = 500 } },
                new DetalleFactura { CantidadProducto = 3, TotalProducto = 2000, Producto = new Producto { Descripcion = "P2", Valor = 666 } }
            ]
        },
        new Factura
        {
            NumeroDocumento = 2,
            RutVendedor = 99000001,
            DvVendedor = "1",
            RutComprador = 22222222,
            DvComprador = "2",
            DireccionComprador = "Calle B",
            ComunaComprador = 65,
            RegionComprador = 13,
            TotalFactura = 0,
            DetalleFactura =
            [
                new DetalleFactura { CantidadProducto = 1, TotalProducto = 5000, Producto = new Producto { Descripcion = "P3", Valor = 5000 } }
            ]
        },
        new Factura
        {
            NumeroDocumento = 3,
            RutVendedor = 99000000,
            DvVendedor = "0",
            RutComprador = 11111111,
            DvComprador = "1",
            DireccionComprador = "Calle C",
            ComunaComprador = 78,
            RegionComprador = 13,
            TotalFactura = 0,
            DetalleFactura =
            [
                new DetalleFactura { CantidadProducto = 5, TotalProducto = 3000, Producto = new Producto { Descripcion = "P1", Valor = 600 } }
            ]
        }
    ];

    // ─── ObtenerTodasLasFacturasAsync ──────────────────────────────────────────

    [Fact]
    public async Task ObtenerTodasLasFacturas_RetornaTodasLasFacturas()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = (await _service.ObtenerTodasLasFacturasAsync()).ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task ObtenerTodasLasFacturas_TotalCalculadoDesdDetalle_NoDesdeJsonField()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = (await _service.ObtenerTodasLasFacturasAsync()).ToList();

        // Factura 1: detalle suma 1000 + 2000 = 3000
        Assert.Equal(3000m, result[0].TotalFactura);
        // Factura 2: detalle suma 5000
        Assert.Equal(5000m, result[1].TotalFactura);
        // Factura 3: detalle suma 3000
        Assert.Equal(3000m, result[2].TotalFactura);
    }

    [Fact]
    public async Task ObtenerTodasLasFacturas_MapeaDetalleCorrectamente()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = (await _service.ObtenerTodasLasFacturasAsync()).ToList();

        Assert.Equal(2, result[0].DetalleFactura.Count);
        Assert.Equal("P1", result[0].DetalleFactura[0].Producto.Descripcion);
    }

    // ─── ObtenerFacturasPorCompradorAsync ─────────────────────────────────────

    [Fact]
    public async Task ObtenerFacturasPorComprador_RutExistente_RetornaFacturasDelComprador()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = (await _service.ObtenerFacturasPorCompradorAsync(11111111)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.Equal(11111111, f.RutComprador));
    }

    [Fact]
    public async Task ObtenerFacturasPorComprador_RutInexistente_RetornaListaVacia()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = await _service.ObtenerFacturasPorCompradorAsync(99999999);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ObtenerFacturasPorComprador_TotalCalculadoDesdDetalle()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = (await _service.ObtenerFacturasPorCompradorAsync(22222222)).ToList();

        Assert.Single(result);
        Assert.Equal(5000m, result[0].TotalFactura);
    }

    // ─── ObtenerCompradorConMayorGastoAsync ───────────────────────────────────

    [Fact]
    public async Task ObtenerCompradorConMayorGasto_RetornaCompradorConMayorTotal()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        // Comprador 11111111: 3000 + 3000 = 6000
        // Comprador 22222222: 5000
        var result = await _service.ObtenerCompradorConMayorGastoAsync();

        Assert.NotNull(result);
        Assert.Equal(11111111, result.RutComprador);
        Assert.Equal(6000m, result.TotalCompras);
    }

    [Fact]
    public async Task ObtenerCompradorConMayorGasto_SinFacturas_RetornaNull()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync([]);

        var result = await _service.ObtenerCompradorConMayorGastoAsync();

        Assert.Null(result);
    }

    // ─── ObtenerTotalesPorCompradorAsync ─────────────────────────────────────

    [Fact]
    public async Task ObtenerTotalesPorComprador_AgrupaYSumaComprasDelMismoComprador()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = (await _service.ObtenerTotalesPorCompradorAsync()).ToList();

        Assert.Equal(2, result.Count);
        var comprador1 = result.Single(c => c.RutComprador == 11111111);
        Assert.Equal(6000m, comprador1.TotalCompras);
    }

    [Fact]
    public async Task ObtenerTotalesPorComprador_OrdenadoPorTotalDescendente()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = (await _service.ObtenerTotalesPorCompradorAsync()).ToList();

        Assert.True(result[0].TotalCompras >= result[1].TotalCompras);
    }

    [Fact]
    public async Task ObtenerTotalesPorComprador_ContieneTodosLosCompradores()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = (await _service.ObtenerTotalesPorCompradorAsync()).ToList();

        Assert.Contains(result, c => c.RutComprador == 11111111);
        Assert.Contains(result, c => c.RutComprador == 22222222);
    }

    // ─── ObtenerFacturasAgrupadasPorComunaAsync ───────────────────────────────

    [Fact]
    public async Task ObtenerFacturasAgrupadasPorComuna_CreaGruposPorComuna()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = (await _service.ObtenerFacturasAgrupadasPorComunaAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ObtenerFacturasAgrupadasPorComuna_GrupoContieneFacturasCorrectas()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = (await _service.ObtenerFacturasAgrupadasPorComunaAsync()).ToList();

        var grupo65 = result.Single(g => g.ComunaComprador == 65);
        Assert.Equal(2, grupo65.Facturas.Count);

        var grupo78 = result.Single(g => g.ComunaComprador == 78);
        Assert.Single(grupo78.Facturas);
    }

    [Fact]
    public async Task ObtenerFacturasAgrupadasPorComuna_OrdenadoPorComunaAscendente()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = (await _service.ObtenerFacturasAgrupadasPorComunaAsync()).ToList();

        Assert.True(result[0].ComunaComprador <= result[1].ComunaComprador);
    }

    // ─── ObtenerFacturasPorComunaAsync ────────────────────────────────────────

    [Fact]
    public async Task ObtenerFacturasPorComuna_ComunaExistente_RetornaFacturasDeEsaComuna()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = (await _service.ObtenerFacturasPorComunaAsync(65)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.Equal(65, f.ComunaComprador));
    }

    [Fact]
    public async Task ObtenerFacturasPorComuna_ComunaInexistente_RetornaListaVacia()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = await _service.ObtenerFacturasPorComunaAsync(999);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ObtenerFacturasPorComuna_TotalCalculadoDesdDetalle()
    {
        _repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(GetTestFacturas());

        var result = (await _service.ObtenerFacturasPorComunaAsync(78)).ToList();

        Assert.Single(result);
        Assert.Equal(3000m, result[0].TotalFactura);
    }
}

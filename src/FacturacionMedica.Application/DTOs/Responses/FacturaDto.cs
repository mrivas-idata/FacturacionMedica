namespace FacturacionMedica.Application.DTOs.Responses;

public class FacturaDto
{
    public int NumeroDocumento { get; set; }
    public int RutVendedor { get; set; }
    public string DvVendedor { get; set; } = string.Empty;
    public int RutComprador { get; set; }
    public string DvComprador { get; set; } = string.Empty;
    public string DireccionComprador { get; set; } = string.Empty;
    public int ComunaComprador { get; set; }
    public int RegionComprador { get; set; }
    public decimal TotalFactura { get; set; }
    public List<DetalleFacturaDto> DetalleFactura { get; set; } = [];
}

public class DetalleFacturaDto
{
    public int CantidadProducto { get; set; }
    public ProductoDto Producto { get; set; } = new();
    public decimal TotalProducto { get; set; }
}

public class ProductoDto
{
    public string Descripcion { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}

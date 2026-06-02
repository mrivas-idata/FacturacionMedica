using System.Text.Json.Serialization;

namespace FacturacionMedica.Domain.Entities;

public class DetalleFactura
{
    [JsonPropertyName("CantidadProducto")]
    public int CantidadProducto { get; set; }

    [JsonPropertyName("Producto")]
    public Producto Producto { get; set; } = new();

    [JsonPropertyName("TotalProducto")]
    public decimal TotalProducto { get; set; }
}

using System.Text.Json.Serialization;

namespace FacturacionMedica.Domain.Entities;

public class Producto
{
    [JsonPropertyName("Descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [JsonPropertyName("Valor")]
    public decimal Valor { get; set; }
}

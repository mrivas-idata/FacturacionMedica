using System.Text.Json.Serialization;

namespace FacturacionMedica.Domain.Entities;

public class Factura
{
    [JsonPropertyName("NumeroDocumento")]
    public int NumeroDocumento { get; set; }

    [JsonPropertyName("RUTVendedor")]
    public int RutVendedor { get; set; }

    [JsonPropertyName("DvVendedor")]
    public string DvVendedor { get; set; } = string.Empty;

    [JsonPropertyName("RUTComprador")]
    public int RutComprador { get; set; }

    [JsonPropertyName("DvComprador")]
    public string DvComprador { get; set; } = string.Empty;

    [JsonPropertyName("DireccionComprador")]
    public string DireccionComprador { get; set; } = string.Empty;

    [JsonPropertyName("ComunaComprador")]
    public int ComunaComprador { get; set; }

    [JsonPropertyName("RegionComprador")]
    public int RegionComprador { get; set; }

    [JsonPropertyName("TotalFactura")]
    public int TotalFactura { get; set; }

    [JsonPropertyName("DetalleFactura")]
    public List<DetalleFactura> DetalleFactura { get; set; } = [];
}

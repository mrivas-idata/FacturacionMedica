namespace FacturacionMedica.Application.DTOs.Responses;

public class CompradorTotalDto
{
    public int RutComprador { get; set; }
    public string DvComprador { get; set; } = string.Empty;
    public decimal TotalCompras { get; set; }
}

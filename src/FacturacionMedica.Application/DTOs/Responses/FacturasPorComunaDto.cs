namespace FacturacionMedica.Application.DTOs.Responses;

public class FacturasPorComunaDto
{
    public int ComunaComprador { get; set; }
    public List<FacturaDto> Facturas { get; set; } = [];
}

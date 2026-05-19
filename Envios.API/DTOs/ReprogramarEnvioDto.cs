namespace Envios.API.DTOs
{
    public class ReprogramarEnvioDto
    {
        public DateTime NuevaFecha { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }
}

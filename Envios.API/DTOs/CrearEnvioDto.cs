namespace Envios.API.DTOs
{
    public class CrearEnvioDto
    {
        public Guid IdOrden { get; set; }
        public string DireccionSnapshot { get; set; } = string.Empty;
        public string NombreRepartidor { get; set; } = string.Empty;
        public DateTime FechaEstimada { get; set; }
    }
}

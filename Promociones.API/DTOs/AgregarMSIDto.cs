namespace Promociones.API.DTOs
{
    public class AgregarMSIDto
    {
        public int Meses { get; set; }
        public string BancosParticipantes { get; set; } = string.Empty;
        public decimal MontoMinimoCompra { get; set; }
    }
}

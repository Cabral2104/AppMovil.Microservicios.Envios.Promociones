namespace Promociones.API.DTOs
{
    public class CrearPromocionDto
    {
        public string NombreCampana { get; set; } = string.Empty;
        public decimal PorcentajeDescuento { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool EsIndefinido { get; set; }
        public decimal? MontoMinimoCompra { get; set; }
    }
}

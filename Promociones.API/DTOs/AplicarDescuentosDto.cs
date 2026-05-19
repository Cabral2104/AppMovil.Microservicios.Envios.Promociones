namespace Promociones.API.DTOs
{
    public class AplicarDescuentosDto
    {
        public decimal MontoTotal { get; set; }
        public List<ItemCarritoDto> Items { get; set; } = new();
    }

    public class ItemCarritoDto
    {
        public Guid IdProducto { get; set; }
        public Guid IdCategoria { get; set; }
        public bool EsElectronica { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
    }
}

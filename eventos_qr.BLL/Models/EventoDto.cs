namespace eventos_qr.BLL.Models
{
    public class EventoDto
    {
        public int IdEvento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int Capacidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Vendidas { get; set; } = 0;

        public DateTime RowVersion { get; set; }
        public long RowVersionTicks { get; set; }
    }
}

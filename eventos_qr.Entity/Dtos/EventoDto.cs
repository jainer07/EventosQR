namespace eventos_qr.Entity.Dtos
{
    public class EventoDto
    {
        public int IdEvento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int Capacidad { get; set; }
        public int Disponibles { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Vendidas { get; set; }
        public bool Estado { get; set; } = true;
        public DateTime RowVersion { get; set; }
        public long RowVersionTicks { get; set; }
    }
}

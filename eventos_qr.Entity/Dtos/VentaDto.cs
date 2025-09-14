namespace eventos_qr.Entity.Dtos
{
    public class VentaDto
    {
        public long IdVenta { get; set; }
        public int IdEvento { get; set; }
        public long IdPersona { get; set; }
        public DateTime Fecha { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
        public int EstadoVenta { get; set; } = 1;
        public string ComprobantePago { get; set; } = "";
        public bool EnvioNotificacion { get; set; }
        public long RowVersion { get; set; }

        public decimal PrecioUnitario { get; set; }
        public string NombrePersona { get; set; } = "";
        public int IdTipoDocumento { get; set; }
        public string NumeroDocumento { get; set; } = "";
        public string CelularPersona { get; set; } = "";
        public string NombreEvento { get; set; } = "";
        public int Capacidad { get; set; }
        public int Disponibles { get; set; }
    }
}

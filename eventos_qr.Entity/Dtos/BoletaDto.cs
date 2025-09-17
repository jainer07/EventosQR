namespace eventos_qr.Entity.Dtos
{
    public class BoletaDto
    {
        public long IdBoleta { get; set; }
        public long IdVenta { get; set; }
        public string NumeroBoleta { get; set; } = "";
        public string CodigoQr { get; set; } = "";
        public string UrlImagen { get; set; } = "";
        public DateTime FechaGeneracionUtc { get; set; }
        public bool Estado { get; set; }
        public long RowVersion { get; set; } = 1;

        public string NombrePersona { get; set; } = "";
        public string DocumentoPersona { get; set; } = "";
        public string NombreEvento { get; set; } = "";
    }
}

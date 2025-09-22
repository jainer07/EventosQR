namespace eventos_qr.Entity.Dtos
{
    public class BoletaScanResponse
    {
        public int Codigo { get; set; }                 // 0=OK, otro=error
        public string Mensaje { get; set; } = "";
        public string? Code { get; set; }
        public bool Estado { get; set; }
        public DateTime? FechaUso { get; set; }


        // Datos útiles para overlay/confirmación visual
        public string? Evento { get; set; }
        public DateTime? FechaEvento { get; set; }
        public string? Titular { get; set; }
    }
}

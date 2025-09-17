namespace eventos_qr.Entity.Dtos
{
    public class PersonaDto : RespuestaType
    {
        public long IdPersona { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public int TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public string? Correo { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public long RowVersion { get; set; }
        public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
    }
}

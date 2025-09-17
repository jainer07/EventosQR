using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eventos_qr.Entity.Models
{
    [Table("tbl_persona")]
    public class PersonaModel
    {
        [Key]
        [Column("IdPersona")]
        public long IdPersona { get; set; }

        [Column("Nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Column("Apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Column("TipoDocumento")]
        public int TipoDocumento { get; set; }

        [Column("NumeroDocumento")]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Column("Celular")]
        public string Celular { get; set; } = string.Empty;

        [Column("Correo")]
        public string? Correo { get; set; }

        [Column("Estado")]
        public bool Estado { get; set; } = true;

        [Column("FechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [ConcurrencyCheck]
        [Column("RowVersion")]
        public long RowVersion { get; set; } = 1;

        public ICollection<VentasModel>? Ventas { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eventos_qr.Entity.Models
{
    [Table("tbl_venta")]
    public class VentasModel
    {
        [Key]
        [Column("IdVenta")]
        public long IdVenta { get; set; }

        [Column("IdEvento")]
        public int IdEvento { get; set; }

        [Column("IdPersona")]
        public long IdPersona { get; set; }

        [Column("FechaUtc")]
        public DateTime FechaUtc { get; set; }

        [Column("Cantidad")]
        public int Cantidad { get; set; }

        [Column("Total")]
        public decimal Total { get; set; }

        [Column("EstadoVenta")]
        public int EstadoVenta { get; set; } = 1;

        [Column("ComprobantePago")]
        public string ComprobantePago { get; set; } = "";

        [Column("EnvioNotificacion")]
        public bool EnvioNotificacion { get; set; }

        [ConcurrencyCheck]
        [Column("RowVersion")]
        public long RowVersion { get; set; }

        public EventoModel? Evento { get; set; }
        public PersonaModel? Persona { get; set; }
    }
}

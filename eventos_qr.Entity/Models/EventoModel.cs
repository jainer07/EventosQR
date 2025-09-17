using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eventos_qr.Entity.Models
{
    [Table("tbl_evento")]
    public class EventoModel
    {
        [Key]
        [Column("IdEvento")]
        public int IdEvento { get; set; }

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("Fecha")]
        public DateTime Fecha { get; set; }

        [Column("Capacidad")]
        public int Capacidad { get; set; }

        [Column("Disponibles")]
        public int Disponibles { get; set; }

        [Column("PrecioUnitario")]
        public decimal PrecioUnitario { get; set; }

        [Column("Vendidas")]
        public int Vendidas { get; set; }

        [Column("Estado")]
        public bool Estado { get; set; } = true;

        [Column("RowVersion", TypeName = "timestamp(6)")]
        public DateTime RowVersion { get; set; }
    }
}

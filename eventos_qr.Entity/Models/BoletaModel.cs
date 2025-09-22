using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eventos_qr.Entity.Models
{
    [Table("tbl_boleta")]
    public class BoletaModel
    {
        [Key]
        [Column("IdBoleta")]
        public long IdBoleta { get; set; }

        [Column("IdVenta")]
        public long IdVenta { get; set; }

        [Column("NumeroBoleta")]
        public string NumeroBoleta { get; set; } = "";

        [Column("CodigoQr")]
        public string CodigoQr { get; set; } = "";

        [Column("UrlImagen")]
        public string UrlImagen { get; set; } = "";

        [Column("FechaGeneracionUtc")]
        public DateTime FechaGeneracionUtc { get; set; }

        [Column("FechaUsoUtc")]
        public DateTime? FechaUsoUtc { get; set; }

        [Column("Estado")]
        public bool Estado { get; set; }

        [Column("OperatorId")]
        public long? OperatorId { get; set; }

        [Column("RowVersion")]
        public long RowVersion { get; set; } = 1;

        public VentasModel? Venta { get; set; }
    }
}

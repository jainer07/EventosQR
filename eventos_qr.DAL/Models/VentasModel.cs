using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eventos_qr.DAL.Models
{
    [Table("tbl_persona")]
    public class VentasModel
    {
        [Key]
        [Column("IdVenta")]
        public int IdVenta { get; set; }
    }
}

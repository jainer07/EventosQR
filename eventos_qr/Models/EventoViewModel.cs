using System.ComponentModel.DataAnnotations;

namespace eventos_qr.Models
{
    public class EventoViewModel
    {
        public int IdEvento { get; set; }

        [Required, StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required, Display(Name = "Fecha del evento")]
        public DateTime Fecha { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Capacidad { get; set; }

        [Required, Range(0, 999999999)]
        [Display(Name = "Precio unitario")]
        [DataType(DataType.Currency)]
        public decimal PrecioUnitario { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int Disponibles { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int Vendidas { get; set; }
        public bool Estado { get; set; } = true;

        // token de concurrencia
        public DateTime RowVersion { get; set; }
        public long RowVersionTicks { get; set; }
    }
}

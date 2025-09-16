using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace eventos_qr.Models
{
    public class VentaViewModel
    {
        public long IdVenta { get; set; }

        [Required, Display(Name = "Evento")]
        public int IdEvento { get; set; }

        [Required]
        public long IdPersona { get; set; }
        
        [Required]
        [Display(Name = "Fecha venta")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        [Display(Name = "Precio unitario")]
        public decimal PrecioUnitario { get; set; }

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        [Display(Name = "Valor total")]
        public decimal Total { get; set; }

        [Required, Display(Name = "Estado venta")]
        public int EstadoVenta { get; set; } = 1;

        [Display(Name = "Comprobante pago")]
        public string ComprobantePago { get; set; } = "";

        [Display(Name = "Envio notificación")]
        public int EnvioNotificacion { get; set; }

        [Display(Name = "Nombre cliente")]
        public string NombrePersona { get; set; } = "";

        public string CelularPersona { get; set; } = "";
        public int IdTipoDocumento { get; set; }

        [Display(Name = "Documento")]
        public string NumeroDocumento { get; set; } = "";

        [Display(Name = "Evento")]
        public string NombreEvento { get; set; } = "";

        [Display(Name = "Capacidad evento")]
        public int Capacidad { get; set; }

        [Display(Name = "Entradas disponibles")]
        public int Disponibles { get; set; }

        public long RowVersion { get; set; }

        public IEnumerable<SelectListItem> Eventos { get; set; } = [];
    }
}

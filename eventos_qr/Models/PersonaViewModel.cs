using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace eventos_qr.Models
{
    public class PersonaViewModel
    {
        public long IdPersona { get; set; }

        [Required, StringLength(150)]
        public string Nombres { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string Apellidos { get; set; } = string.Empty;

        [Required, Display(Name = "Tipo de documento")]
        public int TipoDocumento { get; set; }

        [Required, Display(Name = "Número de documento"), StringLength(10)]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string Celular { get; set; } = string.Empty;

        [EmailAddress, StringLength(150)]
        public string? Correo { get; set; }

        public DateTime FechaRegistro { get; set; }

        public bool Estado { get; set; } = true;

        public long RowVersion { get; set; }

        public string CodigoTipoDocumento { get; set; }
        public IEnumerable<SelectListItem> TiposDocumentos { get; set; } = [];
    }
}

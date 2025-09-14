using System.ComponentModel;

namespace eventos_qr.Entity.Enums
{
    public class Configuracion
    {
        public enum EstadoVenta
        {
            [Description("Pendiente")]
            Pendiente = 1,
            [Description("Aplicada")]
            Aplicada = 2,
            [Description("Rechazada")]
            Rechazada = 3,
            [Description("Eliminada")]
            Eliminada = 4,
        }
    }
}

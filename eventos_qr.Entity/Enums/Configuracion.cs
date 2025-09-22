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

        public enum EstadoBoleta
        {
            [Description("Activa")]
            Activa = 1,
            [Description("Usada")]
            Usada = 2,
            [Description("Anulada")]
            Anulada = 3
        }

        public enum TipoNotifiacion
        {
            [Description("No")]
            No = 0,
            [Description("Pendiente pago")]
            Pendiente = 1,
            [Description("Entregado")]
            Entregado = 2,
        }
    }
}

using eventos_qr.Entity.Dtos;
using eventos_qr.Entity.Models;

namespace eventos_qr.BLL.Mapper
{
    public class VentaMapper
    {
        public VentaDto VentaDtoMapper(VentasModel venta)
        {
            return new VentaDto
            {
                IdVenta = venta.IdVenta,
                IdPersona = venta.IdPersona,
                IdEvento = venta.IdEvento,
                Fecha = venta.FechaUtc,
                Cantidad = venta.Cantidad,
                Total = venta.Total,
                EstadoVenta = venta.EstadoVenta,
                ComprobantePago = venta.ComprobantePago,
                EnvioNotificacion = venta.EnvioNotificacion,
                RowVersion = venta.RowVersion,
                PrecioUnitario = venta.Evento != null ? venta.Evento.PrecioUnitario : 0,
                NombrePersona = venta.Persona != null ? $"{venta.Persona.Nombres} {venta.Persona.Apellidos}" : "",
                IdTipoDocumento = venta.Persona != null ? venta.Persona.TipoDocumento : 0,
                NumeroDocumento = venta.Persona != null ? venta.Persona.NumeroDocumento : "",
                NombreEvento = venta.Evento != null ? venta.Evento.Nombre : "",
                Capacidad = venta.Evento != null ? venta.Evento.Capacidad : 0,
                Disponibles = venta.Evento != null ? venta.Evento.Capacidad - venta.Cantidad : 0,
            };
        }

        public List<VentaDto> VentaDtoMapper(List<VentasModel> ventas)
        {
            return ventas.Select(venta => new VentaDto
            {
                IdVenta = venta.IdVenta,
                IdPersona = venta.IdPersona,
                IdEvento = venta.IdEvento,
                Fecha = venta.FechaUtc,
                Cantidad = venta.Cantidad,
                Total = venta.Total,
                EstadoVenta = venta.EstadoVenta,
                ComprobantePago = venta.ComprobantePago,
                EnvioNotificacion = venta.EnvioNotificacion,
                RowVersion = venta.RowVersion,
                PrecioUnitario = venta.Evento != null ? venta.Evento.PrecioUnitario : 0,
                NombrePersona = venta.Persona != null ? $"{venta.Persona.Nombres} {venta.Persona.Apellidos}" : "",
                IdTipoDocumento = venta.Persona != null ? venta.Persona.TipoDocumento : 0,
                NumeroDocumento = venta.Persona != null ? venta.Persona.NumeroDocumento : "",
                NombreEvento = venta.Evento != null ? venta.Evento.Nombre : "",
                Capacidad = venta.Evento != null ? venta.Evento.Capacidad : 0,
                Disponibles = venta.Evento != null ? venta.Evento.Capacidad - venta.Cantidad : 0,
            }).ToList();
        }

        public VentasModel VentaModelMapper(VentaDto venta)
        {
            return new VentasModel
            {
                IdVenta = venta.IdVenta,
                IdEvento = venta.IdEvento,
                IdPersona = venta.IdPersona,
                FechaUtc = venta.Fecha,
                Cantidad = venta.Cantidad,
                Total = venta.Total,
                EstadoVenta = venta.EstadoVenta,
                ComprobantePago = venta.ComprobantePago,
                EnvioNotificacion = venta.EnvioNotificacion,
            };
        }
    }
}

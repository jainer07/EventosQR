using eventos_qr.Entity.Dtos;
using eventos_qr.Models;

namespace eventos_qr.Mapper
{
    public class VentaMapper
    {
        public VentaViewModel VentaViewModelMapper(VentaDto venta)
        {
            return new VentaViewModel
            {
                IdVenta = venta.IdVenta,
                IdPersona = venta.IdPersona,
                IdEvento = venta.IdEvento,
                Fecha = venta.Fecha,
                Cantidad = venta.Cantidad,
                Total = venta.Total,
                EstadoVenta = venta.EstadoVenta,
                ComprobantePago = venta.ComprobantePago,
                EnvioNotificacion = venta.EnvioNotificacion,
                RowVersion = venta.RowVersion,
                PrecioUnitario = venta.PrecioUnitario,
                NombrePersona = venta.NombrePersona,
                IdTipoDocumento = venta.IdTipoDocumento,
                NumeroDocumento = venta.NumeroDocumento,
                NombreEvento = venta.NombreEvento,
                Capacidad = venta.Capacidad,
                Disponibles = venta.Disponibles,
                CelularPersona = venta.CelularPersona
            };
        }

        public List<VentaViewModel> VentaViewModelMapper(List<VentaDto> ventas)
        {
            return ventas.Select(venta => new VentaViewModel
            {
                IdVenta = venta.IdVenta,
                IdPersona = venta.IdPersona,
                IdEvento = venta.IdEvento,
                Fecha = venta.Fecha,
                Cantidad = venta.Cantidad,
                Total = venta.Total,
                EstadoVenta = venta.EstadoVenta,
                ComprobantePago = venta.ComprobantePago,
                EnvioNotificacion = venta.EnvioNotificacion,
                RowVersion = venta.RowVersion,
                PrecioUnitario = venta.PrecioUnitario,
                NombrePersona = venta.NombrePersona,
                IdTipoDocumento = venta.IdTipoDocumento,
                NumeroDocumento = venta.NumeroDocumento,
                NombreEvento = venta.NombreEvento,
                Capacidad = venta.Capacidad,
                Disponibles = venta.Disponibles,
                CelularPersona = venta.CelularPersona,
            }).ToList();
        }

        public VentaDto VentaDtoMapper(VentaViewModel venta)
        {
            return new VentaDto
            {
                IdVenta = venta.IdVenta,
                IdPersona = venta.IdPersona,
                IdEvento = venta.IdEvento,
                Fecha = venta.Fecha,
                Cantidad = venta.Cantidad,
                Total = venta.Cantidad * venta.PrecioUnitario,
                EstadoVenta = venta.EstadoVenta,
                ComprobantePago = venta.ComprobantePago,
                EnvioNotificacion = venta.EnvioNotificacion,
                RowVersion = venta.RowVersion,
                PrecioUnitario = venta.PrecioUnitario,
                NombrePersona = venta.NombrePersona,
                IdTipoDocumento = venta.IdTipoDocumento,
                NumeroDocumento = venta.NumeroDocumento,
                NombreEvento = venta.NombreEvento,
                Capacidad = venta.Capacidad,
                Disponibles = venta.Disponibles,
                CelularPersona = venta.CelularPersona,
            };
        }
    }
}

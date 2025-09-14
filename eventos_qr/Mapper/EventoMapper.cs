using eventos_qr.Entity.Dtos;
using eventos_qr.Models;

namespace eventos_qr.Mapper
{
    public class EventoMapper
    {
        public EventoViewModel EventoViewModelMapper(EventoDto evento)
        {
            var rvUtc = DateTime.SpecifyKind(evento.RowVersion, DateTimeKind.Utc);

            return new EventoViewModel
            {
                IdEvento = evento.IdEvento,
                Nombre = evento.Nombre,
                Fecha = evento.Fecha,
                Capacidad = evento.Capacidad,
                Disponibles = evento.Disponibles,
                PrecioUnitario = evento.PrecioUnitario,
                Vendidas = evento.Vendidas,
                RowVersion = rvUtc,
                RowVersionTicks = rvUtc.Ticks
            };
        }

        public List<EventoViewModel> EventoViewModelMapper(List<EventoDto> evento)
        {
            return evento.Select(e => new EventoViewModel
            {
                IdEvento = e.IdEvento,
                Nombre = e.Nombre,
                Fecha = e.Fecha,
                Capacidad = e.Capacidad,
                Disponibles = e.Disponibles,
                PrecioUnitario = e.PrecioUnitario,
                RowVersion = e.RowVersion,
                Vendidas = e.Vendidas
            }).ToList();
        }

        public EventoDto EventoDtoMapper(EventoViewModel evento)
        {
            var rowVersionUtc = new DateTime(evento.RowVersionTicks, DateTimeKind.Utc);

            return new EventoDto
            {
                IdEvento = evento.IdEvento,
                Nombre = evento.Nombre,
                Fecha = evento.Fecha,
                Capacidad = evento.Capacidad,
                Disponibles = evento.Disponibles,
                PrecioUnitario = evento.PrecioUnitario,
                Vendidas = evento.Vendidas,
                RowVersion = rowVersionUtc,
                RowVersionTicks = evento.RowVersionTicks
            };
        }
    }
}

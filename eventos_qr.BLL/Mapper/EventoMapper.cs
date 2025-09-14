using eventos_qr.Entity.Models;
using eventos_qr.Entity.Dtos;

namespace eventos_qr.BLL.Mapper
{
    public class EventoMapper
    {
        public EventoDto EventoDtoMapper(EventoModel evento)
        {
            return new EventoDto
            {
                IdEvento = evento.IdEvento,
                Nombre = evento.Nombre,
                Fecha = evento.Fecha,
                Capacidad = evento.Capacidad,
                Disponibles = evento.Disponibles,
                PrecioUnitario = evento.PrecioUnitario,
                Vendidas = evento.Vendidas,
                Estado = evento.Estado,
                RowVersion = DateTime.SpecifyKind(evento.RowVersion, DateTimeKind.Utc),
                RowVersionTicks = DateTime.SpecifyKind(evento.RowVersion, DateTimeKind.Utc).Ticks
            };
        }

        public List<EventoDto> EventoDtoMapper(List<EventoModel> evento)
        {
            return evento.Select(e => new EventoDto
            {
                IdEvento = e.IdEvento,
                Nombre = e.Nombre,
                Fecha = e.Fecha,
                Capacidad = e.Capacidad,
                Disponibles = e.Disponibles,
                PrecioUnitario = e.PrecioUnitario,
                Vendidas = e.Vendidas,
                Estado = e.Estado,
            }).ToList();
        }

        public EventoModel EventoModelMapper(EventoDto evento)
        {
            return new EventoModel
            {
                IdEvento = evento.IdEvento,
                Nombre = evento.Nombre,
                Fecha = evento.Fecha,
                Capacidad = evento.Capacidad,
                Disponibles = evento.Disponibles,
                PrecioUnitario = evento.PrecioUnitario,
                Estado = evento.Estado,
                RowVersion = evento.RowVersion,
                Vendidas = evento.Vendidas
            };
        }
    }
}

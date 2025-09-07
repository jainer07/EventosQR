using eventos_qr.BLL.Models;
using eventos_qr.DAL.Models;

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
                Capacidad = evento.Capacidad,
                Fecha = evento.Fecha,
                PrecioUnitario = evento.PrecioUnitario,
                Vendidas = evento.Vendidas,
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
                Capacidad = e.Capacidad,
                Fecha = e.Fecha,
                PrecioUnitario = e.PrecioUnitario,
                Vendidas = e.Vendidas
            }).ToList();
        }

        public EventoModel EventoModelMapper(EventoDto evento)
        {
            return new EventoModel
            {
                IdEvento = evento.IdEvento,
                Nombre = evento.Nombre,
                Capacidad = evento.Capacidad,
                Fecha = evento.Fecha,
                PrecioUnitario = evento.PrecioUnitario,
                RowVersion = evento.RowVersion,
                Vendidas = evento.Vendidas
            };
        }
    }
}

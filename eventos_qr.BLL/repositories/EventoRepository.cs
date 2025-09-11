using eventos_qr.BLL.Contracts;
using eventos_qr.BLL.Mapper;
using eventos_qr.BLL.Models;
using eventos_qr.DAL.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eventos_qr.BLL.repositories
{
    public class EventoRepository(EventoQueryService eventoQuery) : IEventoRepository
    {
        private readonly EventoQueryService _eventoQuery = eventoQuery;
        private readonly EventoMapper _mapper = new();

        public async Task<EventoDto?> ObtenerAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("El ID del evento debe ser un número positivo.", nameof(id));

                var evento = await _eventoQuery.ObtenerAsync(id);

                if (evento == null)
                    return null;

                return _mapper.EventoDtoMapper(evento);
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error en un log si es necesario
                throw new ApplicationException($"Error al obtener el evento con ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<EventoDto>> ListarAsync()
        {
            try
            {
                var eventos = await _eventoQuery.ListarAsync();

                return _mapper.EventoDtoMapper(eventos);
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error en un log si es necesario
                throw new ApplicationException("Error al listar los eventos: " + ex.Message, ex);
            }
        }

        public async Task<int> CrearAsync(EventoDto evento)
        {
            if (evento is null)
                throw new ArgumentNullException(nameof(evento), "El evento no puede ser nulo.");

            var existe = await ObtenerAsync(evento.IdEvento);
            if (existe != null)
                throw new InvalidOperationException($"El evento con ID {evento.IdEvento} ya existe.");

            var entity = _mapper.EventoModelMapper(evento);

            try
            {
                return await _eventoQuery.CrearAsync(entity);
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error en un log si es necesario
                throw new ApplicationException("Error al crear el evento: " + ex.Message, ex);
            }
        }

        public async Task<bool> ActualizarAsync(int id, EventoDto evento)
        {
            if (evento is null)
                throw new ArgumentNullException(nameof(evento), "El evento no puede ser nulo.");

            if (id != evento.IdEvento)
                throw new ArgumentException("El ID del evento no coincide con el ID proporcionado.", nameof(id));

            var existe = await ObtenerAsync(id) ?? throw new KeyNotFoundException($"El evento con ID {id} no existe.");

            existe.Nombre = evento.Nombre;
            existe.Fecha = evento.Fecha;
            existe.Capacidad = evento.Capacidad;
            existe.PrecioUnitario = evento.PrecioUnitario;
            existe.Vendidas = evento.Vendidas;

            var entity = _mapper.EventoModelMapper(evento);

            try
            {
                return await _eventoQuery.ActualizarAsync(entity, evento.RowVersion);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Mensaje claro para conflicto de concurrencia
                throw new ApplicationException("Conflicto al actualizar el evento. Es posible que los datos hayan sido modificados por otro usuario.", ex);
            }
            catch (Exception ex)
            {
                // Evita concatenar ex.Message; deja que el detalle viva en InnerException
                throw new ApplicationException("Error al actualizar el evento.", ex);
            }
        }

        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("El ID del evento debe ser un número positivo.", nameof(id));

                var existe = await ObtenerAsync(id) ?? throw new KeyNotFoundException($"El evento con ID {id} no existe.");

                var entity = _mapper.EventoModelMapper(existe);
                return await _eventoQuery.EliminarAsync(entity);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al eliminar el evento: " + ex.Message, ex);
            }
        }
    }
}

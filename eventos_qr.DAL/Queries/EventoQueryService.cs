using eventos_qr.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace eventos_qr.DAL.Queries
{
    public class EventoQueryService(EventosQR_Contex ctx)
    {
        private readonly EventosQR_Contex _ctx = ctx;

        public async Task<EventoModel> ObtenerAsync(int id)
        {
            return await _ctx.EventoModels.FirstOrDefaultAsync(e => e.IdEvento == id);
        }

        public async Task<List<EventoModel>> ListarAsync()
        {
            return await _ctx.EventoModels.OrderBy(e => e.Fecha)
                                          .ToListAsync();
        }

        public async Task<int> CrearAsync(EventoModel evento)
        {
            _ctx.EventoModels.Add(evento);
            await _ctx.SaveChangesAsync();
            return evento.IdEvento;
        }

        public async Task<bool> ActualizarAsync(EventoModel evento, DateTime rowVersion)
        {
            _ctx.Entry(evento).Property("RowVersion").OriginalValue = rowVersion;

            try
            {
                await _ctx.SaveChangesAsync();
                return true; // Actualización exitosa
            }
            catch (DbUpdateConcurrencyException)
            {
                return false; // Conflicto de concurrencia
            }
        }

        public async Task<bool> EliminarAsync(EventoModel evento)
        {
            try
            {
                _ctx.EventoModels.Remove(evento);
                await _ctx.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false; // Conflicto de concurrencia
            }
            catch (Exception)
            {
                return false; // Otro error
            }

        }
    }
}

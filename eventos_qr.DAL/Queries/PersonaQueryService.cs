using eventos_qr.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace eventos_qr.DAL.Queries
{
    public class PersonaQueryService(EventosQR_Contex ctx)
    {
        private readonly EventosQR_Contex _ctx = ctx;

        public async Task<PersonaModel?> ObtenerAsync(long id, CancellationToken ct)
        {
            return await _ctx.PersonaModels.AsNoTracking().FirstOrDefaultAsync(p => p.IdPersona == id, ct);
        }

        public async Task<List<PersonaModel>> ListarAsync(string? filtro, int page, int pageSize, CancellationToken ct)
        {
            var query = _ctx.PersonaModels.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                query = query.Where(p => p.Nombres.Contains(filtro) || p.Apellidos.Contains(filtro)
                || p.NumeroDocumento.Contains(filtro) || (p.Correo ?? "").Contains(filtro));
            }
            return await query.OrderBy(x => x.Apellidos).ThenBy(x => x.Nombres)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        }

        public async Task<PersonaModel?> GetByDocumentoAsync(int tipo, string numero, CancellationToken ct)
        {
            return await _ctx.PersonaModels.FirstOrDefaultAsync(p => p.TipoDocumento == tipo && p.NumeroDocumento == numero, ct);
        }

        public async Task<int> CountAsync(string? filtro, CancellationToken ct)
        {
            var query = _ctx.PersonaModels.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                query = query.Where(x => x.Nombres.Contains(filtro) || x.Apellidos.Contains(filtro)
                || x.NumeroDocumento.Contains(filtro) || (x.Correo ?? "").Contains(filtro));
            }
            return await query.CountAsync(ct);
        }

        public async Task<int> CrearAsync(PersonaModel entity, CancellationToken ct)
        {
            _ctx.PersonaModels.Add(entity);
            await _ctx.SaveChangesAsync(ct);

            return (int)entity.IdPersona;
        }

        public async Task<bool> ActualizarAsync(PersonaModel entity, long rowVersion, CancellationToken ct)
        {
            _ctx.Entry(entity).Property("RowVersion").OriginalValue = rowVersion;
            entity.RowVersion = rowVersion + 1;
            try
            {
                _ctx.PersonaModels.Update(entity);
                await _ctx.SaveChangesAsync(ct);

                return true; // Actualización exitosa
            }
            catch (DbUpdateConcurrencyException)
            {
                return false; // Conflicto de concurrencia
            }
            catch (Exception ex)
            {
                return false; // Otro error
            }
        }

        public async Task<bool> EliminarAsync(PersonaModel entity, CancellationToken ct)
        {
            try
            {
                _ctx.PersonaModels.Remove(entity);
                await _ctx.SaveChangesAsync(ct);

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

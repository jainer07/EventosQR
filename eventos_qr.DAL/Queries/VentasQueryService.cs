using eventos_qr.Entity.Dtos;
using eventos_qr.Entity.Models;
using Microsoft.EntityFrameworkCore;


namespace eventos_qr.DAL.Queries
{
    public class VentasQueryService(EventosQR_Contex ctx)
    {
        private readonly EventosQR_Contex _ctx = ctx;

        public async Task<VentaDto?> ObtenerAsync(long id, CancellationToken ct)
        {
            var venta = await _ctx.VentasModels
                .Include(x => x.Evento)
                .Include(x => x.Persona)
                .AsNoTracking()
                .Select(venta => new VentaDto
                {
                    IdVenta = venta.IdVenta,
                    IdEvento = venta.IdEvento,
                    IdPersona = venta.IdPersona,
                    Fecha = venta.FechaUtc,
                    Cantidad = venta.Cantidad,
                    Total = venta.Total,
                    EstadoVenta = venta.EstadoVenta,
                    ComprobantePago = venta.ComprobantePago ?? "",
                    EnvioNotificacion = venta.EnvioNotificacion,
                    RowVersion = venta.RowVersion,
                    PrecioUnitario = venta.Evento.PrecioUnitario,
                    NombrePersona = venta.Persona != null ? (venta.Persona.Nombres + " " + venta.Persona.Apellidos) : "",
                    IdTipoDocumento = venta.Persona != null ? venta.Persona.TipoDocumento : 0,
                    NumeroDocumento = venta.Persona != null ? venta.Persona.NumeroDocumento : "",
                    CelularPersona = venta.Persona != null ? venta.Persona.Celular : "",
                    NombreEvento = venta.Evento != null ? venta.Evento.Nombre : "",
                    Capacidad = venta.Evento != null ? venta.Evento.Capacidad : 0,
                    Disponibles = venta.Evento.Disponibles,
                })
                .FirstOrDefaultAsync(x => x.IdVenta == id, ct);

            return venta;
        }

        public async Task<List<VentaDto>> ListarAsync(string? filtro, int page, int pageSize, CancellationToken ct)
        {
            var query = _ctx.VentasModels.Include(x => x.Evento).Include(x => x.Persona).AsQueryable();
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                query = query.Where(x => (x.Persona != null && (x.Persona.Nombres + " " + x.Persona.Apellidos).Contains(filtro))
                    || (x.Persona != null && x.Persona.NumeroDocumento.Contains(filtro))
                    || x.Evento!.Nombre.Contains(filtro));
            }

            return await query.Select(venta => new VentaDto
            {
                IdVenta = venta.IdVenta,
                IdEvento = venta.IdEvento,
                IdPersona = venta.IdPersona,
                Fecha = venta.FechaUtc,
                Cantidad = venta.Cantidad,
                Total = venta.Total,
                EstadoVenta = venta.EstadoVenta,
                ComprobantePago = venta.ComprobantePago ?? "",
                EnvioNotificacion = venta.EnvioNotificacion,
                RowVersion = venta.RowVersion,
                PrecioUnitario = venta.Evento.PrecioUnitario,
                NombrePersona = venta.Persona != null ? (venta.Persona.Nombres + " " + venta.Persona.Apellidos) : "",
                IdTipoDocumento = venta.Persona != null ? venta.Persona.TipoDocumento : 0,
                NumeroDocumento = venta.Persona != null ? venta.Persona.NumeroDocumento : "",
                CelularPersona = venta.Persona != null ? venta.Persona.Celular : "",
                NombreEvento = venta.Evento != null ? venta.Evento.Nombre : "",
                Capacidad = venta.Evento != null ? venta.Evento.Capacidad : 0,
                Disponibles = venta.Evento.Disponibles,
            })
            .OrderByDescending(x => x.Fecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        }

        public async Task<int> CountAsync(string? filtro, CancellationToken ct)
        {
            var query = _ctx.VentasModels.Include(x => x.Evento).Include(x => x.Persona).AsQueryable();
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                query = query.Where(x => (x.Persona != null && (x.Persona.Nombres + " " + x.Persona.Apellidos).Contains(filtro))
                    || (x.Persona != null && x.Persona.NumeroDocumento.Contains(filtro))
                    || x.Evento!.Nombre.Contains(filtro));
            }

            return await query.CountAsync(ct);
        }

        public async Task<int> CrearAsync(VentasModel entity, CancellationToken ct)
        {
            _ctx.VentasModels.Add(entity);
            await _ctx.SaveChangesAsync(ct);

            return (int)entity.IdPersona;
        }

        public async Task<bool> ActualizarAsync(VentasModel entity, long rowVersion, CancellationToken ct)
        {
            _ctx.Entry(entity).Property("RowVersion").OriginalValue = rowVersion;
            entity.RowVersion = rowVersion + 1;
            try
            {
                _ctx.VentasModels.Update(entity);
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

        public async Task<bool> DeleteAsync(VentasModel entity, CancellationToken ct)
        {
            try
            {
                _ctx.VentasModels.Remove(entity);
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

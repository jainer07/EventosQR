using eventos_qr.Entity.Models;
using Microsoft.EntityFrameworkCore;

namespace eventos_qr.DAL.Queries
{
    public class BoletaQueryService(EventosQR_Contex ctx)
    {
        private readonly EventosQR_Contex _ctx = ctx;

        public async Task<BoletaModel?> FindByCodeAsync(string code, CancellationToken ct)
        {
            return await _ctx.BoletaModels
                .Include(v => v.Venta).ThenInclude(e => e.Evento)
                .Include(v => v.Venta).ThenInclude(e => e.Persona)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.CodigoQr == code, ct);
        }

        public Task<(int codigo, string mensaje, BoletaModel? boleta)> MarkAsUsedAsync(string code, string? gate, string? deviceId, string? operatorId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<(int codigo, string mensaje)> RevertUseAsync(string code, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}

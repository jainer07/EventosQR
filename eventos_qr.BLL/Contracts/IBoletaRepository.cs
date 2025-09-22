using eventos_qr.Entity.Dtos;
using eventos_qr.Entity.Models;

namespace eventos_qr.BLL.Contracts
{
    public interface IBoletaRepository
    {
        Task<RespuestaType> AplicarVentaAsync(long idVenta, bool aplicar, CancellationToken ct);
        Task<BoletaDto?> FindByCodeAsync(string code, CancellationToken ct);
        Task<(int codigo, string mensaje, BoletaDto? boleta)> MarkAsUsedAsync(string code, long? operatorId, CancellationToken ct);

        Task<(int codigo, string mensaje)> RevertUseAsync(string code, CancellationToken ct);
    }
}

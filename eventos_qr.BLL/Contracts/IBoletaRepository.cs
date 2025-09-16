using eventos_qr.Entity.Dtos;

namespace eventos_qr.BLL.Contracts
{
    public interface IBoletaRepository
    {
        Task<RespuestaType> AplicarVentaAsync(long idVenta, bool aplicar, CancellationToken ct);
    }
}

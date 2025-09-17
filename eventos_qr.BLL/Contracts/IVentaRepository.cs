using eventos_qr.Entity.Dtos;

namespace eventos_qr.BLL.Contracts
{
    public interface IVentaRepository
    {
        Task<VentaDto?> ObtenerAsync(long id, CancellationToken ct);
        Task<(List<VentaDto> Items, int Total)> ListarAsync(string? filtro, int page, int pageSize, CancellationToken ct);
        Task<int> CountAsync(string? filtro, CancellationToken ct);
        Task<RespuestaType> CrearAsync(VentaDto entity, CancellationToken ct);
        Task<RespuestaType> ActualizarAsync(long id, VentaDto entity, CancellationToken ct);
        Task<RespuestaType> DeleteAsync(long id, CancellationToken ct);
    }
}

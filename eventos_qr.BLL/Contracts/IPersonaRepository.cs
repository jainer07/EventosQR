using eventos_qr.Entity.Dtos;

namespace eventos_qr.BLL.Contracts
{
    public interface IPersonaRepository
    {
        Task<PersonaDto?> ObtenerAsync(long id, CancellationToken ct);
        Task<PersonaDto?> GetByDocumentoAsync(int tipo, string numero, CancellationToken ct);
        Task<(List<PersonaDto> Items, int Total)> ListarAsync(string? filtro, int page, int pageSize, CancellationToken ct);
        Task<int> CountAsync(string? filtro, CancellationToken ct);
        Task<RespuestaType> CrearAsync(PersonaDto entity, CancellationToken ct);
        Task<RespuestaType> ActualizarAsync(long id, PersonaDto entity, CancellationToken ct);
        Task<RespuestaType> EliminarAsync(long id, CancellationToken ct);
    }
}

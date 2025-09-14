using eventos_qr.Entity.Dtos;

namespace eventos_qr.BLL.Contracts
{
    public interface IEventoRepository
    {
        Task<EventoDto?> ObtenerAsync(int id);
        Task<IEnumerable<EventoDto>> ListarAsync();
        Task<int> CrearAsync(EventoDto evento);
        Task<bool> ActualizarAsync(int id, EventoDto evento);
        Task<bool> EliminarAsync(int id);
    }
}

using eventos_qr.BLL.Models;

namespace eventos_qr.BLL.Contracts
{
    public interface IEventoRepository
    {
        Task<int> CrearAsync(EventoDto evento);
        Task<EventoDto?> ObtenerAsync(int id);
        Task<IEnumerable<EventoDto>> ListarAsync();
        Task<bool> ActualizarAsync(int id, EventoDto evento);
        Task<bool> EliminarAsync(int id);
    }
}

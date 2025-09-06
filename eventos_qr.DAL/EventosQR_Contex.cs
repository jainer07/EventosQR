using Microsoft.EntityFrameworkCore;

namespace eventos_qr.DAL
{
    public class EventosQR_Contex : DbContext
    {
        public EventosQR_Contex(DbContextOptions<EventosQR_Contex> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}

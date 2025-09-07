using eventos_qr.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace eventos_qr.DAL
{
    public class EventosQR_Contex : DbContext
    {
        public EventosQR_Contex(DbContextOptions<EventosQR_Contex> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var e = modelBuilder.Entity<EventoModel>();
            e.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
            e.Property(e => e.RowVersion)
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<EventoModel> EventoModels { get; private set; }
    }
}

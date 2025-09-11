using eventos_qr.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace eventos_qr.DAL
{
    public class EventosQR_Contex : DbContext
    {
        public EventosQR_Contex(DbContextOptions<EventosQR_Contex> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EventoModel>(e =>
            {
                e.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
                e.Property(e => e.RowVersion)
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<PersonaModel>(e =>
            {
                e.Property(e => e.RowVersion)
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();
            });
        }

        public DbSet<EventoModel> EventoModels { get; private set; }
        public DbSet<PersonaModel> PersonaModels { get; private set; }
    }
}

using eventos_qr.Entity.Models;
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

            modelBuilder.Entity<VentasModel>(e =>
            {
                e.Property(x => x.Total).HasPrecision(18, 2);
                e.Property(e => e.RowVersion)
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

                e.HasOne(x => x.Evento)
                 .WithMany()
                 .HasForeignKey(x => x.IdEvento);

                e.HasOne(x => x.Persona)
                 .WithMany(p => p.Ventas!)
                 .HasForeignKey(x => x.IdPersona);
            });
        }

        public DbSet<EventoModel> EventoModels { get; private set; }
        public DbSet<PersonaModel> PersonaModels { get; private set; }
        public DbSet<VentasModel> VentasModels { get; set; }
    }
}

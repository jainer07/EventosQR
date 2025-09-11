using eventos_qr.BLL.Contracts;
using eventos_qr.BLL.repositories;
using eventos_qr.DAL;
using eventos_qr.DAL.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eventos_qr.BLL
{
    public static class Infraestructure
    {
        public static void AddRegisterEventosQR_DbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var main_cs = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<EventosQR_Contex>(builder =>
            {
                builder.UseMySql(main_cs, ServerVersion.AutoDetect(main_cs),
                    my => my.EnableRetryOnFailure());
            }, ServiceLifetime.Transient);
            //services.AddHttpClient();
            //services.AddHttpContextAccessor();
            services.AddRepositories();
        }

        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IEventoRepository, EventoRepository>();
            services.AddScoped<IPersonaRepository, PersonaRepository>();

            // Query services
            services.AddScoped<EventoQueryService>();
            services.AddScoped<PersonaQueryService>();
        }
    }
}

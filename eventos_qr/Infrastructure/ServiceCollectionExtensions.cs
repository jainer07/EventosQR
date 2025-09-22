using Microsoft.OpenApi.Models;

namespace eventos_qr.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiInfrastructure(this IServiceCollection services, IConfiguration cfg)
        {
            // CORS (ajusta orígenes según tu lector/Front)
            services.AddCors(opt =>
            {
                opt.AddPolicy("default", p => p
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("https://kombat-fight-entrada.up.railway.app"));
            });

            // Swagger (lo encendemos solo en Dev en el pipeline)
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "eventos_qr API", Version = "v1" });
            });

            return services;
        }
    }
}

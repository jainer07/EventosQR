using eventos_qr.Api;
using eventos_qr.BLL;
using eventos_qr.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRegisterEventosQR_DbContext(builder.Configuration);

// Infra de API (Swagger, CORS, DI repos, etc.)
builder.Services.AddApiInfrastructure(builder.Configuration);

var app = builder.Build();

// Pipeline unificado (maneja swagger en dev, estáticos, routing, cors, auth)
app.UseAppPipeline();

// Endpoints de la API (QR/boletas)
app.MapBoletasEndpoints();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

using eventos_qr.BLL.Contracts;
using eventos_qr.Entity.Dtos;

namespace eventos_qr.Api
{
    public static class BoletasEndpoints
    {
        public static IEndpointRouteBuilder MapBoletasEndpoints(this IEndpointRouteBuilder routes)
        {
            var api = routes.MapGroup("/api/boletas").WithTags("Boletas");

            // GET /api/boletas/{code}  (pre-check)
            api.MapGet("/{code}", async (string code, IBoletaRepository repo, CancellationToken ct) =>
            {
                var boleta = await repo.FindByCodeAsync(code, ct);
                if (boleta is null) return Results.NotFound(new { Codigo = 99, Mensaje = "Boleta no encontrada" });

                var resp = new BoletaScanResponse
                {
                    Codigo = 0,
                    Mensaje = "OK",
                    Code = code,
                    Estado = boleta.Estado,
                    FechaUso = boleta.FechaUsoUtc,
                    Evento = boleta.NombreEvento,
                    FechaEvento = boleta.FechaEvento,
                    Titular = boleta.NombrePersona,
                };

                return Results.Ok(resp);
            });

            // POST /api/boletas/scan  (marcar como usada o solo validar)
            api.MapPost("/scan", async (BoletaScanRequest req, IBoletaRepository repo, CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(req.Code))
                    return Results.BadRequest(new { Codigo = 98, Mensaje = "Code es requerido" });

                if (req.ValidateOnly)
                {
                    var boletaDB = await repo.FindByCodeAsync(req.Code, ct);
                    if (boletaDB is null) return Results.NotFound(new { Codigo = 99, Mensaje = "Boleta no encontrada" });

                    return Results.Ok(new BoletaScanResponse
                    {
                        Codigo = 0,
                        Mensaje = "VALID",
                        Code = req.Code,
                        Estado = boletaDB.Estado,
                        FechaUso = boletaDB.FechaUsoUtc,
                        Evento = boletaDB.NombreEvento,
                        FechaEvento = boletaDB.FechaEvento,
                        Titular = boletaDB.NombrePersona,
                    });
                }

                var (codigo, mensaje, boleta) = await repo.MarkAsUsedAsync(req.Code, req.OperatorId, ct);

                if (codigo == 99) return Results.NotFound(new { Codigo = codigo, Mensaje = mensaje });
                if (codigo is 10 or 11 or 12) return Results.Conflict(new { Codigo = codigo, Mensaje = mensaje });

                return Results.Ok(new BoletaScanResponse
                {
                    Codigo = codigo,
                    Mensaje = mensaje,
                    Code = req.Code,
                    Estado = boleta!.Estado,
                    FechaUso = boleta.FechaUsoUtc,
                    Evento = boleta.NombreEvento,
                    FechaEvento = boleta.FechaEvento,
                    Titular = boleta.NombrePersona,
                });
            });

            // (Opcional) POST /api/boletas/{code}/revert
            api.MapPost("/{code}/revert", async (string code, IBoletaRepository repo, CancellationToken ct) =>
            {
                var (codigo, mensaje) = await repo.RevertUseAsync(code, ct);
                if (codigo == 99) return Results.NotFound(new { Codigo = codigo, Mensaje = mensaje });
                if (codigo != 0) return Results.BadRequest(new { Codigo = codigo, Mensaje = mensaje });
                return Results.Ok(new { Codigo = 0, Mensaje = "OK" });
            });

            return routes;
        }
    }
}

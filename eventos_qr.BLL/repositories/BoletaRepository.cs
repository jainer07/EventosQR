using eventos_qr.BLL.Contracts;
using eventos_qr.BLL.Helpers;
using eventos_qr.BLL.Mapper;
using eventos_qr.DAL;
using eventos_qr.DAL.Queries;
using eventos_qr.Entity.Dtos;
using eventos_qr.Entity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using SkiaSharp;
using System.IO.Compression;
using static eventos_qr.Entity.Enums.Configuracion;

namespace eventos_qr.BLL.repositories
{
    public class BoletaRepository(EventosQR_Contex ctx, BoletaQueryService boletaQuery, IConfiguration configuration) : IBoletaRepository
    {
        private readonly EventosQR_Contex _ctx = ctx;
        private readonly BoletaQueryService _boletaQuery = boletaQuery;
        private readonly IConfiguration _configuration = configuration;
        private readonly BoletaMapper _mapper = new();

        public async Task<RespuestaType> AplicarVentaAsync(long idVenta, bool aplicar, CancellationToken ct)
        {
            var resp = new RespuestaType { Codigo = 99, Mensaje = "Error al aplicar venta" };

            var strategy = _ctx.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _ctx.Database.BeginTransactionAsync(ct);
                try
                {
                    var venta = await _ctx.VentasModels
                        .Include(v => v.Evento)
                        .Include(v => v.Persona)
                        .FirstOrDefaultAsync(v => v.IdVenta == idVenta, ct);

                    if (venta == null) { resp.Mensaje = "La venta no existe"; return; }

                    if (venta.EstadoVenta == (int)EstadoVenta.Eliminada)
                    { resp.Mensaje = "La venta está eliminada"; return; }

                    if (venta.EstadoVenta == (int)EstadoVenta.Aplicada && aplicar)
                    { resp.Mensaje = "La venta ya está aplicada"; return; }

                    if (venta.EstadoVenta == (int)EstadoVenta.Rechazada && !aplicar)
                    { resp.Mensaje = "La venta ya está rechazada"; return; }

                    // Si RECHAZA: devolvemos cupos
                    if (!aplicar)
                    {
                        await RechazarVenta(venta, ct);

                        await tx.CommitAsync(ct);
                        resp.Codigo = 0;
                        resp.Mensaje = "Venta rechazada correctamente";
                        return;
                    }

                    var (boletas, zipPath) = AplicarVenta(venta);
                    if (boletas.Count > 0)
                    {
                        await _ctx.AddRangeAsync(boletas, ct);

                        try
                        {
                            await _ctx.SaveChangesAsync(ct);
                            await tx.CommitAsync(ct);
                        }
                        catch (DbUpdateException dbEx)
                        {
                            await tx.RollbackAsync(ct);
                            var inner = dbEx.InnerException?.Message ?? dbEx.Message;
                            resp.Codigo = 99;
                            resp.Mensaje = $"No se pudo aplicar la venta (DB): {inner}";
                            return;
                        }

                        var waFinal = CrearNotificacion(venta, boletas);
                        if (!string.IsNullOrEmpty(waFinal))
                        {
                            resp.Codigo = 0;
                            resp.Mensaje = "Venta aplicada correctamente.";
                            var responseData = new
                            {
                                WhatsappUrl = waFinal,
                                ZipPath = zipPath
                            };
                            resp.Data = System.Text.Json.JsonSerializer.Serialize(responseData);
                        }
                        else
                        {
                            resp.Codigo = 1;
                            resp.Mensaje = "no se logron enviar la notifiación";
                            var responseData = new { ZipPath = zipPath };
                            resp.Data = System.Text.Json.JsonSerializer.Serialize(responseData);
                        }
                    }
                    else
                    {
                        await tx.RollbackAsync(ct);
                        resp.Codigo = 1;
                        resp.Mensaje = "No se pudo geerar la boleta.";
                    }
                }
                catch (Exception)
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }

            });

            return resp;
        }

        public async Task<BoletaDto?> FindByCodeAsync(string code, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                    return null;

                var boleta = await _boletaQuery.FindByCodeAsync(code, ct);


                if (boleta == null)
                    return null;

                boleta.FechaGeneracionUtc = UtilitiesHelper.ToBogotaFromUtc(boleta.FechaGeneracionUtc);
                boleta.FechaUsoUtc = boleta.FechaUsoUtc != null ? UtilitiesHelper.ToBogotaFromUtc(boleta.FechaUsoUtc.Value) : null;

                if (boleta.Venta != null)
                {
                    boleta.Venta.FechaUtc = UtilitiesHelper.ToBogotaFromUtc(boleta.Venta.FechaUtc);
                    if (boleta.Venta.Evento != null)
                    {
                        boleta.Venta.Evento.Fecha = UtilitiesHelper.ToBogotaFromUtc(boleta.Venta.Evento.Fecha);
                    }
                }

                return _mapper.BoletaDtoMapper(boleta);
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error en un log si es necesario
                throw new ApplicationException($"Error al obtener la boleta con ID {code}: {ex.Message}", ex);
            }
        }

        public async Task<(int codigo, string mensaje, BoletaDto? boleta)> MarkAsUsedAsync(string code, long? operatorId, CancellationToken ct)
        {
            var boleta = await FindByCodeAsync(code, ct);

            if (boleta is null) return (99, "Boleta no encontrada", null);

            if (!boleta.Estado)
                return (10, "Boleta ya fue usada anteriormente", boleta);

            var entity = _mapper.BoletaModelMapper(boleta);
            // Marcar uso
            entity.Estado = false;
            entity.FechaUsoUtc = DateTime.UtcNow;
            entity.OperatorId = operatorId;

            var original = boleta.RowVersion;
            entity.RowVersion = original + 1;

            _ctx.BoletaModels.Attach(entity);
            _ctx.Entry(entity).Property(e => e.Estado).IsModified = true;
            _ctx.Entry(entity).Property(e => e.FechaUsoUtc).IsModified = true;
            _ctx.Entry(entity).Property(e => e.OperatorId).IsModified = true;
            _ctx.Entry(entity).Property("RowVersion").OriginalValue = original;
            _ctx.Entry(entity).Property(e => e.RowVersion).IsModified = true;

            try
            {
                await _ctx.SaveChangesAsync(ct);

                boleta.RowVersion = entity.RowVersion;
                boleta.FechaUsoUtc = UtilitiesHelper.ToBogotaFromUtc(entity.FechaUsoUtc.Value);
                return (0, "Boleta usada correctamente", boleta);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return (12, "Conflicto de concurrencia. Intenta de nuevo.", null);
            }
        }

        public async Task<(int codigo, string mensaje)> RevertUseAsync(string code, CancellationToken ct)
        {
            var boleta = await FindByCodeAsync(code, ct);
            if (boleta is null) return (99, "Boleta no encontrada");

            if (boleta.Estado)
                return (13, "La boleta no está en estado 'Usada'");

            boleta.Estado = true;
            boleta.FechaUsoUtc = null;
            boleta.OperatorId = null;

            var entity = _mapper.BoletaModelMapper(boleta);
            entity.RowVersion += 1;

            _ctx.BoletaModels.Attach(entity);
            _ctx.Entry(entity).Property(e => e.Estado).IsModified = true;
            _ctx.Entry(entity).Property(e => e.FechaUsoUtc).IsModified = true;
            _ctx.Entry(entity).Property(e => e.OperatorId).IsModified = true;
            _ctx.Entry(entity).Property("RowVersion").OriginalValue = boleta.RowVersion;
            _ctx.Entry(entity).Property(e => e.RowVersion).IsModified = true;

            try
            {
                await _ctx.SaveChangesAsync(ct);
                return (0, "Uso revertido");
            }
            catch (DbUpdateConcurrencyException)
            {
                return (12, "Conflicto de concurrencia. Intenta de nuevo.");
            }
        }

        private async Task RechazarVenta(VentasModel venta, CancellationToken ct)
        {
            var ev = venta.Evento!;
            ev.Disponibles += venta.Cantidad;
            ev.Vendidas -= venta.Cantidad;
            if (ev.Vendidas < 0) ev.Vendidas = 0;
            if (ev.Disponibles > ev.Capacidad) ev.Disponibles = ev.Capacidad;

            venta.EstadoVenta = (int)EstadoVenta.Rechazada;

            await _ctx.SaveChangesAsync(ct);
        }

        private (List<BoletaModel>, string) AplicarVenta(VentasModel venta)
        {
            try
            {
                venta.EstadoVenta = (int)EstadoVenta.Aplicada;

                // Generar N boletas (una por entrada)
                var baseUrlPublica = _configuration["Archivos:BaseUrlPublica"]; // ej: https://tudominio.com
                var rootBoletas = _configuration["Archivos:RaizBoletas"];   // ej: wwwroot/boletas
                var rootFonts = _configuration["Archivos:RaizFonts"];   // ej: wwwroot/boletas
                var plantillaPath = Path.Combine(rootBoletas, "Plantilla_Boleta.jpg"); // ya la subiste a wwwroot/boletas
                var fontPath = Path.Combine(rootFonts, "NotoSans.ttf");

                Directory.CreateDirectory(rootBoletas);

                var dirVenta = Path.Combine(rootBoletas, venta.IdVenta.ToString());
                Directory.CreateDirectory(dirVenta);

                var boletas = new List<BoletaModel>();
                var rutasImagenes = new List<string>();

                for (int i = 1; i <= venta.Cantidad; i++)
                {
                    var numero = GenerarNumeroBoleta(venta.IdVenta, i);       // ej: VF-2025-000001-01
                    var token = Guid.NewGuid().ToString("N");                 // código seguro
                    var payload = $"{venta.IdVenta}|{token}"; // o data que prefieras

                    var fileName = $"{numero}.jpg";
                    var outPath = Path.Combine(dirVenta, fileName);

                    // 1) Genera QR (PNG in-memory)
                    var qrPng = GenerarQrPng(token);

                    // Render sobre plantilla
                    GenerarBoleta(plantillaPath, fontPath, new BoletaOverlayData
                    {
                        NumeroBoleta = numero,
                        QrPayload = payload
                    }, outPath,
                    qrSizePx: 500,
                    safeBottom: 100,
                    jpegQuality: 92);

                    rutasImagenes.Add(outPath);

                    boletas.Add(new BoletaModel
                    {
                        IdVenta = venta.IdVenta,
                        NumeroBoleta = numero,
                        CodigoQr = payload,
                        UrlImagen = "",
                        FechaGeneracionUtc = DateTime.UtcNow,
                        Estado = true
                    });
                }

                // Crear el archivo ZIP
                var zipPath = CrearArchivoZip(venta, rutasImagenes);

                return (boletas, zipPath);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string CrearArchivoZip(VentasModel venta, List<string> rutasImagenes)
        {
            try
            {
                var rootBoletas = _configuration["Archivos:RaizBoletas"];
                var dirVenta = Path.Combine(rootBoletas, venta.IdVenta.ToString());

                // Nombre del archivo ZIP
                var nombreZip = $"Boletas_Venta_{venta.IdVenta}_{venta.Evento!.Nombre.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmm}.zip";
                var zipPath = Path.Combine(dirVenta, nombreZip);

                // Crear el archivo ZIP
                using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    foreach (var rutaImagen in rutasImagenes)
                    {
                        if (File.Exists(rutaImagen))
                        {
                            var nombreArchivo = Path.GetFileName(rutaImagen);
                            zipArchive.CreateEntryFromFile(rutaImagen, nombreArchivo);
                        }
                    }
                }

                return zipPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear archivo ZIP: {ex.Message}", ex);
            }
        }

        private string CrearNotificacion(VentasModel venta, List<BoletaModel> boletas)
        {
            var plantillaOk = _configuration["Mensajes:EntregaLarga"];
            var fecha = UtilitiesHelper.ToBogotaFromUtc(venta.Evento!.Fecha);

            var mensaje = UtilitiesHelper.RenderPlantilla(plantillaOk, new()
            {
                ["NOMBRE"] = $"{venta.Persona!.Nombres} {venta.Persona.Apellidos}",
                ["EVENTO"] = venta.Evento!.Nombre,
                ["FECHA"] = fecha.ToString("yyyy-MM-ddTHH:mm"),
                ["LINKS"] = string.Join(" ", boletas.Select(b => b.UrlImagen))
            });

            var numeroLimpio = (venta.Persona!.Celular ?? "").Replace("+", "").Replace(" ", "").Replace("-", "");
            var numeroFinal = "57" + numeroLimpio;
            var linkWa = _configuration["LinkWhatsapp"]; // "https://wa.me/{NUMERO}?text={MENSAJE}"
            var msgParam = Uri.EscapeDataString(mensaje);

            return linkWa.Replace("{NUMERO}", numeroFinal).Replace("{MENSAJE}", msgParam);
        }

        // QRCoder (paquete: QRCoder)
        private byte[] GenerarQrPng(string payload, int pixelsPorModulo = 10)
        {
            using var generator = new QRCoder.QRCodeGenerator();
            var data = generator.CreateQrCode(payload, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var pngQr = new QRCoder.PngByteQRCode(data);
            return pngQr.GetGraphic(pixelsPorModulo, new byte[] { 0, 0, 0 }, new byte[] { 255, 255, 255 }, true);
        }

        private void GenerarBoleta(string plantillaPath, string fontPath, BoletaOverlayData info, string outPath,
            int qrSizePx = 500, int safeBottom = 120, int jpegQuality = 92)
        {
            if (!File.Exists(plantillaPath)) throw new FileNotFoundException("Plantilla no encontrada", plantillaPath);
            if (!File.Exists(fontPath)) throw new FileNotFoundException("Fuente no encontrada", fontPath);

            using var plantilla = SKBitmap.Decode(plantillaPath);
            using var surface = SKSurface.Create(new SKImageInfo(plantilla.Width, plantilla.Height));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);
            canvas.DrawBitmap(plantilla, 0, 0);

            // ---- QR centrado con tarjeta más delgada ----
            float qrX = (plantilla.Width - qrSizePx) / 2f;
            float qrY = plantilla.Height - safeBottom - qrSizePx;

            using (var qrBitmap = SKBitmap.Decode(GenerarQrPng(info.QrPayload)))
            {
                float pad = 8f; // ⬅ antes 24: borde blanco más delgado
                var tarjeta = new SKRect(qrX - pad, qrY - pad, qrX + qrSizePx + pad, qrY + qrSizePx + pad);
                using (var bg = new SKPaint { Color = SKColors.White, IsAntialias = true })
                    canvas.DrawRoundRect(tarjeta, 16, 16, bg);

                var destRect = new SKRect(qrX, qrY, qrX + qrSizePx, qrY + qrSizePx);
                canvas.DrawBitmap(qrBitmap, destRect);
            }

            // ---- Texto: más pequeño y centrado ----
            using var typeface = SKTypeface.FromFile(fontPath);
            float fontSize = 42f; // ⬅ antes 62
            using var font = new SKFont(typeface, fontSize)
            {
                Edging = SKFontEdging.Antialias,
                Subpixel = true
            };

            using var paintFill = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextAlign = SKTextAlign.Center
            };
            using var paintStroke = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 5f,
                TextAlign = SKTextAlign.Center
            };

            string texto = $"N° {info.NumeroBoleta}";
            var fm = font.Metrics;
            float centerX = plantilla.Width / 2f;
            float gapBelow = 14f; // ⬅ antes 28: menos espacio entre QR y texto
            float qrBottom = plantilla.Height - safeBottom;
            float baselineY = qrBottom + gapBelow - fm.Ascent;

            // Medimos para centrar exacto el rect de fondo
            font.MeasureText(texto, out var bounds);
            float textWidth = bounds.Width;

            // Fondo oscuro para legibilidad (ajustado al nuevo tamaño)
            float padX = 22f, padY = 10f, radius = 12f;
            var rect = new SKRect(
                centerX - textWidth / 2f - padX,
                baselineY + fm.Ascent - padY,
                centerX + textWidth / 2f + padX,
                baselineY + fm.Descent + padY
            );
            using (var bgText = new SKPaint { Color = new SKColor(0, 0, 0, 190), IsAntialias = true })
                canvas.DrawRoundRect(rect, radius, radius, bgText);

            paintFill.TextAlign = SKTextAlign.Center;

            // Trazo + relleno (centrado horizontal porque usamos centerX)
            canvas.DrawText(texto, centerX, baselineY, font, paintStroke);
            canvas.DrawText(texto, centerX, baselineY, font, paintFill);

            // Guardar
            canvas.Flush();
            using var img = surface.Snapshot();
            using var data = img.Encode(SKEncodedImageFormat.Jpeg, jpegQuality);
            Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
            using var fs = File.Open(outPath, FileMode.Create, FileAccess.Write, FileShare.None);
            data.SaveTo(fs);
        }

        private static string GenerarNumeroBoleta(long idVenta, int index)
        {
            // FV-000123-01 (Venta 123, boleta 01)
            return $"FV-{idVenta:D6}-{index:D2}";
        }
    }

    public sealed class BoletaOverlayData
    {
        public required string NumeroBoleta { get; set; }  // "FV-000021-01"
        public required string QrPayload { get; set; }  // URL o token codificado en el QR
    }
}

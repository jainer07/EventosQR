using eventos_qr.BLL.Contracts;
using eventos_qr.BLL.Helpers;
using eventos_qr.DAL;
using eventos_qr.Mapper;
using eventos_qr.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text.Json;
using static eventos_qr.Entity.Enums.Configuracion;

namespace eventos_qr.Controllers
{
    public class VentaController : Controller
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IEventoRepository _eventoRepository;
        private readonly IConfiguration _configuration;
        private readonly IPersonaRepository _personaRepository;
        private readonly IBoletaRepository _boletaRepository;
        private readonly EventosQR_Contex _ctx;
        private readonly VentaMapper _mapper = new();
        private const int PageSize = 10;
        public VentaController(IVentaRepository ventaRepository, IEventoRepository eventoRepository, IConfiguration configuration,
            IPersonaRepository personaRepository, IBoletaRepository boletaRepository, EventosQR_Contex ctx)
        {
            _ventaRepository = ventaRepository;
            _eventoRepository = eventoRepository;
            _configuration = configuration;
            _personaRepository = personaRepository;
            _boletaRepository = boletaRepository;
            _ctx = ctx;
        }

        public async Task<IActionResult> Index(string? q, int page = 1, CancellationToken ct = default)
        {
            var (items, total) = await _ventaRepository.ListarAsync(q, page, PageSize, ct);

            ViewBag.Query = q;
            ViewBag.Page = page;
            ViewBag.Total = total;
            ViewBag.PageSize = PageSize;

            var model = _mapper.VentaViewModelMapper(items);

            return View(model);
        }

        public async Task<IActionResult> Create(CancellationToken ct = default)
        {
            var eventos = await _eventoRepository.ListarAsync();
            var lsEventos = new List<SelectListItem>();

            foreach (var item in eventos)
            {
                var evento = new SelectListItem()
                {
                    Value = item.IdEvento.ToString(),
                    Text = item.Nombre,
                };

                lsEventos.Add(evento);
            }

            var fecha = UtilitiesHelper.ToBogotaFromUtc(DateTime.UtcNow);

            var model = new VentaViewModel()
            {
                Eventos = lsEventos,
                Capacidad = eventos.ToList()[0].Capacidad,
                Disponibles = eventos.ToList()[0].Disponibles,
                PrecioUnitario = eventos.ToList()[0].PrecioUnitario,
                Fecha = fecha
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentaViewModel model, CancellationToken ct)
        {

            var dto = _mapper.VentaDtoMapper(model);
            dto.Fecha = UtilitiesHelper.ToUtcFromBogota(model.Fecha);

            var resultado = await _ventaRepository.CrearAsync(dto, ct);

            if (resultado.Codigo == 0)
            {
                if (!string.IsNullOrEmpty(resultado.Data))
                {
                    var ls = JsonSerializer.Deserialize<List<string>>(resultado.Data);

                    TempData["WhatsappPlantilla"] = ls[0];
                    TempData["WhatsappNumero"] = ls[1];
                    TempData["WhatsappMsg64"] = ls[2];
                    TempData["WhatsappIdVenta"] = ls[3];
                }

                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                model = await GetModelIndex(model);

                // Si hay error, mostrar el mensaje en la vista actual
                ViewBag.ErrorMessage = resultado.Mensaje;
                return View(model);
            }
        }

        public async Task<IActionResult> Details(long id, CancellationToken ct)
        {
            var venta = await _ventaRepository.ObtenerAsync(id, ct);

            if (venta == null) return NotFound();

            var ventaMapper = _mapper.VentaViewModelMapper(venta);
            return View(ventaMapper);
        }

        public async Task<IActionResult> Edit(long id, CancellationToken ct = default)
        {
            var venta = await _ventaRepository.ObtenerAsync(id, ct);
            if (venta == null) return NotFound();

            var ventaMapper = _mapper.VentaViewModelMapper(venta);

            var eventos = await _eventoRepository.ListarAsync();
            var lsEventos = new List<SelectListItem>();

            foreach (var item in eventos)
            {
                var evento = new SelectListItem()
                {
                    Value = item.IdEvento.ToString(),
                    Text = item.Nombre,
                };

                lsEventos.Add(evento);
            }

            ventaMapper.Eventos = lsEventos;
            ventaMapper.Capacidad = eventos.ToList()[0].Capacidad;
            ventaMapper.Disponibles = eventos.ToList()[0].Disponibles;
            ventaMapper.PrecioUnitario = eventos.ToList()[0].PrecioUnitario;

            return View(ventaMapper);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, VentaViewModel model, CancellationToken ct)
        {
            if (id != model.IdVenta) return BadRequest();

            var dto = _mapper.VentaDtoMapper(model);
            dto.Fecha = UtilitiesHelper.ToUtcFromBogota(model.Fecha);

            var resultado = await _ventaRepository.ActualizarAsync(id, dto, ct);
            if (resultado.Codigo == 0)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                model = await GetModelIndex(model);

                // Si hay error, mostrar el mensaje en la vista actual
                ViewBag.ErrorMessage = resultado.Mensaje;
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var venta = await _ventaRepository.ObtenerAsync(id, ct);

            if (venta == null) return NotFound();

            var ventaMapper = _mapper.VentaViewModelMapper(venta);
            return View(ventaMapper);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            var resultado = await _ventaRepository.DeleteAsync(id, ct);
            if (resultado.Codigo == 0)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ErrorMessage = resultado.Mensaje;

            var venta = await _ventaRepository.ObtenerAsync(id, ct);
            if (venta == null) return NotFound();

            var personaMapper = _mapper.VentaViewModelMapper(venta);
            return View(personaMapper);
        }

        public async Task<IActionResult> Aplicar(long id, CancellationToken ct)
        {
            var venta = await _ventaRepository.ObtenerAsync(id, ct);
            if (venta == null) return NotFound();

            var dto = _mapper.VentaViewModelMapper(venta);

            return View("Aplicar", dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Aplicar(long id, string accion, CancellationToken ct)
        {
            // accion = "aplicar" | "rechazar"
            var aplicar = string.Equals(accion, "aplicar", StringComparison.OrdinalIgnoreCase);

            var resp = await _boletaRepository.AplicarVentaAsync(id, aplicar, ct);
            if (resp.Codigo == 0)
            {
                TempData["SuccessMessage"] = resp.Mensaje;
                if (aplicar && !string.IsNullOrWhiteSpace(resp.Data))
                {
                    try
                    {
                        var responseData = JsonSerializer.Deserialize<JsonElement>(resp.Data);

                        // WhatsApp URL
                        if (responseData.TryGetProperty("WhatsappUrl", out var whatsappElement))
                        {
                            TempData["WhatsappUrl"] = whatsappElement.GetString();
                        }

                        // ZIP Path para descarga
                        if (responseData.TryGetProperty("ZipPath", out var zipElement))
                        {
                            TempData["ZipPath"] = zipElement.GetString();
                        }

                        return RedirectToAction(nameof(Aplicar), new { id });
                    }
                    catch (JsonException)
                    {
                        // Si falla el parse, probablemente es el formato anterior (solo WhatsApp)
                        TempData["WhatsappUrl"] = resp.Data;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = resp.Mensaje;
            return RedirectToAction(nameof(Aplicar), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> BuscarPersona(string numeroDocumento, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento))
                return Json(new { success = false, message = "Debe ingresar un número de documento" });

            try
            {
                var (personas, total) = await _personaRepository.ListarAsync(numeroDocumento, 1, 1, ct);
                var persona = personas.FirstOrDefault(p => p.NumeroDocumento == numeroDocumento);

                if (persona != null)
                {
                    return Json(new
                    {
                        success = true,
                        persona = new
                        {
                            IdPersona = persona.IdPersona,
                            NombreCompleto = persona.NombreCompleto,
                            NumeroDocumento = persona.NumeroDocumento,
                            TipoDocumento = persona.TipoDocumento,
                            Celular = persona.Celular,
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "No se encontró ninguna persona con ese número de documento" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al buscar la persona: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarNotificacion(int id, CancellationToken ct)
        {
            try
            {
                var venta = await _ctx.VentasModels.FirstOrDefaultAsync(v => v.IdVenta == id, ct);
                if (venta == null)
                    return NotFound();

                venta.EnvioNotificacion = (int)TipoNotifiacion.Pendiente;
                await _ctx.SaveChangesAsync(ct);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> DescargarBoletasZip(long id, CancellationToken ct)
        {
            try
            {
                var venta = await _ventaRepository.ObtenerAsync(id, ct);
                if (venta == null)
                {
                    TempData["ErrorMessage"] = "La venta no existe.";
                    return RedirectToAction(nameof(Index));
                }

                if (venta.EstadoVenta != (int)EstadoVenta.Aplicada)
                {
                    TempData["ErrorMessage"] = "Solo se pueden descargar boletas de ventas aplicadas.";
                    return RedirectToAction(nameof(Index));
                }

                // Obtener las boletas de la base de datos
                var boletas = await _ctx.BoletaModels
                    .Where(b => b.IdVenta == id && b.Estado)
                    .ToListAsync(ct);

                if (!boletas.Any())
                {
                    TempData["ErrorMessage"] = "No se encontraron boletas para esta venta.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar que las imágenes existan y crear ZIP
                var rootBoletas = _configuration["Archivos:RaizBoletas"];
                var dirVenta = Path.Combine(rootBoletas, id.ToString());

                if (!Directory.Exists(dirVenta))
                {
                    TempData["ErrorMessage"] = "Los archivos de las boletas no se encontraron.";
                    return RedirectToAction(nameof(Index));
                }

                var rutasImagenes = new List<string>();
                foreach (var boleta in boletas)
                {
                    var fileName = $"{boleta.NumeroBoleta}.jpg";
                    var filePath = Path.Combine(dirVenta, fileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        rutasImagenes.Add(filePath);
                    }
                }

                if (!rutasImagenes.Any())
                {
                    TempData["ErrorMessage"] = "No se encontraron archivos de imagen de las boletas.";
                    return RedirectToAction(nameof(Index));
                }

                // Crear ZIP temporal
                var evento = await _eventoRepository.ObtenerAsync(venta.IdEvento);
                var nombreZip = $"Boletas_Venta_{venta.IdVenta}_{evento?.Nombre?.Replace(" ", "_") ?? "Evento"}_{DateTime.Now:yyyyMMdd_HHmm}.zip";
                var zipPath = Path.Combine(Path.GetTempPath(), nombreZip);

                using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    foreach (var rutaImagen in rutasImagenes)
                    {
                        var nombreArchivo = Path.GetFileName(rutaImagen);
                        zipArchive.CreateEntryFromFile(rutaImagen, nombreArchivo);
                    }
                }

                // Leer el archivo y devolverlo como descarga
                var fileBytes = await System.IO.File.ReadAllBytesAsync(zipPath, ct);

                // Eliminar archivo temporal después de leerlo
                try
                {
                    System.IO.File.Delete(zipPath);
                }
                catch
                {
                    // Ignorar errores al eliminar archivo temporal
                }

                return File(fileBytes, "application/zip", nombreZip);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al generar el archivo ZIP: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult DescargarZip(string zipPath)
        {
            try
            {
                if (string.IsNullOrEmpty(zipPath) || !System.IO.File.Exists(zipPath))
                {
                    TempData["ErrorMessage"] = "El archivo ZIP no se encontró o no existe.";
                    return RedirectToAction(nameof(Index));
                }

                var fileName = Path.GetFileName(zipPath);
                var fileBytes = System.IO.File.ReadAllBytes(zipPath);

                // Eliminar el archivo después de leerlo (opcional)
                // System.IO.File.Delete(zipPath);

                return File(fileBytes, "application/zip", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al descargar el archivo: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<VentaViewModel> GetModelIndex(VentaViewModel model)
        {
            var eventos = await _eventoRepository.ListarAsync();
            var lsEventos = new List<SelectListItem>();

            foreach (var item in eventos)
            {
                var evento = new SelectListItem()
                {
                    Value = item.IdEvento.ToString(),
                    Text = item.Nombre,
                };

                lsEventos.Add(evento);
            }

            model.Capacidad = eventos.ToList()[0].Capacidad;
            model.Eventos = lsEventos;

            return model;
        }
    }
}

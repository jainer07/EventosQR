using eventos_qr.BLL.Contracts;
using eventos_qr.BLL.Helpers;
using eventos_qr.BLL.repositories;
using eventos_qr.DAL;
using eventos_qr.Mapper;
using eventos_qr.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace eventos_qr.Controllers
{
    public class VentaController : Controller
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IEventoRepository _eventoRepository;
        private readonly IPersonaRepository _personaRepository;
        private readonly EventosQR_Contex _ctx;
        private readonly VentaMapper _mapper = new();
        private const int PageSize = 10;
        public VentaController(IVentaRepository ventaRepository, IEventoRepository eventoRepository,
            IPersonaRepository personaRepository, EventosQR_Contex ctx)
        {
            _ventaRepository = ventaRepository;
            _eventoRepository = eventoRepository;
            _personaRepository = personaRepository;
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
            if (ModelState.IsValid)
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

            model = await GetModelIndex(model);

            return View(model);
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

                venta.EnvioNotificacion = true;
                await _ctx.SaveChangesAsync(ct);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
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

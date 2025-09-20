using eventos_qr.BLL.Contracts;
using eventos_qr.BLL.Helpers;
using eventos_qr.Mapper;
using eventos_qr.Models;
using Microsoft.AspNetCore.Mvc;

namespace eventos_qr.Controllers
{
    public class EventoController(IEventoRepository eventoRepository) : Controller
    {
        private readonly IEventoRepository _eventoRepository = eventoRepository;
        private readonly EventoMapper _mapper = new();

        public async Task<IActionResult> Index()
        {
            var eventos = await _eventoRepository.ListarAsync();
            var eventoMapper = _mapper.EventoViewModelMapper(eventos.ToList());

            return View(eventoMapper);
        }

        public IActionResult Create()
        {
            var model = new EventoViewModel { Fecha = DateTime.Today.AddDays(7) };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventoViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.Vendidas > model.Capacidad)
                ModelState.AddModelError(nameof(model.Vendidas), "Vendidas no puede superar la Capacidad.");

            var dto = _mapper.EventoDtoMapper(model);
            dto.Fecha = UtilitiesHelper.ToUtcFromBogota(model.Fecha);

            await _eventoRepository.CrearAsync(dto);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var evento = await _eventoRepository.ObtenerAsync(id);

            if (evento == null) return NotFound();

            var eventoMapper = _mapper.EventoViewModelMapper(evento);
            return View(eventoMapper);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var evento = await _eventoRepository.ObtenerAsync(id);

            if (evento == null) return NotFound();

            var eventoMapper = _mapper.EventoViewModelMapper(evento);
            return View(eventoMapper);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventoViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (id != model.IdEvento) return BadRequest();

            if (model.Vendidas > model.Capacidad)
                ModelState.AddModelError(nameof(model.Vendidas), "Vendidas no puede superar la Capacidad.");

            var dto = _mapper.EventoDtoMapper(model);
            dto.Fecha = UtilitiesHelper.ToUtcFromBogota(model.Fecha);

            var result = await _eventoRepository.ActualizarAsync(id, dto);

            if (result)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Otro usuario modificó este evento. Recarga la página e inténtalo de nuevo.");

            var fresh = await _eventoRepository.ObtenerAsync(id); // o query directo
            if (fresh != null)
            {
                var vm = _mapper.EventoViewModelMapper(fresh);
                
                vm.Nombre = model.Nombre;
                vm.Fecha = model.Fecha;
                vm.Capacidad = model.Capacidad;
                vm.PrecioUnitario = model.PrecioUnitario;
                vm.Vendidas = model.Vendidas;

                return View(vm);
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var evento = await _eventoRepository.ObtenerAsync(id);

            if (evento == null) return NotFound();

            var eventoMapper = _mapper.EventoViewModelMapper(evento);
            return View(eventoMapper);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _eventoRepository.EliminarAsync(id);
            if (result)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Otro usuario modificó este evento. Recarga la página e inténtalo de nuevo.");
            
            var evento = await _eventoRepository.ObtenerAsync(id);
            if (evento == null) return NotFound();
            
            var eventoMapper = _mapper.EventoViewModelMapper(evento);
            return View(eventoMapper);
        }
    }
}

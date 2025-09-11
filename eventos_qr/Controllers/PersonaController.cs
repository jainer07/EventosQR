using eventos_qr.BLL.Contracts;
using eventos_qr.BLL.repositories;
using eventos_qr.Mapper;
using eventos_qr.Models;
using Microsoft.AspNetCore.Mvc;

namespace eventos_qr.Controllers
{
    public class PersonaController(IPersonaRepository personaRepository) : Controller
    {
        private readonly IPersonaRepository _personaRepository = personaRepository;
        private readonly PersonaMapper _mapper = new();
        private const int PageSize = 10;

        public async Task<IActionResult> Index(string? q, int page = 1, CancellationToken ct = default)
        {
            var (items, total) = await _personaRepository.ListarAsync(q, page, PageSize, ct);

            ViewBag.Query = q;
            ViewBag.Page = page;
            ViewBag.Total = total;
            ViewBag.PageSize = PageSize;

            var model = _mapper.PersonaViewModelMapper(items);

            return View(model);
        }

        public IActionResult Create() => View(new PersonaViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonaViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            var resultado = await _personaRepository.CrearAsync(_mapper.PersonaDtoMapper(model), ct);

            if (resultado.Codigo == 0)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Si hay error, mostrar el mensaje en la vista actual
                ViewBag.ErrorMessage = resultado.Mensaje;
                return View(model);
            }
        }

        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var persona = await _personaRepository.ObtenerAsync(id, ct);

            if (persona == null) return NotFound();

            var personaMapper = _mapper.PersonaViewModelMapper(persona);
            return View(personaMapper);
        }

        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var persona = await _personaRepository.ObtenerAsync(id, ct);

            if (persona == null) return NotFound();

            var personaMapper = _mapper.PersonaViewModelMapper(persona);
            return View(personaMapper);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, PersonaViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            if (id != model.IdPersona) return BadRequest();

            var resultado = await _personaRepository.ActualizarAsync(id, _mapper.PersonaDtoMapper(model), ct);

            if (resultado.Codigo == 0)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            // Si hay error, mostrar el mensaje en la vista actual
            ViewBag.ErrorMessage = resultado.Mensaje;

            var fresh = await _personaRepository.ObtenerAsync(id, ct);
            if (fresh != null)
            {
                var vm = _mapper.PersonaViewModelMapper(fresh);
                // Copia los campos editados del usuario en vm si quieres que no se pierdan
                vm.Nombres = model.Nombres;
                vm.Apellidos = model.Apellidos;
                vm.TipoDocumento = model.TipoDocumento;
                vm.NumeroDocumento = model.NumeroDocumento;
                vm.Celular = model.Celular;
                vm.Correo = model.Correo;
                vm.Estado = model.Estado;

                return View(vm);
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var persona = await _personaRepository.ObtenerAsync(id, ct);

            if (persona == null) return NotFound();

            var personaMapper = _mapper.PersonaViewModelMapper(persona);
            return View(personaMapper);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            var resultado = await _personaRepository.EliminarAsync(id, ct);
            if (resultado.Codigo == 0)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ErrorMessage = resultado.Mensaje;

            var persona = await _personaRepository.ObtenerAsync(id, ct);
            if (persona == null) return NotFound();

            var personaMapper = _mapper.PersonaViewModelMapper(persona);
            return View(personaMapper);
        }
    }
}

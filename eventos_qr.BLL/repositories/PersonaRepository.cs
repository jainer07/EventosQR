using eventos_qr.BLL.Contracts;
using eventos_qr.BLL.Helpers;
using eventos_qr.BLL.Mapper;
using eventos_qr.BLL.Models;
using eventos_qr.DAL.Queries;
using Microsoft.EntityFrameworkCore;

namespace eventos_qr.BLL.repositories
{
    public class PersonaRepository(PersonaQueryService personaQuery) : IPersonaRepository
    {
        private readonly PersonaQueryService _personaQuery = personaQuery;
        private readonly PersonaMapper _mapper = new();

        public async Task<PersonaDto?> ObtenerAsync(long id, CancellationToken ct)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentException("El ID de la persona debe ser un número positivo.", nameof(id));

                var evento = await _personaQuery.ObtenerAsync(id, ct);

                if (evento == null)
                    return null;

                return _mapper.PersonaDtoMapper(evento);
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error en un log si es necesario
                throw new ApplicationException($"Error al obtener la persona con ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<(List<PersonaDto> Items, int Total)> ListarAsync(string? filtro, int page, int pageSize, CancellationToken ct)
        {
            try
            {
                var personas = await _personaQuery.ListarAsync(filtro, page, pageSize, ct);
                var total = await CountAsync(filtro, ct);
                var items = _mapper.PersonaDtoMapper(personas).ToList();


                return (items, total);
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error en un log si es necesario
                throw new ApplicationException("Error al listar las personas: " + ex.Message, ex);
            }
        }

        public async Task<PersonaDto?> GetByDocumentoAsync(int tipo, string numero, CancellationToken ct)
        {
            try
            {
                if (tipo > 0 && !string.IsNullOrWhiteSpace(numero))
                {
                    var evento = await _personaQuery.GetByDocumentoAsync(tipo, numero, ct);

                    if (evento == null)
                        return null;

                    return _mapper.PersonaDtoMapper(evento);
                }
                else
                {
                    throw new ArgumentException("El tipo de documento debe ser un número positivo y el número no puede estar vacío.");
                }
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error en un log si es necesario
                throw new ApplicationException($"Error al obtener la persona con tipo {tipo} y número {numero}: {ex.Message}", ex);
            }
        }

        public async Task<int> CountAsync(string? filtro, CancellationToken ct)
        {
            try
            {
                var count = await _personaQuery.CountAsync(filtro, ct);

                return count;
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error en un log si es necesario
                throw new ApplicationException("Error al contar las personas: " + ex.Message, ex);
            }
        }

        public async Task<RespuestaType> CrearAsync(PersonaDto entity, CancellationToken ct)
        {
            var respuesta = new RespuestaType() { Codigo = 99, Mensaje = "Error al guardar persona" };

            if (entity is null)
            {
                respuesta.Mensaje = "La entidad persona no puede ser nula.";
                respuesta.Codigo = 2;
                return respuesta;
            }

            var existe = await ObtenerAsync(entity.IdPersona, ct);
            if (existe != null)
            {
                respuesta.Mensaje = $"La persona con ID {entity.IdPersona} ya existe.";
                respuesta.Codigo = 1;
                return respuesta;
            }

            if (!UtilitiesHelper.IsValidCelular(entity.Celular))
            {
                respuesta.Mensaje = "El número de celular no es válido.";
                respuesta.Codigo = 3;
                return respuesta;
            }

            if (!UtilitiesHelper.IsValidCorreo(entity.Correo))
            {
                respuesta.Mensaje = "El correo electrónico no es válido.";
                respuesta.Codigo = 4;
                return respuesta;
            }

            if (!UtilitiesHelper.IsValidDocumento(entity.TipoDocumento, entity.NumeroDocumento))
            {
                respuesta.Mensaje = "El número de documento no es válido para el tipo de documento especificado.";
                respuesta.Codigo = 5;
                return respuesta;
            }

            entity.FechaRegistro = DateTime.Now;
            var perosna = _mapper.PersonaModelMapper(entity);

            try
            {
                var result = await _personaQuery.CrearAsync(perosna, ct);
                if (result > 0)
                {
                    respuesta.Codigo = 0;
                    respuesta.Mensaje = "Persona creada exitosamente.";
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error en un log si es necesario
                respuesta.Mensaje = "Error al crear la persona: " + ex.Message;
                respuesta.Codigo = 99;
                return respuesta;
            }
        }

        public async Task<RespuestaType> ActualizarAsync(long id, PersonaDto entity, CancellationToken ct)
        {
            var respuesta = new RespuestaType() { Codigo = 99, Mensaje = "Error al actualizar persona" };

            if (entity is null)
            {
                respuesta.Mensaje = "La entidad persona no puede ser nula.";
                respuesta.Codigo = 2;
                return respuesta;
            }

            if (id != entity.IdPersona)
            {
                respuesta.Mensaje = "El ID de la persona no coincide con el ID proporcionado.";
                respuesta.Codigo = 2;
                return respuesta;
            }

            var existe = await ObtenerAsync(id, ct);
            if (existe is null)
            {
                respuesta.Mensaje = $"La persona con ID {entity.IdPersona} no existe.";
                respuesta.Codigo = 1;
                return respuesta;
            }

            if (!UtilitiesHelper.IsValidCelular(entity.Celular))
            {
                respuesta.Mensaje = "El número de celular no es válido.";
                respuesta.Codigo = 3;
                return respuesta;
            }

            if (!UtilitiesHelper.IsValidCorreo(entity.Correo))
            {
                respuesta.Mensaje = "El correo electrónico no es válido.";
                respuesta.Codigo = 4;
                return respuesta;
            }

            if (!UtilitiesHelper.IsValidDocumento(entity.TipoDocumento, entity.NumeroDocumento))
            {
                respuesta.Mensaje = "El número de documento no es válido para el tipo de documento especificado.";
                respuesta.Codigo = 5;
                return respuesta;
            }

            existe.Nombres = entity.Nombres;
            existe.Apellidos = entity.Apellidos;
            existe.TipoDocumento = entity.TipoDocumento;
            existe.NumeroDocumento = entity.NumeroDocumento;
            existe.Celular = entity.Celular;
            existe.Correo = entity.Correo;
            existe.Estado = entity.Estado;

            var persona = _mapper.PersonaModelMapper(existe);

            try
            {
                var result = await _personaQuery.ActualizarAsync(persona, entity.RowVersion, ct);
                if (result)
                {
                    respuesta.Codigo = 0;
                    respuesta.Mensaje = "Persona creada exitosamente.";
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error en un log si es necesario
                respuesta.Mensaje = "Error al actualizar la persona: " + ex.Message;
                respuesta.Codigo = 99;
                return respuesta;
            }
        }

        public async Task<RespuestaType> EliminarAsync(long id, CancellationToken ct)
        {
            var respuesta = new RespuestaType() { Codigo = 99, Mensaje = "Error al eliminar persona" };


            if (id <= 0)
            {
                respuesta.Mensaje = "El ID del evento debe ser un número positivo.";
                respuesta.Codigo = 2;
                return respuesta;
            }

            var existe = await ObtenerAsync(id, ct);
            if (existe is null)
            {
                respuesta.Mensaje = $"La persona con ID {id} no existe.";
                respuesta.Codigo = 1;
                return respuesta;
            }

            var persona = _mapper.PersonaModelMapper(existe);
            
            try
            {
                var result = await _personaQuery.EliminarAsync(persona, ct);

                if (result)
                {
                    respuesta.Codigo = 0;
                    respuesta.Mensaje = "Persona eliminada exitosamente.";
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = "Error al eliminar la persona: " + ex.Message;
                respuesta.Codigo = 99;
                return respuesta;
            }
        }
    }
}

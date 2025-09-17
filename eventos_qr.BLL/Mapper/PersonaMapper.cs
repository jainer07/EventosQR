using eventos_qr.Entity.Models;
using eventos_qr.Entity.Dtos;

namespace eventos_qr.BLL.Mapper
{
    public class PersonaMapper
    {
        public PersonaDto PersonaDtoMapper(PersonaModel persona)
        {
            return new PersonaDto
            {
                IdPersona = persona.IdPersona,
                Nombres = persona.Nombres,
                Apellidos = persona.Apellidos,
                TipoDocumento = persona.TipoDocumento,
                NumeroDocumento = persona.NumeroDocumento,
                Celular = persona.Celular,
                Correo = persona.Correo,
                Estado = persona.Estado,
                RowVersion = persona.RowVersion,
                FechaRegistro = persona.FechaRegistro,
            };
        }

        public List<PersonaDto> PersonaDtoMapper(List<PersonaModel> persona)
        {
            return persona.Select(p => new PersonaDto
            {
                IdPersona = p.IdPersona,
                Nombres = p.Nombres,
                Apellidos = p.Apellidos,
                TipoDocumento = p.TipoDocumento,
                NumeroDocumento = p.NumeroDocumento,
                Celular = p.Celular,
                Correo = p.Correo,
                Estado = p.Estado,
                FechaRegistro = p.FechaRegistro,
            }).ToList();
        }

        public PersonaModel PersonaModelMapper(PersonaDto persona)
        {
            return new PersonaModel
            {
                IdPersona = persona.IdPersona,
                Nombres = persona.Nombres,
                Apellidos = persona.Apellidos,
                TipoDocumento = persona.TipoDocumento,
                NumeroDocumento = persona.NumeroDocumento,
                Celular = persona.Celular,
                Correo = persona.Correo,
                Estado = persona.Estado,
                FechaRegistro = persona.FechaRegistro,
            };
        }
    }
}

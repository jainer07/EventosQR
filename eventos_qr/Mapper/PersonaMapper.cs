using eventos_qr.BLL.Models;
using eventos_qr.Models;

namespace eventos_qr.Mapper
{
    public class PersonaMapper
    {
        public PersonaViewModel PersonaViewModelMapper(PersonaDto persona)
        {
            var model = new PersonaViewModel
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

            switch (persona.TipoDocumento)
            {
                case 1:
                    model.CodigoTipoDocumento = "CC";
                    break;
                case 2:
                    model.CodigoTipoDocumento = "CE";
                    break;
                case 3:
                    model.CodigoTipoDocumento = "NI";
                    break;
                case 4:
                    model.CodigoTipoDocumento = "PA";
                    break;
            }

            return model;
        }

        public List<PersonaViewModel> PersonaViewModelMapper(List<PersonaDto> personas)
        {
            var ls = new List<PersonaViewModel>();
            foreach (var item in personas)
            {
                var persona = new PersonaViewModel
                {
                    IdPersona = item.IdPersona,
                    Nombres = item.Nombres,
                    Apellidos = item.Apellidos,
                    TipoDocumento = item.TipoDocumento,
                    NumeroDocumento = item.NumeroDocumento,
                    Celular = item.Celular,
                    Correo = item.Correo,
                    Estado = item.Estado,
                    FechaRegistro = item.FechaRegistro,
                };

                switch (item.TipoDocumento)
                {
                    case 1:
                        persona.CodigoTipoDocumento = "CC";
                        break;
                    case 2:
                        persona.CodigoTipoDocumento = "CE";
                        break;
                    case 3:
                        persona.CodigoTipoDocumento = "NI";
                        break;
                    case 4:
                        persona.CodigoTipoDocumento = "PA";
                        break;
                }

                ls.Add(persona);
            }

            return ls;
        }

        public PersonaDto PersonaDtoMapper(PersonaViewModel persona)
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
            };
        }
    }
}

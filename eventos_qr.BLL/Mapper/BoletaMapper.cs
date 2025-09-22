using eventos_qr.Entity.Dtos;
using eventos_qr.Entity.Models;

namespace eventos_qr.BLL.Mapper
{
    public class BoletaMapper
    {
        public BoletaDto BoletaDtoMapper(BoletaModel boleta)
        {
            var dto = new BoletaDto
            {
                IdBoleta = boleta.IdBoleta,
                IdVenta = boleta.IdVenta,
                NumeroBoleta = boleta.NumeroBoleta,
                CodigoQr = boleta.CodigoQr,
                UrlImagen = boleta.UrlImagen,
                FechaGeneracion = boleta.FechaGeneracionUtc,
                Estado = boleta.Estado,
                FechaUsoUtc = boleta.FechaUsoUtc,
                OperatorId = boleta.OperatorId,
                RowVersion = boleta.RowVersion,
            };

            if (boleta.Venta != null)
            {
                dto.NombreEvento = boleta.Venta.Evento?.Nombre ?? "";
                dto.FechaEvento = boleta.Venta.Evento?.Fecha ?? DateTime.MinValue;
                dto.NombrePersona = $"{boleta.Venta.Persona?.Nombres} {boleta.Venta.Persona?.Apellidos}";
                dto.DocumentoPersona = boleta.Venta.Persona?.NumeroDocumento ?? "";
            }

            return dto;
        }

        public List<BoletaDto> BoletaDtoMapper(List<BoletaModel> boleta)
        {
            return boleta.Select(b => new BoletaDto
            {
                IdBoleta = b.IdBoleta,
                IdVenta = b.IdVenta,
                NumeroBoleta = b.NumeroBoleta,
                CodigoQr = b.CodigoQr,
                UrlImagen = b.UrlImagen,
                FechaGeneracion = b.FechaGeneracionUtc,
                Estado = b.Estado,
                OperatorId = b.OperatorId,
                FechaUsoUtc = b.FechaUsoUtc,
            }).ToList();
        }

        public BoletaModel BoletaModelMapper(BoletaDto boleta)
        {
            return new BoletaModel
            {
                IdBoleta = boleta.IdBoleta,
                IdVenta = boleta.IdVenta,
                NumeroBoleta = boleta.NumeroBoleta,
                CodigoQr = boleta.CodigoQr,
                UrlImagen = boleta.UrlImagen,
                FechaGeneracionUtc = boleta.FechaGeneracion,
                Estado = boleta.Estado,
                OperatorId = boleta.OperatorId,
                FechaUsoUtc = boleta.FechaUsoUtc,
                RowVersion = boleta.RowVersion,
            };
        }
    }
}

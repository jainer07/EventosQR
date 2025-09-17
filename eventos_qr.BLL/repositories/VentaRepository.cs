using eventos_qr.BLL.Contracts;
using eventos_qr.BLL.Helpers;
using eventos_qr.DAL;
using eventos_qr.DAL.Queries;
using eventos_qr.Entity.Dtos;
using eventos_qr.Entity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using static eventos_qr.Entity.Enums.Configuracion;

namespace eventos_qr.BLL.repositories
{
    public class VentaRepository(EventosQR_Contex ctx, VentasQueryService ventasQuery, IPersonaRepository personaRepository,
        IEventoRepository eventoRepository, IConfiguration configuration) : IVentaRepository
    {
        private readonly VentasQueryService _ventasQuery = ventasQuery;
        private readonly IPersonaRepository _personaRepository = personaRepository;
        private readonly IEventoRepository _eventoRepository = eventoRepository;
        private readonly IConfiguration _configuration = configuration;
        private readonly EventosQR_Contex _ctx = ctx;

        public async Task<VentaDto?> ObtenerAsync(long id, CancellationToken ct)
        {
            try
            {
                if (id < 0)
                    return null;

                var venta = await _ventasQuery.ObtenerAsync(id, ct);

                if (venta == null)
                    return null;

                venta.Fecha = UtilitiesHelper.ToBogotaFromUtc(venta.Fecha);

                return venta;
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error en un log si es necesario
                throw new ApplicationException($"Error al obtener la persona con ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<(List<VentaDto> Items, int Total)> ListarAsync(string? filtro, int page, int pageSize, CancellationToken ct)
        {
            try
            {
                var ventas = await _ventasQuery.ListarAsync(filtro, page, pageSize, ct);

                foreach (var item in ventas)
                {
                    item.Fecha = UtilitiesHelper.ToBogotaFromUtc(item.Fecha);
                }

                var total = await CountAsync(filtro, ct);

                return (ventas, total);
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error en un log si es necesario
                throw new ApplicationException("Error al listar las personas: " + ex.Message, ex);
            }
        }

        public async Task<int> CountAsync(string? filtro, CancellationToken ct)
        {
            try
            {
                var count = await _ventasQuery.CountAsync(filtro, ct);

                return count;
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error en un log si es necesario
                throw new ApplicationException("Error al contar las personas: " + ex.Message, ex);
            }
        }

        public async Task<RespuestaType> CrearAsync(VentaDto entity, CancellationToken ct)
        {
            var respuesta = new RespuestaType() { Codigo = 99, Mensaje = "Error al guardar venta" };

            try
            {
                respuesta = ValidarObligatorios(entity);
                if (respuesta.Codigo != 0) return respuesta;

                if (entity.Fecha == default) entity.Fecha = DateTime.UtcNow;

                entity.EstadoVenta = (int)EstadoVenta.Pendiente;

                var strategy = _ctx.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _ctx.Database.BeginTransactionAsync(ct);
                    try
                    {
                        var evento = await _ctx.EventoModels.FirstOrDefaultAsync(e => e.IdEvento == entity.IdEvento, ct);
                        if (evento == null)
                        {
                            respuesta.Mensaje = "El evento no existe";
                            await tx.RollbackAsync(ct);
                            return;
                        }

                        if (entity.Cantidad <= 0 || evento.Disponibles < entity.Cantidad)
                        {
                            respuesta.Mensaje = "No hay cupos suficientes para la cantidad solicitada";
                            await tx.RollbackAsync(ct);
                            return;
                        }

                        var model = new VentasModel
                        {
                            IdEvento = entity.IdEvento,
                            IdPersona = entity.IdPersona,
                            FechaUtc = entity.Fecha,
                            Cantidad = entity.Cantidad,
                            Total = entity.Cantidad * entity.PrecioUnitario,
                            EstadoVenta = entity.EstadoVenta,
                            ComprobantePago = entity.ComprobantePago ?? "",
                            EnvioNotificacion = (int)TipoNotifiacion.No,
                            RowVersion = 1
                        };

                        await _ctx.VentasModels.AddAsync(model, ct);

                        evento.Disponibles -= entity.Cantidad;
                        evento.Vendidas += entity.Cantidad;
                        if (evento.Disponibles < 0) evento.Disponibles = 0;
                        if (evento.Vendidas > evento.Capacidad) evento.Vendidas = evento.Capacidad;

                        try
                        {
                            _ctx.Entry(evento).Property("RowVersion").OriginalValue = evento.RowVersion;

                            await _ctx.SaveChangesAsync(ct);
                            await tx.CommitAsync(ct);

                        }
                        catch (DbUpdateException dbEx)
                        {
                            await tx.RollbackAsync(ct);

                            // Extrae el detalle de MySQL
                            var inner = dbEx.InnerException?.Message ?? dbEx.Message;
                            respuesta.Codigo = 99;
                            respuesta.Mensaje = $"No se pudo guardar la venta (DB): {inner}";
                            return; // importante: salir del strategy.ExecuteAsync
                        }
                        catch (Exception ex)
                        {
                            await tx.RollbackAsync(ct);

                            // Extrae el detalle de MySQL
                            var inner = ex.InnerException?.Message ?? ex.Message;
                            respuesta.Codigo = 99;
                            respuesta.Mensaje = $"No se pudo guardar la venta (DB): {inner}";
                            return; // importante:
                        }

                        // armar respuesta OK

                        var plantilla = _configuration["Mensajes:PagoPendiente"];
                        var plantillaWhatsapp = _configuration["LinkWhatsapp"];
                        var mensaje = UtilitiesHelper.RenderPlantilla(plantilla, new Dictionary<string, string> { ["NOMBRE"] = entity.NombrePersona });
                        var numeroLimpio = (entity.CelularPersona ?? "").Replace("+", "").Replace(" ", "").Replace("-", "");
                        var numeroFinal = "57" + numeroLimpio;
                        var msg64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(mensaje));

                        var ls = new List<string>() { plantillaWhatsapp, numeroFinal, msg64, model.IdVenta.ToString() };

                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Venta creada exitosamente.";
                        respuesta.Data = JsonSerializer.Serialize(ls);
                    }
                    catch (Exception)
                    {
                        await tx.RollbackAsync(ct);
                        throw;
                    }
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                respuesta.Mensaje = "Error de concurrencia al crear la venta. Intente nuevamente";
            }
            catch (Exception ex)
            {
                respuesta.Codigo = 99;
                respuesta.Mensaje = $"Error al crear la venta: {ex.Message}";
            }

            return respuesta;
        }

        public async Task<RespuestaType> ActualizarAsync(long id, VentaDto entity, CancellationToken ct)
        {
            var respuesta = new RespuestaType() { Codigo = 99, Mensaje = "Error al actualizar venta" };
            try
            {
                if (id <= 0)
                {
                    respuesta.Mensaje = "El ID de la venta es requerido";
                    return respuesta;
                }

                respuesta = ValidarObligatorios(entity);
                if (respuesta.Codigo != 0) return respuesta;

                var ventaExistente = await _ventasQuery.ObtenerAsync(id, ct);
                if (ventaExistente == null)
                {
                    respuesta.Mensaje = "La venta no existe";
                    return respuesta;
                }

                respuesta = ValidarActualizar(entity, ventaExistente);
                if (respuesta.Codigo != 0) return respuesta;

                respuesta = await ValidarEventoPersona(entity, ct);
                if (respuesta.Codigo != 0) return respuesta;

                var strategy = _ctx.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _ctx.Database.BeginTransactionAsync(ct);
                    try
                    {
                        var ventaModel = await _ctx.VentasModels.FirstOrDefaultAsync(v => v.IdVenta == id, ct);
                        if (ventaModel is null)
                        {
                            await tx.RollbackAsync(ct);
                            respuesta.Mensaje = "La venta no existe en la base de datos";
                            return;
                        }

                        if (ventaModel.RowVersion != entity.RowVersion)
                        {
                            await tx.RollbackAsync(ct);
                            respuesta.Mensaje = "La venta ha sido modificada por otro usuario. Recargue los datos e intente nuevamente";
                            return;
                        }

                        var evento = await _ctx.EventoModels.FirstOrDefaultAsync(e => e.IdEvento == entity.IdEvento, ct);
                        if (evento == null)
                        {
                            await tx.RollbackAsync(ct);
                            respuesta.Mensaje = "El evento no existe";
                            return;
                        }

                        // Si cambió el evento, reponemos al viejo y descontamos del nuevo
                        if (entity.IdEvento != ventaModel.IdEvento)
                        {
                            // 1) Reponer al evento anterior
                            evento.Disponibles += ventaModel.Cantidad;
                            evento.Vendidas -= ventaModel.Cantidad;
                            if (evento.Vendidas < 0) evento.Vendidas = 0;
                            if (evento.Disponibles > evento.Capacidad) evento.Disponibles = evento.Capacidad;

                            // Descontar del nuevo
                            var nuevoEvento = await _ctx.EventoModels.FirstOrDefaultAsync(x => x.IdEvento == entity.IdEvento, ct);
                            if (nuevoEvento is null)
                            {
                                await tx.RollbackAsync(ct);
                                respuesta.Mensaje = "El nuevo evento no existe";
                                return;
                            }

                            if (nuevoEvento.Disponibles < entity.Cantidad)
                            {
                                await tx.RollbackAsync(ct);
                                respuesta.Mensaje = "No hay cupos suficientes en el nuevo evento";
                                return;
                            }

                            nuevoEvento.Disponibles -= entity.Cantidad;
                            nuevoEvento.Vendidas += entity.Cantidad;

                            if (nuevoEvento.Disponibles < 0) nuevoEvento.Disponibles = 0;
                            if (nuevoEvento.Vendidas > nuevoEvento.Capacidad) nuevoEvento.Vendidas = nuevoEvento.Capacidad;

                            ventaModel.IdEvento = entity.IdEvento;
                        }
                        else
                        {
                            // Mismo evento: ajustar delta de cantidades si cambió
                            var delta = entity.Cantidad - ventaModel.Cantidad;
                            if (delta != 0)
                            {
                                if (delta > 0)
                                {
                                    // Quiero más entradas: verificar disponibles
                                    if (evento.Disponibles < delta)
                                    {
                                        await tx.RollbackAsync(ct);
                                        respuesta.Mensaje = "No hay cupos suficientes para aumentar la cantidad";
                                        return;
                                    }
                                    evento.Disponibles -= delta;
                                    evento.Vendidas += delta;
                                }
                                else
                                {
                                    // Devuelvo entradas
                                    evento.Disponibles += (-delta);
                                }
                            }
                        }

                        // Actualizar resto de campos
                        ventaModel.IdPersona = entity.IdPersona;
                        ventaModel.FechaUtc = entity.Fecha;
                        ventaModel.Cantidad = entity.Cantidad;
                        ventaModel.Total = entity.Cantidad * evento.PrecioUnitario;
                        ventaModel.EstadoVenta = entity.EstadoVenta;
                        ventaModel.ComprobantePago = entity.ComprobantePago ?? "";
                        ventaModel.EnvioNotificacion = entity.EnvioNotificacion;

                        // Concurrencia (token incrementado)
                        _ctx.Entry(ventaModel).Property(x => x.RowVersion).OriginalValue = entity.RowVersion;
                        ventaModel.RowVersion = entity.RowVersion + 1;

                        try
                        {
                            await _ctx.SaveChangesAsync(ct);
                            await tx.CommitAsync(ct);
                        }
                        catch (DbUpdateException dbEx)
                        {
                            await tx.RollbackAsync(ct);
                            var inner = dbEx.InnerException?.Message ?? dbEx.Message;
                            respuesta.Codigo = 99;
                            respuesta.Mensaje = $"No se pudo actualizar la venta (DB): {inner}";
                            return;
                        }
                        catch (Exception ex)
                        {
                            await tx.RollbackAsync(ct);
                            var inner = ex.InnerException?.Message ?? ex.Message;
                            respuesta.Codigo = 99;
                            respuesta.Mensaje = $"No se pudo actualizar la venta: {inner}";
                            return;
                        }

                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Venta actualizada correctamente";
                    }
                    catch
                    {
                        await tx.RollbackAsync(ct);
                        throw;
                    }
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                respuesta.Mensaje = "La venta ha sido modificada por otro usuario. Recargue los datos e intente nuevamente";
            }
            catch (Exception ex)
            {
                respuesta.Codigo = 99;
                respuesta.Mensaje = $"Error al actualizar la venta: {ex.Message}";
                // Log del error aquí
            }

            return respuesta;
        }

        public async Task<RespuestaType> DeleteAsync(long id, CancellationToken ct)
        {
            var respuesta = new RespuestaType() { Codigo = 99, Mensaje = "Error al eliminar venta" };

            try
            {
                if (id <= 0)
                {
                    respuesta.Mensaje = "El ID de la venta es requerido";
                    return respuesta;
                }

                var ventaExistente = await _ventasQuery.ObtenerAsync(id, ct);
                if (ventaExistente == null)
                {
                    respuesta.Mensaje = "La venta no existe";
                    return respuesta;
                }

                if (ventaExistente.EstadoVenta == (int)EstadoVenta.Eliminada) // Eliminada
                {
                    respuesta.Mensaje = "La venta ya ha sido eliminada";
                    return respuesta;
                }

                if (ventaExistente.EstadoVenta == (int)EstadoVenta.Aplicada) // Aplicada
                {
                    respuesta.Mensaje = "No se puede eliminar una venta que ya ha sido aplicada";
                    return respuesta;
                }

                var strategy = _ctx.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _ctx.Database.BeginTransactionAsync(ct);
                    try
                    {
                        var ventaModel = await _ctx.VentasModels.FirstOrDefaultAsync(v => v.IdVenta == id, ct);
                        if (ventaModel is null)
                        {
                            await tx.RollbackAsync(ct);
                            respuesta.Mensaje = "La venta no existe en la base de datos";
                            return;
                        }

                        var evento = await _ctx.EventoModels.FirstOrDefaultAsync(x => x.IdEvento == ventaModel.IdEvento, ct);
                        if (evento is null)
                        {
                            await tx.RollbackAsync(ct);
                            respuesta.Mensaje = "El evento asociado no existe";
                            return;
                        }

                        // Reponer cupos y vendidas
                        evento.Disponibles += ventaModel.Cantidad;
                        if (evento.Disponibles > evento.Capacidad) evento.Disponibles = evento.Capacidad;

                        evento.Vendidas -= ventaModel.Cantidad;
                        if (evento.Vendidas < 0) evento.Vendidas = 0;

                        // Marcar como eliminada + concurrencia
                        ventaModel.EstadoVenta = (int)EstadoVenta.Eliminada;
                        _ctx.Entry(ventaModel).Property(x => x.RowVersion).OriginalValue = ventaModel.RowVersion;
                        ventaModel.RowVersion = ventaModel.RowVersion + 1;

                        try
                        {
                            await _ctx.SaveChangesAsync(ct);
                            await tx.CommitAsync(ct);
                        }
                        catch (DbUpdateException dbEx)
                        {
                            await tx.RollbackAsync(ct);
                            var inner = dbEx.InnerException?.Message ?? dbEx.Message;
                            respuesta.Codigo = 99;
                            respuesta.Mensaje = $"No se pudo eliminar la venta (DB): {inner}";
                            return;
                        }
                        catch (Exception ex)
                        {
                            await tx.RollbackAsync(ct);
                            var inner = ex.InnerException?.Message ?? ex.Message;
                            respuesta.Codigo = 99;
                            respuesta.Mensaje = $"No se pudo eliminar la venta: {inner}";
                            return;
                        }

                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Venta eliminada correctamente";
                    }
                    catch (Exception)
                    {
                        await tx.RollbackAsync(ct);
                        throw;
                    }
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                respuesta.Mensaje = "La venta ha sido modificada por otro usuario. Recargue los datos e intente nuevamente";
            }
            catch (Exception ex)
            {
                respuesta.Codigo = 99;
                respuesta.Mensaje = $"Error al eliminar la venta: {ex.Message}";
            }

            return respuesta;
        }

        private RespuestaType ValidarObligatorios(VentaDto entity)
        {
            var respuesta = new RespuestaType()
            {
                Codigo = 0,
                Mensaje = ""
            };

            if (entity is null)
            {
                respuesta.Codigo = 1;
                respuesta.Mensaje = "La entidad persona no puede ser nula.";
                return respuesta;
            }

            if (entity.IdEvento <= 0)
            {
                respuesta.Codigo = 2;
                respuesta.Mensaje = "El evento es requerido";
                return respuesta;
            }

            if (entity.IdPersona <= 0 || string.IsNullOrEmpty(entity.NombrePersona) || string.IsNullOrEmpty(entity.CelularPersona))
            {
                respuesta.Codigo = 3;
                respuesta.Mensaje = "La persona es requerida";
                return respuesta;
            }

            if (entity.Cantidad <= 0)
            {
                respuesta.Codigo = 4;
                respuesta.Mensaje = "La cantidad debe ser mayor a cero";
                return respuesta;
            }

            return respuesta;
        }
        private async Task<RespuestaType> ValidarEventoPersona(VentaDto entity, CancellationToken ct)
        {
            var respuesta = new RespuestaType()
            {
                Codigo = 0,
                Mensaje = ""
            };

            var persona = await _personaRepository.ObtenerAsync(entity.IdPersona, ct);
            if (persona == null)
            {
                respuesta.Mensaje = $"La persona no existe";
                respuesta.Codigo = 6;
                return respuesta;
            }

            var evento = await _eventoRepository.ObtenerAsync(entity.IdEvento);
            if (evento == null)
            {
                respuesta.Mensaje = $"El evento no existe";
                respuesta.Codigo = 7;
                return respuesta;
            }

            if (!evento.Estado)
            {
                respuesta.Mensaje = "El evento no está activo";
                respuesta.Codigo = 8;
            }

            if (entity.Fecha > DateTime.UtcNow.AddDays(1))
            {
                respuesta.Mensaje = "La fecha de venta no puede ser futura";
                return respuesta;
            }

            if (evento.Capacidad < entity.Cantidad)
            {
                respuesta.Mensaje = $"Solo hay {evento.Capacidad} entradas disponibles";
                return respuesta;
            }

            if (evento.Fecha < DateTime.UtcNow)
            {
                respuesta.Mensaje = "No se pueden vender entradas para eventos que ya pasaron";
                return respuesta;
            }

            var eventoActual = await _ctx.EventoModels.AsNoTracking()
                                                      .FirstOrDefaultAsync(e => e.IdEvento == entity.IdEvento, ct);

            if (eventoActual == null)
            {
                respuesta.Mensaje = "El evento no se encuentra en la base de datos";
                respuesta.Codigo = 7;
                return respuesta;
            }

            if (eventoActual.Disponibles < entity.Cantidad)
            {
                respuesta.Mensaje = $"Solo hay {eventoActual.Disponibles} entradas disponibles";
                respuesta.Codigo = 10;
                return respuesta;
            }

            if (evento.Fecha < DateTime.UtcNow)
            {
                respuesta.Mensaje = "No se pueden vender entradas para eventos que ya pasaron";
                respuesta.Codigo = 11;
                return respuesta;
            }

            return respuesta;
        }
        private RespuestaType ValidarActualizar(VentaDto entity, VentaDto ventaExistente)
        {
            var respuesta = new RespuestaType() { Codigo = 0, Mensaje = "" };

            if (ventaExistente == null)
            {
                respuesta.Mensaje = "La venta no existe";
                return respuesta;
            }

            if (ventaExistente.EstadoVenta != (int)EstadoVenta.Pendiente)
            {
                respuesta.Mensaje = "Solo se pueden actualizar ventas en estado pendiente";
                return respuesta;
            }

            if (entity.IdPersona != ventaExistente.IdPersona)
            {
                respuesta.Mensaje = "No se puede cambiar la persona de una venta existente";
                return respuesta;
            }

            if (entity.RowVersion != ventaExistente.RowVersion)
            {
                respuesta.Mensaje = "La venta ha sido modificada por otro usuario. Recargue los datos e intente nuevamente";
                return respuesta;
            }

            return respuesta;
        }
    }
}

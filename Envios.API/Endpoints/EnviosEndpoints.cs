using Envios.Domain.Entities;
using Envios.Domain.Enums;
using Envios.Infrastructure;
using Envios.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Envios.API.Endpoints
{
    public static class EnviosEndpoints
    {
        public static void MapEnviosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/envios")
                .WithTags("Envíos");

            group.MapPost("/", CrearEnvio)
                .WithSummary("Crear un nuevo envío");

            group.MapGet("/{id:guid}", ObtenerEnvioPorId)
                .WithSummary("Obtener envío por ID");

            group.MapGet("/orden/{idOrden:guid}", ObtenerEnvioPorOrden)
                .WithSummary("Obtener envío por ID de orden");

            group.MapPatch("/{id:guid}/estado", CambiarEstado)
                .WithSummary("Cambiar estado del envío");

            group.MapPatch("/{id:guid}/reprogramar", ReprogramarEntrega)
                .WithSummary("Reprogramar entrega fallida");

            group.MapGet("/{id:guid}/rastreo", ObtenerHistorial)
                .WithSummary("Obtener historial de rastreo");
        }

        // POST /api/envios
        private static async Task<IResult> CrearEnvio(
            CrearEnvioDto dto,
            EnviosDbContext db,
            CancellationToken ct)
        {
            var resultado = Envio.Crear(
                dto.IdOrden,
                dto.DireccionSnapshot,
                dto.NombreRepartidor,
                dto.FechaEstimada
            );

            if (!resultado.EsExitoso)
                return Results.BadRequest(new { error = resultado.Error });

            db.Envios.Add(resultado.Valor!);
            await db.SaveChangesAsync(ct);

            return Results.Created(
                $"/api/envios/{resultado.Valor!.Id}",
                MapearEnvio(resultado.Valor!)
            );
        }

        // GET /api/envios/{id}
        private static async Task<IResult> ObtenerEnvioPorId(
            Guid id,
            EnviosDbContext db,
            CancellationToken ct)
        {
            var envio = await db.Envios
                .FirstOrDefaultAsync(e => e.Id == id, ct);

            if (envio is null)
                return Results.NotFound(new { error = $"No se encontró el envío con ID {id}" });

            return Results.Ok(MapearEnvio(envio));
        }

        // GET /api/envios/orden/{idOrden}
        private static async Task<IResult> ObtenerEnvioPorOrden(
            Guid idOrden,
            EnviosDbContext db,
            CancellationToken ct)
        {
            var envio = await db.Envios
                .FirstOrDefaultAsync(e => e.IdOrden == idOrden, ct);

            if (envio is null)
                return Results.NotFound(new { error = $"No se encontró envío para la orden {idOrden}" });

            return Results.Ok(MapearEnvio(envio));
        }

        // PATCH /api/envios/{id}/estado
        private static async Task<IResult> CambiarEstado(
            Guid id,
            CambiarEstadoDto dto,
            EnviosDbContext db,
            CancellationToken ct)
        {
            var envio = await db.Envios
                .FirstOrDefaultAsync(e => e.Id == id, ct);

            if (envio is null)
                return Results.NotFound(new { error = $"No se encontró el envío con ID {id}" });

            if (!Enum.TryParse<EstadoEnvio>(dto.NuevoEstado, ignoreCase: true, out var nuevoEstado))
                return Results.BadRequest(new
                {
                    error = "Estado inválido",
                    valoresPermitidos = Enum.GetNames<EstadoEnvio>()
                });

            var resultado = envio.CambiarEstado(nuevoEstado, dto.Nota);

            if (!resultado.EsExitoso)
                return Results.BadRequest(new { error = resultado.Error });

            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                id = envio.Id,
                estadoActual = envio.EstadoActual.ToString(),
                fechaEntregado = envio.FechaEntregado
            });
        }

        // PATCH /api/envios/{id}/reprogramar
        private static async Task<IResult> ReprogramarEntrega(
            Guid id,
            ReprogramarEnvioDto dto,
            EnviosDbContext db,
            CancellationToken ct)
        {
            var envio = await db.Envios
                .FirstOrDefaultAsync(e => e.Id == id, ct);

            if (envio is null)
                return Results.NotFound(new { error = $"No se encontró el envío con ID {id}" });

            var resultado = envio.Reprogramar(dto.NuevaFecha, dto.Motivo);

            if (!resultado.EsExitoso)
                return Results.BadRequest(new { error = resultado.Error });

            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                id = envio.Id,
                estadoActual = envio.EstadoActual.ToString(),
                nuevaFechaEstimada = envio.FechaEstimada
            });
        }

        // GET /api/envios/{id}/rastreo
        private static async Task<IResult> ObtenerHistorial(
            Guid id,
            EnviosDbContext db,
            CancellationToken ct)
        {
            var envio = await db.Envios
                .FirstOrDefaultAsync(e => e.Id == id, ct);

            if (envio is null)
                return Results.NotFound(new { error = $"No se encontró el envío con ID {id}" });

            var historial = envio.HistorialRastros
                .OrderBy(h => h.FechaEvento)
                .Select(h => new
                {
                    h.Id,
                    h.Estado,
                    h.Nota,
                    h.FechaEvento,
                    h.NuevaFechaProgramada
                });

            return Results.Ok(new
            {
                idEnvio = envio.Id,
                guiaPaqueteria = envio.GuiaPaqueteria,
                estadoActual = envio.EstadoActual.ToString(),
                historial
            });
        }

        // Mapper interno
        private static object MapearEnvio(Envio envio) => new
        {
            envio.Id,
            envio.IdOrden,
            envio.DireccionSnapshot,
            envio.GuiaPaqueteria,
            estadoActual = envio.EstadoActual.ToString(),
            envio.NombreRepartidor,
            envio.FechaEstimada,
            envio.FechaEntregado
        };
    }
}

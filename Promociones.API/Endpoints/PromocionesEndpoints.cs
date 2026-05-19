using Promociones.Domain.Entities;
using Promociones.Infrastructure;
using Promociones.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Promociones.API.Endpoints
{
    public static class PromocionesEndpoints
    {
        public static void MapPromocionesEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/promociones")
                .WithTags("Promociones");

            group.MapGet("/", ObtenerTodas)
                .WithSummary("Listar todas las campañas");

            group.MapGet("/activas", ObtenerActivas)
                .WithSummary("Listar campañas vigentes");

            group.MapGet("/{id:guid}", ObtenerPorId)
                .WithSummary("Obtener campaña por ID");

            group.MapPost("/", CrearPromocion)
                .WithSummary("Crear nueva campaña");

            group.MapDelete("/{id:guid}", DesactivarPromocion)
                .WithSummary("Desactivar campaña");

            group.MapPost("/{id:guid}/reglas", AgregarRegla)
                .WithSummary("Agregar regla de categoría");

            group.MapGet("/{id:guid}/reglas", ObtenerReglas)
                .WithSummary("Listar reglas de categoría");

            group.MapPost("/{id:guid}/msi", AgregarMSI)
                .WithSummary("Agregar opción MSI");

            group.MapGet("/{id:guid}/msi", ObtenerMSI)
                .WithSummary("Listar opciones MSI");

            group.MapPost("/aplicar", AplicarDescuentos)
                .WithSummary("Calcular descuentos aplicables a un carrito");
        }

        // GET /api/promociones
        private static async Task<IResult> ObtenerTodas(
            PromocionesDbContext db,
            CancellationToken ct)
        {
            var promociones = await db.Promociones
                .AsNoTracking()
                .ToListAsync(ct);

            return Results.Ok(promociones.Select(MapearPromocion));
        }

        // GET /api/promociones/activas
        private static async Task<IResult> ObtenerActivas(
            PromocionesDbContext db,
            CancellationToken ct)
        {
            var todas = await db.Promociones
                .AsNoTracking()
                .ToListAsync(ct);

            var activas = todas.Where(p => p.EstaVigente());

            return Results.Ok(activas.Select(MapearPromocion));
        }

        // GET /api/promociones/{id}
        private static async Task<IResult> ObtenerPorId(
            Guid id,
            PromocionesDbContext db,
            CancellationToken ct)
        {
            var promo = await db.Promociones
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (promo is null)
                return Results.NotFound(new { error = $"No se encontró la campaña con ID {id}" });

            return Results.Ok(MapearPromocion(promo));
        }

        // POST /api/promociones
        private static async Task<IResult> CrearPromocion(
            CrearPromocionDto dto,
            PromocionesDbContext db,
            CancellationToken ct)
        {
            var resultado = Promocion.Crear(
                dto.NombreCampana,
                dto.PorcentajeDescuento,
                dto.FechaInicio,
                dto.FechaFin,
                dto.EsIndefinido,
                dto.MontoMinimoCompra
            );

            if (!resultado.EsExitoso)
                return Results.BadRequest(new { error = resultado.Error });

            db.Promociones.Add(resultado.Valor!);
            await db.SaveChangesAsync(ct);

            return Results.Created(
                $"/api/promociones/{resultado.Valor!.Id}",
                MapearPromocion(resultado.Valor!)
            );
        }

        // DELETE /api/promociones/{id}
        private static async Task<IResult> DesactivarPromocion(
            Guid id,
            PromocionesDbContext db,
            CancellationToken ct)
        {
            var promo = await db.Promociones
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (promo is null)
                return Results.NotFound(new { error = $"No se encontró la campaña con ID {id}" });

            db.Promociones.Remove(promo);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { mensaje = "Campaña desactivada correctamente" });
        }

        // POST /api/promociones/{id}/reglas
        private static async Task<IResult> AgregarRegla(
            Guid id,
            AgregarReglaCategoriaDto dto,
            PromocionesDbContext db,
            CancellationToken ct)
        {
            var promo = await db.Promociones
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (promo is null)
                return Results.NotFound(new { error = $"No se encontró la campaña con ID {id}" });

            var resultado = promo.AgregarReglaCategoria(
                dto.IdCategoriaCatalogo,
                dto.PorcentajeAplicable
            );

            if (!resultado.EsExitoso)
                return Results.BadRequest(new { error = resultado.Error });

            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/promociones/{id}/reglas",
                new { mensaje = "Regla agregada correctamente", idPromocion = id });
        }

        // GET /api/promociones/{id}/reglas
        private static async Task<IResult> ObtenerReglas(
            Guid id,
            PromocionesDbContext db,
            CancellationToken ct)
        {
            var promo = await db.Promociones
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (promo is null)
                return Results.NotFound(new { error = $"No se encontró la campaña con ID {id}" });

            return Results.Ok(promo.ReglasCategorias.Select(r => new
            {
                r.Id,
                r.IdCategoriaCatalogo,
                r.PorcentajeAplicable
            }));
        }

        // POST /api/promociones/{id}/msi
        private static async Task<IResult> AgregarMSI(
            Guid id,
            AgregarMSIDto dto,
            PromocionesDbContext db,
            CancellationToken ct)
        {
            var promo = await db.Promociones
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (promo is null)
                return Results.NotFound(new { error = $"No se encontró la campaña con ID {id}" });

            var resultado = promo.AgregarOpcionMSI(
                dto.Meses,
                dto.BancosParticipantes,
                dto.MontoMinimoCompra
            );

            if (!resultado.EsExitoso)
                return Results.BadRequest(new { error = resultado.Error });

            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/promociones/{id}/msi",
                new { mensaje = "Opción MSI agregada correctamente", idPromocion = id });
        }

        // GET /api/promociones/{id}/msi
        private static async Task<IResult> ObtenerMSI(
            Guid id,
            PromocionesDbContext db,
            CancellationToken ct)
        {
            var promo = await db.Promociones
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (promo is null)
                return Results.NotFound(new { error = $"No se encontró la campaña con ID {id}" });

            return Results.Ok(promo.OpcionesMSI.Select(m => new
            {
                m.Id,
                m.Meses,
                m.BancosParticipantes,
                m.MontoMinimoCompra
            }));
        }

        // POST /api/promociones/aplicar
        // Este es el endpoint que consume el API Gateway al momento del checkout
        private static async Task<IResult> AplicarDescuentos(
            AplicarDescuentosDto dto,
            PromocionesDbContext db,
            CancellationToken ct)
        {
            var promocionesVigentes = await db.Promociones
                .AsNoTracking()
                .ToListAsync(ct);

            promocionesVigentes = promocionesVigentes
                .Where(p => p.EstaVigente())
                .ToList();

            var descuentoTotal = 0m;
            var promocionesAplicadas = new List<object>();

            foreach (var item in dto.Items)
            {
                var porcentaje = 0m;
                string? campanaAplicada = null;

                // Buscar regla específica por categoría primero
                foreach (var promo in promocionesVigentes)
                {
                    var regla = promo.ReglasCategorias
                        .FirstOrDefault(r => r.IdCategoriaCatalogo == item.IdCategoria);

                    if (regla is not null)
                    {
                        porcentaje = regla.PorcentajeAplicable;
                        campanaAplicada = promo.NombreCampana;
                        break;
                    }
                }

                // Si no hay regla por categoría, aplicar descuento general
                // según reglas de negocio del profesor
                if (porcentaje == 0 && dto.MontoTotal >= 10000)
                {
                    porcentaje = item.EsElectronica ? 5 : 10;
                    campanaAplicada = "Descuento por monto mayor a $10,000";
                }

                if (porcentaje > 0)
                {
                    var montoItem = item.PrecioUnitario * item.Cantidad;
                    var descuentoItem = montoItem * (porcentaje / 100);
                    descuentoTotal += descuentoItem;

                    promocionesAplicadas.Add(new
                    {
                        idProducto = item.IdProducto,
                        campana = campanaAplicada,
                        porcentaje,
                        descuentoAplicado = descuentoItem
                    });
                }
            }

            // Opciones MSI disponibles para el monto total
            var msiDisponibles = promocionesVigentes
                .SelectMany(p => p.OpcionesMSI)
                .Where(m => dto.MontoTotal >= m.MontoMinimoCompra)
                .Select(m => new
                {
                    m.Meses,
                    m.BancosParticipantes,
                    m.MontoMinimoCompra
                })
                .Distinct()
                .ToList();

            return Results.Ok(new
            {
                montoOriginal = dto.MontoTotal,
                descuentoTotal,
                montoFinal = dto.MontoTotal - descuentoTotal,
                promocionesAplicadas,
                opcionesMSI = msiDisponibles
            });
        }

        // Mapper interno
        private static object MapearPromocion(Promocion p) => new
        {
            p.Id,
            p.NombreCampana,
            p.PorcentajeDescuento,
            p.MontoMinimoCompra,
            p.FechaInicio,
            p.FechaFin,
            p.EsIndefinido,
            estaVigente = p.EstaVigente(),
            totalReglas = p.ReglasCategorias.Count,
            totalMSI = p.OpcionesMSI.Count
        };
    }
}

using Promociones.Infrastructure;
using Promociones.API.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PromocionesDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PromocionesDB"),
        npgsql => npgsql.MigrationsAssembly("Promociones.Infrastructure")
    )
    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Promociones API",
        Version = "v1",
        Description = "Microservicio de gestión de promociones, descuentos y MSI"
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PromocionesDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Promociones API v1"));
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Promociones.API",
    timestamp = DateTime.UtcNow
})).WithTags("Health");

app.MapPromocionesEndpoints();

app.Run();
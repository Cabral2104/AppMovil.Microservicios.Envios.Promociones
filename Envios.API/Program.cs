using Envios.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Envios.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Base de datos
builder.Services.AddDbContext<EnviosDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("EnviosDB"),
        npgsql => npgsql.MigrationsAssembly("Envios.Infrastructure")
    )
);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Envíos API",
        Version = "v1",
        Description = "Microservicio de gestión de envíos y rastreo"
    });
});

var app = builder.Build();

// Migraciones automáticas al iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EnviosDbContext>();
    db.Database.Migrate();
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Envíos API v1"));
}

app.UseHttpsRedirection();

// Endpoints
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Envios.API",
    timestamp = DateTime.UtcNow
})).WithTags("Health");

app.MapEnviosEndpoints();

app.Run();
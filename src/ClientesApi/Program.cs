using ClientesApi.Data;
using ClientesApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ClientesDb>(o => o.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=clientes.db"));
var app = builder.Build();
using (var scope = app.Services.CreateScope()) { scope.ServiceProvider.GetRequiredService<ClientesDb>().Database.EnsureCreated(); }

var clientes = app.MapGroup("/api/clientes");
clientes.MapGet("/", async (ClientesDb db) => Results.Ok(await db.Clientes.AsNoTracking().OrderBy(c => c.Id).ToListAsync()));
clientes.MapGet("/{id:int}", async (int id, ClientesDb db) => await db.Clientes.FindAsync(id) is Cliente c ? Results.Ok(c) : Results.NotFound(new { mensaje = "Cliente no encontrado" }));
clientes.MapPost("/", async (Cliente cliente, ClientesDb db) => {
    if (string.IsNullOrWhiteSpace(cliente.Nombre) || string.IsNullOrWhiteSpace(cliente.Email)) return Results.BadRequest(new { mensaje = "Nombre y email son requeridos" });
    if (await db.Clientes.AnyAsync(c => c.Email == cliente.Email)) return Results.Conflict(new { mensaje = "El email ya existe" });
    db.Clientes.Add(cliente); await db.SaveChangesAsync(); return Results.Created($"/api/clientes/{cliente.Id}", cliente);
});
clientes.MapPut("/{id:int}", async (int id, Cliente input, ClientesDb db) => {
    var c = await db.Clientes.FindAsync(id); if (c is null) return Results.NotFound(new { mensaje = "Cliente no encontrado" });
    if (string.IsNullOrWhiteSpace(input.Nombre) || string.IsNullOrWhiteSpace(input.Email)) return Results.BadRequest(new { mensaje = "Nombre y email son requeridos" });
    if (await db.Clientes.AnyAsync(x => x.Id != id && x.Email == input.Email)) return Results.Conflict(new { mensaje = "El email ya existe" });
    c.Nombre=input.Nombre; c.Email=input.Email; c.Telefono=input.Telefono; c.Direccion=input.Direccion; await db.SaveChangesAsync(); return Results.Ok(c);
});
clientes.MapDelete("/{id:int}", async (int id, ClientesDb db) => { var c=await db.Clientes.FindAsync(id); if(c is null) return Results.NotFound(new { mensaje="Cliente no encontrado" }); db.Remove(c); await db.SaveChangesAsync(); return Results.NoContent(); });
app.Run();

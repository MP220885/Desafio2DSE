using PedidosApi.Data;
using PedidosApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PedidosDb>(o => o.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=pedidos.db"));
builder.Services.AddHttpClient("clientes", c => c.BaseAddress = new Uri(builder.Configuration["ClientesApiUrl"] ?? "http://localhost:5001"));
var app = builder.Build();
using (var scope = app.Services.CreateScope()) { scope.ServiceProvider.GetRequiredService<PedidosDb>().Database.EnsureCreated(); }
var pedidos=app.MapGroup("/api/pedidos");
pedidos.MapGet("/", async(PedidosDb db)=>Results.Ok(await db.Pedidos.AsNoTracking().OrderByDescending(p=>p.Fecha).ToListAsync()));
pedidos.MapGet("/{id:int}",async(int id,PedidosDb db)=>await db.Pedidos.FindAsync(id) is Pedido p?Results.Ok(p):Results.NotFound(new {mensaje="Pedido no encontrado"}));
pedidos.MapGet("/cliente/{clienteId:int}",async(int clienteId,PedidosDb db)=>Results.Ok(await db.Pedidos.AsNoTracking().Where(p=>p.ClienteId==clienteId).OrderByDescending(p=>p.Fecha).ToListAsync()));
pedidos.MapPost("/",async(Pedido pedido,PedidosDb db,IHttpClientFactory factory)=>{ if(string.IsNullOrWhiteSpace(pedido.Descripcion)||pedido.Total<=0) return Results.BadRequest(new {mensaje="Descripción y total válido son requeridos"}); var response=await factory.CreateClient("clientes").GetAsync($"/api/clientes/{pedido.ClienteId}"); if(!response.IsSuccessStatusCode)return Results.BadRequest(new {mensaje="El cliente asociado no existe"}); pedido.Fecha=DateTime.UtcNow; db.Pedidos.Add(pedido);await db.SaveChangesAsync();return Results.Created($"/api/pedidos/{pedido.Id}",pedido); });
app.Run();

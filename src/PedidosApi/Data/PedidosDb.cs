using Microsoft.EntityFrameworkCore;
using PedidosApi.Models;
namespace PedidosApi.Data;
public class PedidosDb(DbContextOptions<PedidosDb> options):DbContext(options){public DbSet<Pedido> Pedidos=>Set<Pedido>(); protected override void OnModelCreating(ModelBuilder b){b.Entity<Pedido>().Property(p=>p.Descripcion).HasMaxLength(250).IsRequired(); b.Entity<Pedido>().Property(p=>p.Total).HasPrecision(12,2); b.Entity<Pedido>().HasIndex(p=>p.ClienteId);}}

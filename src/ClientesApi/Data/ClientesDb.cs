using ClientesApi.Models;
using Microsoft.EntityFrameworkCore;
namespace ClientesApi.Data;
public class ClientesDb(DbContextOptions<ClientesDb> options) : DbContext(options) { public DbSet<Cliente> Clientes => Set<Cliente>(); protected override void OnModelCreating(ModelBuilder b) { b.Entity<Cliente>().HasIndex(c=>c.Email).IsUnique(); b.Entity<Cliente>().Property(c=>c.Nombre).HasMaxLength(120).IsRequired(); b.Entity<Cliente>().Property(c=>c.Email).HasMaxLength(160).IsRequired(); } }

using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Dsw2026Tpi.Data;

public class Dsw2026TpiDbContext: DbContext
{
    
    public Dsw2026TpiDbContext(DbContextOptions<Dsw2026TpiDbContext> options):
        base(options)
    {
    }
    public DbSet<Disponibilidad> Disponibilidades { get; set; }
    public DbSet<Turno> Turnos { get; set; }
    public DbSet<Doctor> Doctores { get; set; }
    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<Speciality> Especialidades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

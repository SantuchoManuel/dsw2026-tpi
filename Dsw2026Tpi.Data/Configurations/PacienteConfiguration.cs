using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("Pacientes");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Dni)
               .IsRequired();

        builder.Property(p => p.Email)
               .HasMaxLength(150)
               .IsRequired();

        builder.Property(p => p.Name)
               .HasMaxLength(100);

        builder.Property(p => p.Cellnumber)
               .HasMaxLength(20);
        builder.HasQueryFilter(p => !p.Deleted);
    }
}
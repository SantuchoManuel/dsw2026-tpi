using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Data.Configurations
{
    public class CitaConfiguration : IEntityTypeConfiguration<Cita>
    {
        public void Configure(EntityTypeBuilder<Cita> builder)
        {
            builder.ToTable("Citas");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.CitaEstado).IsRequired();

            builder.HasOne(c => c.Paciente)
                   .WithMany()
                   .HasForeignKey(c => c.PacienteId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Turno)
                   .WithOne()
                   .HasForeignKey<Cita>(c => c.TurnoId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

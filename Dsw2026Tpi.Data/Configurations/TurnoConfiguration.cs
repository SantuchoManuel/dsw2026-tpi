using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Data.Configurations
{
    public class TurnoConfiguration : IEntityTypeConfiguration<Turno>
    {
        public void Configure(EntityTypeBuilder<Turno> builder)
        {
            builder.ToTable("Turnos");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Fecha).IsRequired();
            builder.Property(t => t.HoraDeInicio).IsRequired();
            builder.Property(t => t.HoraDeFin).IsRequired();
            builder.Property(t => t.EstadoTurno).IsRequired();

            builder.HasOne(t => t.Disponibilidad)
                   .WithMany()
                   .HasForeignKey(t => t.DisponibilidadId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Data.Configurations
{
    public class DisponibilidadConfiguration : IEntityTypeConfiguration<Disponibilidad>
    {
        public void Configure(EntityTypeBuilder<Disponibilidad> builder)
        {
            builder.ToTable("Disponibilidades");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Mes).IsRequired();
            builder.Property(d => d.Año).IsRequired();
            builder.Property(d => d.DiaDeLaSemana).IsRequired();
            builder.Property(d => d.HoraDeEntrada).IsRequired();
            builder.Property(d => d.HoraDeSalida).IsRequired();

            builder.HasOne(d => d.Doctor)
                   .WithMany()
                   .HasForeignKey(d => d.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

}

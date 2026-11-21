using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Data.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Status).IsRequired();

            builder.HasOne(c => c.Patient)
                   .WithMany()
                   .HasForeignKey(c => c.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.AvailabilitySlot)
                   .WithOne()
                   .HasForeignKey<Appointment>(c => c.AvailabilitySlotId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

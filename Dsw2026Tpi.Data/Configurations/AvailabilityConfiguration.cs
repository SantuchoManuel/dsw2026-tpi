using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Data.Configurations
{
    public class AvailabilityConfiguration : IEntityTypeConfiguration<Availability>
    {
        public void Configure(EntityTypeBuilder<Availability> builder)
        {
            builder.ToTable("Availabilities");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Month).IsRequired();
            builder.Property(d => d.Year).IsRequired();
            builder.Property(d => d.DayOfWeek).IsRequired();
            builder.Property(d => d.StartTime).IsRequired();
            builder.Property(d => d.EndTime).IsRequired();

            builder.HasOne(d => d.Doctor)
                   .WithMany()
                   .HasForeignKey(d => d.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasQueryFilter(d => !d.Deleted);
        }
    }

}

using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Data.Configurations
{
    public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
    {
        public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
        {
            builder.ToTable("AvailabilitySlots");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Date).IsRequired();
            builder.Property(t => t.StartTime).IsRequired();
            builder.Property(t => t.EndTime).IsRequired();
            builder.Property(t => t.Status).IsRequired()
                   .HasConversion<string>()
                   .IsRequired();

            builder.HasOne(t => t.Availability)
                   .WithMany(d => d.AvailabilitySlots)
                   .HasForeignKey(t => t.AvailabilityId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasQueryFilter(t => !t.Deleted);
        }
    }
}

using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

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
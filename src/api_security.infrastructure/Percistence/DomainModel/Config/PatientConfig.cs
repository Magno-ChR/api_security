using api_security.domain.Entities.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api_security.infrastructure.Percistence.DomainModel.Config;

internal class PatientConfig : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patient");
        builder.HasKey(x => x.Id).HasName("PatientId");

        builder.Property(p => p.Id)
            .HasColumnName("PatientId");

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
    }
}

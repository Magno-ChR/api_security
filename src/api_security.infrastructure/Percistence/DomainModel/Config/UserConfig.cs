using api_security.domain.Entities.Credentials;
using api_security.domain.Entities.UserRoles;
using api_security.domain.Entities.Users;
using api_security.domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_security.infrastructure.Percistence.DomainModel.Config;

internal class UserConfig : IEntityTypeConfiguration<User>,
    IEntityTypeConfiguration<UserRole>,
    IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");
        builder.HasKey(x => x.Id).HasName("UserId");

        builder.Property(p => p.Id)
            .HasColumnName("UserId");

        builder.HasMany("_userRoles");
        builder.HasMany("_credentials");

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.UserRoles);
        builder.Ignore(x => x.Credentials);
    }

    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRole");
        builder.HasKey(x => x.Id).HasName("UserRoleId");

        builder.Property(p => p.Id)
            .HasColumnName("UserRoleId");

        var roleTypeConverter = new ValueConverter<RoleType, string>(
        v => v.ToString(),
        v => (RoleType)Enum.Parse(typeof(RoleType), v));

        builder.Property(p => p.Role)
            .HasConversion(roleTypeConverter)
            .HasColumnName("Role");

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
    }

    public void Configure(EntityTypeBuilder<Credential> builder)
    {
        builder.ToTable("Credential");
        builder.HasKey(x => x.Id).HasName("CredentialId");

        builder.Property(p => p.Id)
            .HasColumnName("CredentialId");

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
    }

}

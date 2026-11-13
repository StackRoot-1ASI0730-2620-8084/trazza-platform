using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Iam.Domain.Model.Aggregates;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

namespace Trazza.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>
///     Mapping of the IAM &amp; Profiles tables of <c>trazza_database</c>. <c>saved_addresses</c> is handled with SQL.
/// </summary>
public static class ModelBuilderExtensions
{
    public static void ApplyIamConfiguration(this ModelBuilder builder)
    {
        builder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Id).ValueGeneratedOnAdd();
            entity.Property(user => user.PublicId).HasPublicId();
            entity.Property(user => user.Email).HasMaxLength(255).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Ignore(user => user.CanSignIn);
            entity.HasMany(user => user.Roles).WithOne().HasForeignKey(role => role.UserId);
            entity.Navigation(user => user.Roles).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(role => new { role.UserId, role.Role });
            entity.Property(role => role.Role).HasEnumText();
        });

        builder.Entity<PersonIdentity>(entity =>
        {
            entity.ToTable("person_identities");
            entity.HasKey(identity => identity.UserId);
            entity.Property(identity => identity.UserId).ValueGeneratedNever();
            entity.Property(identity => identity.FullName).HasMaxLength(200).IsRequired();
            entity.Property(identity => identity.KycStatus).HasEnumText();
        });

        builder.Entity<IdentityVerification>(entity =>
        {
            entity.ToTable("identity_verifications");
            entity.HasKey(verification => verification.Id);
            entity.Property(verification => verification.Id).ValueGeneratedOnAdd();
            entity.Property(verification => verification.Status).HasEnumText();
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(token => token.Id);
            entity.Property(token => token.Id).ValueGeneratedOnAdd();
        });

        builder.Entity<CarrierProfile>(entity =>
        {
            entity.ToTable("carrier_profiles");
            entity.HasKey(profile => profile.Id);
            entity.Property(profile => profile.Id).ValueGeneratedOnAdd();
        });

        builder.Entity<MerchantProfile>(entity =>
        {
            entity.ToTable("merchant_profiles");
            entity.HasKey(profile => profile.Id);
            entity.Property(profile => profile.Id).ValueGeneratedOnAdd();
        });

        builder.Entity<Driver>(entity =>
        {
            entity.ToTable("drivers");
            entity.HasKey(driver => driver.Id);
            entity.Property(driver => driver.Id).ValueGeneratedOnAdd();
            entity.Property(driver => driver.Sex).HasNullableEnumText();
        });

        builder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("vehicles");
            entity.HasKey(vehicle => vehicle.Id);
            entity.Property(vehicle => vehicle.Id).ValueGeneratedOnAdd();
            entity.Property(vehicle => vehicle.PublicId).HasPublicId();
        });
    }
}

using Microsoft.EntityFrameworkCore;
using TrazzaPlatform.API.Reputation.Domain.Model.Aggregates;
using TrazzaPlatform.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

namespace TrazzaPlatform.API.Reputation.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>Mapping of the Loyalty &amp; Reputation tables.</summary>
public static class ModelBuilderExtensions
{
    public static void ApplyReputationConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Rating>(entity =>
        {
            entity.ToTable("ratings");
            entity.HasKey(rating => rating.Id);
            entity.Property(rating => rating.Id).ValueGeneratedOnAdd();
            entity.Property(rating => rating.Comment).HasMaxLength(Rating.MaxCommentLength);
        });

        builder.Entity<LoyaltyPointEntry>(entity =>
        {
            entity.ToTable("loyalty_point_entries");
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.Id).ValueGeneratedOnAdd();
            entity.Property(entry => entry.Reason).HasEnumText();
        });
    }
}
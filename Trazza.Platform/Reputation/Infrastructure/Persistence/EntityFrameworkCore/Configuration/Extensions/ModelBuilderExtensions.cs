using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Reputation.Domain.Model.Aggregates;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

namespace Trazza.Platform.Reputation.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

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
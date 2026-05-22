using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication2.Entities;

namespace WebApplication2.Configurations;

public class UserEventConfiguration:IEntityTypeConfiguration<UserEvent>
{
    public void Configure(EntityTypeBuilder<UserEvent> builder)
    {
        builder.HasKey(ue => new { ue.AppUserId, ue.EventId });

        builder.HasOne(ue => ue.AppUser)
            .WithMany(u => u.UserEvents)
            .HasForeignKey(ue => ue.AppUserId);

        builder.HasOne(ue => ue.Event)
            .WithMany(e => e.UserEvents)
            .HasForeignKey(ue => ue.EventId);
    }
}
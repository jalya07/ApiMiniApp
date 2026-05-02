using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication2.Entities;

namespace WebApplication2.Configurations;

public class EventConfiguration:IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.EventId);
 
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(150);
 
        builder.Property(e => e.Description)
            .HasMaxLength(500);
 
        builder.Property(e => e.Location)
            .IsRequired()
            .HasMaxLength(200);
 
        builder.Property(e => e.BannerImageUrl)
            .HasMaxLength(500);
 
        builder.Property(e => e.Date)
            .IsRequired();
 
        builder.HasOne(e => e.Organizer)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
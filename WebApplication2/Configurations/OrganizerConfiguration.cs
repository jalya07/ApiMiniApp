using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication2.Entities;

namespace WebApplication2.Configurations;

public class OrganizerConfiguration:IEntityTypeConfiguration<Organizer>

{
    public void Configure(EntityTypeBuilder<Organizer> builder)
    {
        builder.HasKey(o => o.OrganizerId);
 
        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(100);
 
        builder.Property(o => o.Email)
            .IsRequired()
            .HasMaxLength(256);
 
        builder.HasIndex(o => o.Email)
            .IsUnique();
 
        builder.Property(o => o.Phone)
            .HasMaxLength(20);
 
        builder.Property(o => o.LogoUrl)
            .HasMaxLength(500);
    }
}
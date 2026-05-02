using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication2.Entities;

namespace WebApplication2.Configurations;

public class TicketConfiguration:IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.TicketId);
 
        builder.Property(t => t.Type)
            .IsRequired()
            .HasMaxLength(50);
 
        builder.Property(t => t.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
 
        builder.Property(t => t.QuantityAvailable)
            .IsRequired();
 
        builder.HasOne(t => t.Event)
            .WithMany(e => e.Tickets)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
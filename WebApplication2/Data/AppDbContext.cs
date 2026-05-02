using Microsoft.EntityFrameworkCore;
using WebApplication2.Configurations;
using WebApplication2.Entities;

namespace WebApplication2.Data;

public class AppDbContext:DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
 
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Organizer> Organizers => Set<Organizer>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizerConfiguration());
        modelBuilder.ApplyConfiguration(new TicketConfiguration());
    }
}
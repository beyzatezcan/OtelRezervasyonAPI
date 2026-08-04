using Microsoft.EntityFrameworkCore;

namespace otelrezervation.Models;

public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<SiteSetting> SiteSettings { get; set; }

}

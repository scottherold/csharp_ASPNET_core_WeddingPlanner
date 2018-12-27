using Microsoft.EntityFrameworkCore;

namespace WeddingPlanner.Models
{
    // Context named generically to be used with multiple models/controllers
    public class HomeContext : DbContext
    {
        public HomeContext(DbContextOptions<HomeContext> options) : base(options) { }
        // Reference for queries using the User.cs model.
        public DbSet<User> Users { get; set; }
        public DbSet<Wedding> Weddings {get; set; }
        public DbSet<Attendee> Attendees { get; set; }
    }
}
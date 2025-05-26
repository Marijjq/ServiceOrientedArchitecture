using EventPlanner.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RSVP> RSVPs { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Invite -> Inviter (User)
            modelBuilder.Entity<Invite>()
                .HasOne(i => i.Inviter)
                .WithMany() // No navigation property in User
                .HasForeignKey(i => i.InviterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Invite -> Invitee (User)
            modelBuilder.Entity<Invite>()
                .HasOne(i => i.Invitee)
                .WithMany() // No navigation property in User
                .HasForeignKey(i => i.InviteeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@eventplanner.com",
                    Password = "Admin123!",
                    FirstName = "Admin",
                    LastName = "User",
                    Role = "Admin",
                    PhoneNumber = "1234567890"
                },
                new User
                {
                    Id = 2,
                    Username = "john.doe",
                    Email = "john.doe@example.com",
                    Password = "Password1!",
                    FirstName = "John",
                    LastName = "Doe",
                    Role = "User",
                    PhoneNumber = "0987654321"
                },
                new User
                {
                    Id = 3,
                    Username = "jane.smith",
                    Email = "jane.smith@example.com",
                    Password = "Password2!",
                    FirstName = "Jane",
                    LastName = "Smith",
                    Role = "User",
                    PhoneNumber = "1122334455"
                }
            );
        }
    }
}

        

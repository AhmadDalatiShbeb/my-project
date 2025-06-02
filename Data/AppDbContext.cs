using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ticket_Booking_System.Model;

namespace Ticket_Booking_System.Data
{
        public class AppDbContext : IdentityDbContext <User , IdentityRole , string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Seat> Seats => Set<Seat>();
        public DbSet<Ticket> Tickets { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
          
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
       .HasOne(b => b.User)
       .WithMany(u => u.Bookings)
       .HasForeignKey(b => b.UserId)
       .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
        .HasOne(t => t.Booking)
        .WithOne(b => b.Ticket)
        .HasForeignKey<Ticket>(t => t.BookingId);

        }
    }


}


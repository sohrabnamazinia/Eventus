using CodeFirstStoreFunctions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<EventImage> EventImages { get; set; }
        public DbSet<EventUserRole> EventUserRole { get; set; }
        public DbSet<UserImage> UsersImage { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Event>().HasOne<AppUser>(s => s.Creator).WithMany(x => x.CreatedEvents);
            modelBuilder.Entity<AppUser>().HasOne(x => x.Image).WithOne(x => x.User).HasForeignKey<UserImage>(x => x.UserId);
            modelBuilder.Entity<EventUserRole>().HasKey(o => new {o.AppUserId, o.EventId});
            modelBuilder.Entity<Ticket>().HasOne(x => x.Type).WithMany(x => x.Tickets).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Event>().HasQueryFilter(x => EF.Property<bool>(x, "IsDeleted") == false);
            modelBuilder.Entity<EventUserRole>().HasQueryFilter(x => EF.Property<bool>(x, "IsDeleted") == false);
            modelBuilder.Entity<Task>().HasQueryFilter(x => EF.Property<bool>(x, "IsDeleted") == false);
        }

    }
}

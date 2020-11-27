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
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<UserImage> UsersImage { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Event>().HasOne<AppUser>(s => s.Creator).WithMany(x => x.CreatedEvents);
            modelBuilder.Entity<AppUser>().HasOne(x => x.Image).WithOne(x => x.User).HasForeignKey<UserImage>(x => x.UserId);
        }

    }
}

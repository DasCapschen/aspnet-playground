using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using src.Areas.BirdVoice.Models;
using src.Areas.ActiviyProtocol.Models;
using src.Areas.Identity.Data;
using src.Models;

namespace src.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //okay, so, owned types are ALWAYS queried automatically...
            //in general, owned types kind of make this stupidly complicated...
            //so, let's revert back to HasMany :)

            //using Owner is okay for Activity Protocol, because we *never* query it without its entries.
            builder.Entity<ActivityProtocol>().OwnsMany(p => p.Entries).WithOwner(e => e.Protocol);

            builder.Entity<UserActiveBird>(config => {
                config.HasOne(ab => ab.Bird).WithMany().IsRequired();
                config.HasKey(ab => new { ab.UserId, ab.BirdId }); 
            });
            builder.Entity<UserBirdStats>(config => {
                config.HasOne(bs => bs.Bird).WithMany().IsRequired();
                config.HasKey(bs => new { bs.UserId, bs.BirdId });
            });

            builder.Entity<ApplicationUser>(user => {
                user.HasMany(u => u.Protocols).WithOne(p => p.User).IsRequired();
                user.HasMany(u => u.ActiveBirds).WithOne(ab => ab.User).IsRequired();
                user.HasMany(u => u.BirdStats).WithOne(bs => bs.User).IsRequired();
            });
        }

        public DbSet<Areas.BirdVoice.Models.BirdNames> BirdNames { get; set; }

        //not having public accessors for these, because they should only be accessed through user!
        //public DbSet<Areas.BirdVoice.Models.UserBirdStats> UserBirdStats { get; set; }
        //public DbSet<Areas.BirdVoice.Models.UserActiveBird> UserActiveBirds { get; set; }
        //public DbSet<Models.ActivityProtocol> ActivityProtocols { get; set; }
    }
}

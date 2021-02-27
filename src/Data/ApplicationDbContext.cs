using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

            builder.Entity<ApplicationUser>(user_config => {
                //each user owns many protocols
                user_config.OwnsMany(
                    user => user.Protocols,
                    protocol_config => {
                        //protocol has owner 
                        protocol_config.WithOwner(protocol => protocol.Owner);
                        //protocol owns many entries
                        protocol_config.OwnsMany(protocol => protocol.Entries).WithOwner(entry => entry.Protocol);
                    }
                );
                //1 user owns many active birds (list of birds this user is actively learning)
                user_config.OwnsMany(
                    u => u.ActiveBirds,
                    ab_config => {
                        ab_config.WithOwner(ab => ab.User);
                        ab_config.HasOne(ab => ab.Bird);
                    }   
                );
                //1 user owns many bird stats (stats about birds this user learned)
                user_config.OwnsMany(
                    u => u.BirdStats,
                    bs_config => {
                        //each "bird stats" has 1 user and 1 bird
                        bs_config.WithOwner(bs => bs.User);
                        bs_config.HasOne(bs => bs.Bird);
                    }
                );
            });
        }

        //"must be accessed through owning entity"
        //public DbSet<Models.ActivityProtocol.ProtocolEntry> ProtocolEntries { get; set; }
        //public DbSet<Models.ActivityProtocol> ActivityProtocols { get; set; }

        public DbSet<Areas.BirdVoice.Models.BirdNames> BirdNames { get; set; }
        public DbSet<Areas.BirdVoice.Models.UserBirdStats> UserBirdStats { get; set; }
        public DbSet<Areas.BirdVoice.Models.UserActiveBird> UserActiveBirds { get; set; }
    }
}

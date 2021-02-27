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
            });
        }

        public DbSet<src.Models.ActivityProtocol.ProtocolEntry> ProtocolEntries { get; set; }
        public DbSet<src.Models.ActivityProtocol> ActivityProtocols { get; set; }
    }
}

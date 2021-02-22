using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using src.Models;

namespace src.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<src.Models.ActivityProtocol>(config => {
                //each protocol owns many entries
                config.OwnsMany(p => p.Entries).WithOwner(e => e.Protocol);
                //and has an owner, but owner has no reference to the protocol!
                config.HasOne(p => p.Owner).WithMany().IsRequired();
            });
        }

        public DbSet<src.Models.ActivityProtocol.ProtocolEntry> ProtocolEntries { get; set; }
        public DbSet<src.Models.ActivityProtocol> ActivityProtocols { get; set; }
    }
}

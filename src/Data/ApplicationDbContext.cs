using System;
using System.Collections.Generic;
using System.Text;
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

            builder.Entity<src.Models.ActivityProtocol.ProtocolEntry>()
                .HasOne(e => e.Protocol) //configure that entry must have *one* protocol
                .WithMany(p => p.Entries) //configure that protocol may have *many* entries
                .IsRequired(); //foreign key may NOT be NULL!
        }

        public DbSet<src.Models.ActivityProtocol.ProtocolEntry> ProtocolEntries { get; set; }
        public DbSet<src.Models.ActivityProtocol> ActivityProtocols { get; set; }
    }
}

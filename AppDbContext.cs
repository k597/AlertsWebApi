
using Microsoft.EntityFrameworkCore;
using ObWebApi3;
using System.Net;

public class AppDbContext : DbContext
{
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<IpAddress> IpAddresses { get; set; }
    public DbSet<AlertIpAddress> AlertIpAddresses { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Alerts table
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(a => a.Id); 
            entity.Property(a => a.Title).IsRequired().HasMaxLength(255); 
            entity.Property(a => a.Description).IsRequired().HasMaxLength(1000); 
            entity.Property(a => a.Severity).IsRequired().HasMaxLength(50); 
            entity.Property(a => a.Count).IsRequired().HasDefaultValue(1); 
        });

        // Configure IpAddresses table
        modelBuilder.Entity<IpAddress>(entity =>
        {
            entity.HasKey(i => i.Id); 
            entity.Property(i => i.Address).IsRequired().HasMaxLength(50); 
            entity.Property(i => i.SourceType).IsRequired().HasMaxLength(50); 
            entity.Property(i => i.Blacklisted).IsRequired().HasDefaultValue(false); 
            entity.Property(i => i.Count).IsRequired().HasDefaultValue(1); 

            // Configure Many-to-Many Relationship using AlertIpAddress
            modelBuilder.Entity<AlertIpAddress>(entity =>
            {
                entity.HasKey(aip => new { aip.AlertId, aip.IpAddressId });

                entity.HasOne(aip => aip.Alert)
                    .WithMany(a => a.AlertIpAddresses)
                    .HasForeignKey(aip => aip.AlertId);

                entity.HasOne(aip => aip.IpAddress)
                    .WithMany(ip => ip.AlertIpAddresses)
                    .HasForeignKey(aip => aip.IpAddressId);
            });
        });
    }
}



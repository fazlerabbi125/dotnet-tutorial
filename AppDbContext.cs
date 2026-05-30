using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TutorialProj.Models;
namespace TutorialProj;

// Fluent API
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<Order> Orders { get; set; }

    // Fallback connection string — only used when the context is created WITHOUT DI
    // (e.g. by EF migration tools at design-time). At runtime, Program.cs supplies
    // the connection string via DbContextOptions, making this block a no-op.
    // https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#onconfiguring
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
            options.UseSqlite("Data Source=logitrack.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Required for Identity table configurations (AspNetUsers, AspNetRoles, etc.)
        base.OnModelCreating(modelBuilder);
        
        // For enforcing rules and configuration in db
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            // Explicitly define primary key to resolve EF Core error
            entity.HasKey(i => i.ItemId);
            
            entity.Property(i => i.Name)
                .HasMaxLength(150)
                .IsRequired();
            entity.Property(i => i.Quantity)
                .HasDefaultValue(0);
            entity.Property(i => i.Location)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            // Explicitly define primary key to resolve EF Core error
            entity.HasKey(o => o.OrderId);
            
            entity.Property(o => o.CustomerName)
                .HasMaxLength(150)
                .IsRequired();
            entity.Property(o => o.DatePlaced)
                .IsRequired();

            entity.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override int SaveChanges()
    {
        ApplyTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTimestamps()
    {
        // 1. Filter only entries that inherit from TimeStampMixin and are being Added or Modified
        var entries = ChangeTracker.Entries<TimeStampMixin>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            // 2. ONLY set CreatedAt if the record is brand new
            if (entry.State == EntityState.Added)
            {
                entry.Property(x => x.CreatedAt).CurrentValue = utcNow;
            }
            else
            {
                // Prevent EF Core from trying to update or overwrite the existing CreatedAt value in SQL
                entry.Property(x => x.CreatedAt).IsModified = false;
            }

            // 3. Always update UpdatedAt regardless of Added or Modified states
            entry.Property(x => x.UpdatedAt).CurrentValue = utcNow;
        }
    }
}
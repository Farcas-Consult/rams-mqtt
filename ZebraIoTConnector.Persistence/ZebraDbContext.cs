using Microsoft.EntityFrameworkCore;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Persistence
{
    public class ZebraDbContext : DbContext
    {
        public ZebraDbContext() { }

        public ZebraDbContext(DbContextOptions<ZebraDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=sql.data,1433;Database=ZebraRFID_DockDoor;MultipleActiveResultSets=true;User ID=sa;Password=Zebra2022!");
            }
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Existing InventoryOperation relationships
            modelBuilder.Entity<InventoryOperation>()
                .HasOne(e => e.StorageUnitFrom)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<InventoryOperation>()
                .HasOne(e => e.StorageUnitTo)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<InventoryOperation>()
                .HasOne(e => e.Sublot)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<InventoryOperation>()
                .HasOne(e => e.Equipment)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            // Asset relationships
            modelBuilder.Entity<Asset>()
                .HasOne(a => a.CurrentLocation)
                .WithMany()
                .HasForeignKey(a => a.CurrentLocationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Asset>()
                .HasMany(a => a.Movements)
                .WithOne(m => m.Asset)
                .HasForeignKey(m => m.AssetId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure decimal precision for AcquisitionValue
            modelBuilder.Entity<Asset>()
                .Property(a => a.AcquisitionValue)
                .HasPrecision(18, 2);

            // Gate relationships
            modelBuilder.Entity<Gate>()
                .HasOne(g => g.Location)
                .WithMany()
                .HasForeignKey(g => g.LocationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Gate>()
                .HasMany(g => g.Readers)
                .WithOne(e => e.Gate)
                .HasForeignKey(e => e.GateId)
                .OnDelete(DeleteBehavior.NoAction);

            // AssetMovement relationships
            modelBuilder.Entity<AssetMovement>()
                .HasOne(m => m.FromLocation)
                .WithMany()
                .HasForeignKey(m => m.FromLocationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AssetMovement>()
                .HasOne(m => m.ToLocation)
                .WithMany()
                .HasForeignKey(m => m.ToLocationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AssetMovement>()
                .HasOne(m => m.Gate)
                .WithMany()
                .HasForeignKey(m => m.GateId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AssetMovement>()
                .HasOne(m => m.Reader)
                .WithMany()
                .HasForeignKey(m => m.ReaderId)
                .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Equipment> Equipments { get; internal set; }
        public DbSet<AssetTag> AssetTags { get; set; }
        public DbSet<StorageUnit> StorageUnits { get; internal set; }
        public DbSet<Sublot> Sublots { get; internal set; }
        public DbSet<InventoryOperation> InventoryOperation { get; internal set; }

        // Asset tracking entities
        public DbSet<Asset> Assets { get; internal set; }
        public DbSet<Gate> Gates { get; internal set; }
        public DbSet<Site> Sites { get; internal set; }
        public DbSet<AssetMovement> AssetMovements { get; internal set; }


    }
}
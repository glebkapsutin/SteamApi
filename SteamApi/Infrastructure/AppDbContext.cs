using Microsoft.EntityFrameworkCore;

namespace SteamApi.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Domain.Game> Games => Set<Domain.Game>();
        public DbSet<Domain.Tag> Tags => Set<Domain.Tag>();
        public DbSet<Domain.GameTag> GameTags => Set<Domain.GameTag>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.Game>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(512);
                e.Property(x => x.StoreUrl).HasMaxLength(1024);
                e.Property(x => x.ImageUrl).HasMaxLength(1024);
                e.HasIndex(x => x.ReleaseDate);
            });

            modelBuilder.Entity<Domain.Tag>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(128);
                e.HasIndex(x => x.Name).IsUnique();
            });

            modelBuilder.Entity<Domain.GameTag>(e =>
            {
                e.HasKey(x => new { x.GameId, x.TagId });
                e.HasOne(x => x.Game).WithMany(x => x.GameTags).HasForeignKey(x => x.GameId);
                e.HasOne(x => x.Tag).WithMany(x => x.GameTags).HasForeignKey(x => x.TagId);
            });
        }
    }
}



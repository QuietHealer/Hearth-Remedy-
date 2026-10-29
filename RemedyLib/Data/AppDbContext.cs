using Microsoft.EntityFrameworkCore;

namespace RemedyLib.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Title).IsRequired().HasMaxLength(100);
            e.Property(p => p.Description).IsRequired().HasMaxLength(300);
            e.Property(p => p.Ingredients).IsRequired().HasMaxLength(2000);
            e.Property(p => p.Steps).IsRequired().HasMaxLength(4000);
            e.Property(p => p.AuthorDeviceId).IsRequired().HasMaxLength(64);

            // Enums are stored as readable text ("SoreThroat"), not numbers.
            e.Property(p => p.Category).HasConversion<string>().HasMaxLength(40);
            e.Property(p => p.UsageMethod).HasConversion<string>().HasMaxLength(10);

            e.HasIndex(p => p.CreatedUtc);
            e.HasIndex(p => p.Category);
        });

        modelBuilder.Entity<Vote>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.DeviceId).IsRequired().HasMaxLength(64);
            // One vote per device per post, enforced by the database itself.
            e.HasIndex(v => new { v.PostId, v.DeviceId }).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("CK_Votes_Value", "\"Value\" IN (-1, 1)"));
            e.HasOne(v => v.Post).WithMany(p => p.Votes).HasForeignKey(v => v.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Report>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.DeviceId).IsRequired().HasMaxLength(64);
            e.Property(r => r.Reason).HasMaxLength(200);
            e.HasIndex(r => new { r.PostId, r.DeviceId }).IsUnique();
            e.HasOne(r => r.Post).WithMany(p => p.Reports).HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Pre-made posts. Fixed IDs and dates are required so migrations stay stable.
        var seedDate = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Post>().HasData(
            new Post
            {
                Id = new Guid("11111111-1111-1111-1111-111111111111"),
                Title = "Honey and lemon for a sore throat",
                Description = "A warm, soothing drink that coats the throat.",
                Category = Category.Throat,
                UsageMethod = UsageMethod.Eat,
                Ingredients = "1 tbsp honey\nJuice of half a lemon\n1 cup warm (not boiling) water",
                Steps = "Stir the honey and lemon juice into the warm water.\nSip slowly.\nDo not give honey to children under 1 year old.",
                CreatedUtc = seedDate,
                AuthorDeviceId = "seed"
            },
            new Post
            {
                Id = new Guid("22222222-2222-2222-2222-222222222222"),
                Title = "Steam for a stuffy nose",
                Description = "Warm steam helps loosen congestion.",
                Category = Category.ColdAndFlu,
                UsageMethod = UsageMethod.Breathe,
                Ingredients = "A bowl of hot water\nA towel",
                Steps = "Pour hot (not boiling) water into a bowl.\nLean over it at a safe distance with a towel over your head.\nBreathe slowly for 5 to 10 minutes.\nKeep children away from hot water.",
                CreatedUtc = seedDate.AddMinutes(1),
                AuthorDeviceId = "seed"
            },
            new Post
            {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                Title = "Oatmeal bath for itchy skin",
                Description = "A gentle soak that calms dry, irritated skin.",
                Category = Category.Skin,
                UsageMethod = UsageMethod.Skin,
                Ingredients = "1 cup plain oats, ground into a fine powder\nA tub of lukewarm water",
                Steps = "Stir the oat powder into lukewarm bathwater.\nSoak for 15 to 20 minutes.\nPat skin dry and moisturize.",
                CreatedUtc = seedDate.AddMinutes(2),
                AuthorDeviceId = "seed"
            });
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace HearthRemedies.Data
{
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Post> Posts => Set<Post>();
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Post>().HasData(
                new Post
                {
                    Id = 1,
                    Title = "Honey and lemon tea",
                    Ingredients = "Honey, lemon, hot water",
                    Steps = "Mix and sip warm.",
                    Category = "Cough",
                    UsageMethod = "eat",
                    CreatedAt = new DateTime(2026, 10, 1)
                }
            );
        }
        
    }
}

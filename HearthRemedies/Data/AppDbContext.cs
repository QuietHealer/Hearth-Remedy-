using System;
using System.Collections.Generic;
using System.Text;

namespace HearthRemedies.Data
{
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<RemedyPost> Posts => Set<RemedyPost>();
    }
}

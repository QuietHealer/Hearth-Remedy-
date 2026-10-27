using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RemedyLib.Data;

/// <summary>
/// Used only by EF Core tools (Add-Migration, Update-Database) so they can build
/// the context without starting the MAUI app.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(Secrets.ConnectionString)
            .Options;
        return new AppDbContext(options);
    }
}

using RemedyLib.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RemedyLib;
namespace HearthRemedies
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();



            // Database file stored in the app's private data folder
            //Old database code
            //var dbPath = Path.Combine(FileSystem.AppDataDirectory, "hearth.db");
            //builder.Services.AddDbContextFactory<AppDbContext>(options =>
            //    options.UseSqlite($"Filename={dbPath}"));
            //System.Diagnostics.Debug.WriteLine($"DB PATH: {dbPath}");
            builder.Services.AddDbContextFactory<AppDbContext>(o => o.UseNpgsql(Secrets.ConnectionString));
            builder.Services.AddSingleton<IDeviceIdProvider, PreferencesDeviceIdProvider>();
            builder.Services.AddSingleton<RemedyService>();
            //builder.Services.AddDbContext<AppDbContext>(options =>
            //    options.UseNpgsql(Secrets.ConnectionString));

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            var app = builder.Build();
            // Create the database on startup
            using (var scope = app.Services.CreateScope())
            {
                var factory = scope.ServiceProvider
                    .GetRequiredService<IDbContextFactory<AppDbContext>>();
                using var db = factory.CreateDbContext();
                db.Database.EnsureCreated();
            }
            return app;
        }
    }
}

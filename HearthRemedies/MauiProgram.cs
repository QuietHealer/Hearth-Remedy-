using RemedyLib.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using HearthRemedies.Model;
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



            
            builder.Services.AddDbContextFactory<AppDbContext>(o => o.UseNpgsql(Secrets.ConnectionString));
            builder.Services.AddSingleton<IDeviceIdProvider, PreferencesDeviceIdProvider>();
            builder.Services.AddSingleton<RemedyService>();
            

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<HearthRemedies.Controller.PostFilterState>();
            builder.Services.AddScoped<PostTransferService>();
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

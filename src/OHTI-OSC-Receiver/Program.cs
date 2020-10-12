
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenSoundControlBroadcastClient;

namespace OHTI_OSC_Receiver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Bind application settings from appsettings.json
                    services.Configure<ApplicationSettings>(hostContext.Configuration.GetSection("App"));

                    // Add the open sound control listener
                    services.AddSingleton<OpenSoundControlListener>();

                    // Add the open sound control broadcast listener
                    services.AddSingleton<UDPBroadcastReceiver>();

                    // Initiate background worker that keep things going
                    services.AddHostedService<Worker>();
                }).ConfigureWebHostDefaults((webBuilder) =>
                {
                    // This one configures the web hosting
                    webBuilder.UseStartup<StartupWeb>();
                });
    }
}

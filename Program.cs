using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DigitalWellbeing
{
    class Program
    {
        static void Main()
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IConfiguration>(context.Configuration);
                    services.AddSingleton<Configuration>();
                    services.AddSingleton<Utils>();
                    services.AddSingleton<DBStorage>();
                    services.AddSingleton<Logger>();
                    services.AddSingleton<Listener>();

                    services.AddHostedService<ListenerHostedService>();
                })
                .UseConsoleLifetime()
                .Build();

            host.Run();
        }
    }

    public class ListenerHostedService : IHostedService
    {
        private readonly Listener _listener;

        public ListenerHostedService(Listener listener)
        {
            _listener = listener;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _listener.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

using System.Net.Http;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SiddhiCli.Configuration;
using SiddhiCli.Services;

namespace SiddhiCli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var serviceProvider = ConfigureApplication();

            var application = new CommandLineApplication(false);
            application.VersionOption("-v|--version", "1.0.0");
            application.HelpOption("-? | -h | --help");

            application.AddSubcommand(Commands.DeployedCommand(serviceProvider));
            application.AddSubcommand(Commands.ListCommand(serviceProvider));
            application.AddSubcommand(Commands.InstallCommand(serviceProvider));
            application.AddSubcommand(Commands.UninstallCommand(serviceProvider));
            application.AddSubcommand(Commands.MetaCommand(serviceProvider));
            application.AddSubcommand(Commands.StateCommand(serviceProvider));

            application.Execute(args);
        }

        private static ServiceProvider ConfigureApplication()
        {
            var appSettings = Assembly.GetExecutingAssembly().GetManifestResourceStream("SiddhiCli.appsettings.json");

            var builder = new ConfigurationBuilder().AddJsonStream(appSettings);
            var configuration = builder.Build();

            SerilogExtensions.ConfigureLogger("siddhicli.log");
            var serviceProvider = ConfigureServices(configuration);

            var logger = serviceProvider.GetService<ILogger<Program>>();

            return serviceProvider;
        }

        private static ServiceProvider ConfigureServices(IConfigurationRoot configuration)
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging(configure =>
                {
                    configure.AddConsole();
                    configure.AddSerilog();
                })
                .AddOptions()
                // services
                .AddTransient<IHttpClientFactory, UntrustedHttpClientFactory>()
                .AddTransient<ISiddhiAppsService, SiddhiAppsService>()
                .AddTransient<ISiddhiApiClient, SiddhiApiClient>()
                .AddTransient<IConsole, PhysicalConsole>()
                // settings
                .Configure<SiddhiAppsApiSettings>(configuration.GetSection(nameof(SiddhiAppsApiSettings)));
            return serviceCollection.BuildServiceProvider();
        }
    }
}
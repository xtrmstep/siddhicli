using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using SiddhiCli.Services;

namespace SiddhiCli
{
    public class Commands
    {
        public static CommandLineApplication DeployedCommand(ServiceProvider serviceProvider)
        {
            var cmd = new CommandLineApplication
            {
                Name = "deployed",
                Description = "Get a list of deployed Siddhi applications"
            };
            cmd.HelpOption("-? | -h | --help");

            cmd.OnExecute(() =>
            {
                var console = serviceProvider.GetService<IConsole>();
                var appService = serviceProvider.GetService<ISiddhiAppsService>();
                var output = appService.GetDeployed();

                console.WriteLine(output);

                return 0;
            });
            return cmd;
        }

        public static CommandLineApplication ListCommand(ServiceProvider serviceProvider)
        {
            var cmd = new CommandLineApplication
            {
                Name = "list",
                Description = "Get a list of Siddhi applications in current folder"
            };
            cmd.AddName("l");
            cmd.HelpOption("-? | -h | --help");

            cmd.OnExecute(() =>
            {
                var console = serviceProvider.GetService<IConsole>();
                var appService = serviceProvider.GetService<ISiddhiAppsService>();
                var output = appService.List();
                console.WriteLine(output);

                return 0;
            });
            return cmd;
        }

        public static CommandLineApplication InstallCommand(ServiceProvider serviceProvider)
        {
            var cmd = new CommandLineApplication
            {
                Name = "install",
                Description = "Deploy Siddhi application to the worker node"
            };
            cmd.AddName("i");
            cmd.HelpOption("-? | -h | --help");

            var app = cmd.Argument("app", "Siddhi application name");

            cmd.OnExecute(() =>
            {
                var console = serviceProvider.GetService<IConsole>();

                var appService = serviceProvider.GetService<ISiddhiAppsService>();
                string output;
                if (string.IsNullOrEmpty(app.Value))
                {
                    console.WriteLine("Deploying all applications...");
                    output = appService.InstallAllInCurrentFolder();
                }
                else
                {
                    console.WriteLine($"Deploying application: {app.Value}");
                    output = appService.Install(app.Value);
                }

                console.WriteLine(output);

                return 0;
            });
            return cmd;
        }

        public static CommandLineApplication UninstallCommand(ServiceProvider serviceProvider)
        {
            var cmd = new CommandLineApplication
            {
                Name = "uninstall",
                Description = "Delete Siddhi application from the worker node"
            };
            cmd.AddName("u");
            cmd.HelpOption("-? | -h | --help");

            var app = cmd.Argument("app", "Siddhi application name");

            cmd.OnExecute(() =>
            {
                var console = serviceProvider.GetService<IConsole>();

                var appService = serviceProvider.GetService<ISiddhiAppsService>();
                string output;
                if (string.IsNullOrEmpty(app.Value))
                {
                    console.WriteLine("Deleting all applications...");
                    output = appService.UninstallAll();
                }
                else
                {
                    console.WriteLine($"Deleting application: {app.Value}");
                    output = appService.Uninstall(app.Value);
                }

                console.WriteLine(output);

                return 0;
            });
            return cmd;
        }

        public static CommandLineApplication MetaCommand(ServiceProvider serviceProvider)
        {
            var cmd = new CommandLineApplication
            {
                Name = "meta",
                Description = "Get Siddhi application meta data"
            };
            cmd.AddName("m");
            cmd.HelpOption("-? | -h | --help");

            var app = cmd.Argument("app", "Siddhi application name");

            cmd.OnExecute(() =>
            {
                var console = serviceProvider.GetService<IConsole>();

                var errors = 0;
                errors += ArgumentRequiresValue(app, console);
                if (errors > 0) return 1;

                var appService = serviceProvider.GetService<ISiddhiAppsService>();
                var output = appService.GetMeta(app.Value);
                console.WriteLine(output);

                return 0;
            });
            return cmd;
        }

        public static CommandLineApplication StateCommand(ServiceProvider serviceProvider)
        {
            var cmd = new CommandLineApplication
            {
                Name = "state",
                Description = "Get Siddhi application state"
            };
            cmd.AddName("s");
            cmd.HelpOption("-? | -h | --help");

            var app = cmd.Argument("app", "Siddhi application name");
            var tbl = cmd.Argument("tbl", "Application state table name");

            cmd.OnExecute(() =>
            {
                var console = serviceProvider.GetService<IConsole>();

                var errors = 0;
                errors += ArgumentRequiresValue(app, console);
                if (errors > 0) return 1;

                var appService = serviceProvider.GetService<ISiddhiAppsService>();
                var output = appService.GetState(app.Value, tbl.Value);
                console.WriteLine(output);

                return 0;
            });
            return cmd;
        }

        private static int ArgumentRequiresValue(CommandArgument arg, IConsole console)
        {
            if (!string.IsNullOrEmpty(arg.Value)) return 0;
            console.WriteLine($"ERROR: Argument {arg.Name} need to have a value.");
            return 1;
        }
    }
}
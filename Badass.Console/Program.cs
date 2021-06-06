using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Badass.Postgres;
using Badass.ProjectGeneration;
using Badass.Model;
using Microsoft.Extensions.Configuration;
using Mono.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Badass.Console
{
    class Program
    {
        private static Settings _settings;
        private static Stopwatch _sw = new Stopwatch();
        private static IFileSystem _fileSystem = new FileSystem();

        static void Main(string[] args)
        {
            WriteLogo();
            var levelSwitch = new LoggingLevelSwitch();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console().MinimumLevel.ControlledBy(levelSwitch)
                .CreateLogger();

            _sw.Start();

            var settings = ParseArguments(args);
            if (settings == null)
            {
                return;
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(settings.ConfigurationFile, optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            if (!UpdateSettingsFromConfiguration(settings, configuration))
            {
                return;
            }

            _settings = settings;

            if (settings.Debug)
            {
                if (!Debugger.IsAttached)
                {
                    Debugger.Launch();
                }
            }

            if (settings.Verbosity > 0)
            {
                levelSwitch.MinimumLevel = LogEventLevel.Verbose;
            }

            var provider = new PostgresTypeProvider(settings.ConnectionString);
            var fileSystem = new FileSystem();
            var writer = new FileWriter(fileSystem, settings.RootDirectory);
            var generator = new Generator(fileSystem, settings, writer);

            if (settings.DeleteGeneratedFiles)
            {
                var cleaner = new GeneratedFileCleaner(fileSystem, settings);
                cleaner.ClearGeneratedFiles();
            }

            generator.Generate(provider);
        }

        private static void WriteLogo()
        {
            var fg = System.Console.ForegroundColor;
            var bg = System.Console.BackgroundColor;
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.Write("B");
            System.Console.ForegroundColor = fg;
            System.Console.BackgroundColor = bg;
            System.Console.WriteLine("adass");
        }

        private static Settings ParseArguments(string[] args)
        {
            var s = new Settings(_fileSystem);
            s.NewAppSettings = new NewAppSettings();
            s.WebUIType = WebUIType.React;

            var os = new OptionSet() {
                "Usage: dotnet Badass.Console.dll -r VALUE <Options>",
                "",
                "Options:",
                { "brand-color=", "Brand Color for new project. Only applicable when -n or --new option is specified", bc => s.NewAppSettings.BrandColour = bc },
                { "c|config=", "JSON configuration file to use.", c => s.ConfigurationFile = c },
                { "data-fldr|database-code-folder=", "the root folder to generate database code into.", m => s.DataDirectory = m },
                { "data-test-fldr|database-test-folder=", "the root folder to generate database test helpers into.", m => s.TestDataDirectory = m },
                { "dbg|debug", "Attach Debugger on start", d => s.Debug = d != null },
                { "del", "delete generated files before re-generating", d => s.DeleteGeneratedFiles = d != null},
                { "flutter", "Generate a Flutter client for application", f => {if (f != null) s.ClientAppTypes.Add(ClientAppUIType.Flutter); } },
                { "h|?|help",  "show this message and exit", h => s.ShowHelp = h != null },
                { "logo=", "SVG logo for new project. Only applicable when -n or --new option is specified", logo => s.NewAppSettings.LogoFileName = logo },
                { "name=", "Name of the application. Used for default C# namespace for generated items", n => s.ApplicationName = n },
                { "n|new", "Generate a new project", n => s.NewAppSettings.CreateNew = n != null },
                { "no-policy", "Globally disable generation of security policies", p => { if (p != null) s.GenerateSecurityPolicies = false; }  },
                { "no-test-repo", "Disable generation of test repositories", t => { if (t != null) s.GenerateTestRepos = false; }},
                { "r|root=", "the root folder to generate code into.", r => s.RootDirectory = r },
                { "react", "Set the web UI generated to be React", r => {if (r != null) s.WebUIType = WebUIType.React; } },
                { "test-data=", "Generate test data of the specified size for empty tables.", t => s.TestDataSize = int.Parse(t) },
                { "tmplt=", "Template project directory", t => { if (t != null) { s.NewAppSettings.TemplateProjectDirectory = t; } }},
                { "t|type=", "Only generate for a single type (for debugging)", t => s.TypeName = t },
                { "u|update-db-operations",  "Update database with generated operations", u => s.AddGeneratedOptionsToDatabase = u != null },
                { "v", "increase debug message verbosity", v => { if (v != null) ++s.Verbosity; } },
                { "x|exclude=", "Exclude schema", x => s.ExcludedSchemas.Add(x) },
            };

            var extra = os.Parse(args);
            if (extra.Any())
            {
                var message = string.Join(" ", extra.ToArray());
                System.Console.WriteLine("There were some un-recognized command-line arguments: " + message);
            }

            if (s.ShowHelp)
            {
                os.WriteOptionDescriptions(System.Console.Out);
                return null;
            }

            return s;
        }

        private static bool UpdateSettingsFromConfiguration(Settings settings, IConfigurationRoot configuration)
        {
            settings.ConnectionString = configuration.GetConnectionString("application-db");
            if (string.IsNullOrEmpty(settings.ConnectionString))
            {
                System.Console.WriteLine("Connection string has not been configured. Provide an entry in the appsettings.json");
                return false;
            }

            if (string.IsNullOrEmpty(settings.RootDirectory))
            {
                settings.RootDirectory = configuration.GetValue<string>("root");
                if (string.IsNullOrEmpty(settings.RootDirectory))
                {
                    System.Console.WriteLine("root folder has not been specified. You can provide an entry for 'root' in the appsettings.json, or provide one using the -r command-line argument");
                    return false;
                }
            }

            if (string.IsNullOrEmpty(settings.ApplicationName))
            {
                // this setting is not required, so should not trigger an error
                settings.ApplicationName = configuration.GetValue<string>("name");
            }

            if (string.IsNullOrEmpty(settings.DataDirectory))
            {
                // this setting is not required, so should not trigger an error
                settings.DataDirectory = configuration.GetValue<string>("data-dir");
            }

            if (string.IsNullOrEmpty(settings.TestDataDirectory))
            {
                settings.TestDataDirectory = configuration.GetValue<string>("data-test-dir");
            }
            
            if (string.IsNullOrEmpty(settings.OpenApiUri))
            {
                settings.OpenApiUri = configuration.GetValue<string>("openapi-uri");
            }

            if (string.IsNullOrEmpty(settings.NewAppSettings.TemplateProjectDirectory))
            {
                settings.NewAppSettings.TemplateProjectDirectory = configuration.GetValue<string>("template-dir");
            }
            
            return true;
        }
    }
}

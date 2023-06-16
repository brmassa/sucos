﻿using System.CommandLine;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;

namespace SuCoS;

/// <summary>
/// The main entry point of the program.
/// </summary>
public class Program
{
    private static int Main(string[] args)
    {
        // use Serilog to log the program's output
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(formatProvider: System.Globalization.CultureInfo.CurrentCulture)
            .CreateLogger();

        // Print the logo of the program.
        OutputLogo();

        // Print the name and version of the program.
        var assembly = Assembly.GetEntryAssembly();
        var assemblyName = assembly?.GetName();
        var appName = assemblyName?.Name;
        var appVersion = assemblyName?.Version;
        Log.Information("{name} v{version}", appName, appVersion);

        // Shared options between the commands
        var sourceOption = new Option<string>(new[] { "--source", "-s" }, () => ".", "Source directory path");
        var verboseOption = new Option<bool>(new[] { "--verbose", "-v" }, () => false, "Verbose output");

        // BuildCommand setup
        var buildOutputOption = new Option<string>(new[] { "--output", "-o" }, () => "./public", "Output directory path");

        Command buildCommand = new("build", "Builds the site")
            {
                sourceOption,
                buildOutputOption,
                verboseOption
            };
        buildCommand.SetHandler((source, output, verbose) =>
        {
            BuildOptions buildOptions = new()
            {
                Source = source,
                Output = output,
                Verbose = verbose
            };
            _ = new BuildCommand(buildOptions);
        },
        sourceOption, buildOutputOption, verboseOption);

        // ServerCommand setup
        Command serveCommand = new("serve", "Starts the server")
        {
            sourceOption,
            verboseOption
        };
        serveCommand.SetHandler(async (source, verbose) =>
        {
            ServeOptions serverOptions = new()
            {
                Source = source,
                Verbose = verbose
            };

            var serveCommand = new ServeCommand(serverOptions);
            await serveCommand.RunServer();
            await Task.Delay(-1);  // Wait forever.
        },
        sourceOption, verboseOption);

        RootCommand rootCommand = new("SuCoS commands")
        {
            buildCommand,
            serveCommand
        };

        return rootCommand.Invoke(args);
    }

    private static void OutputLogo()
    {
        Log.Information(@"
 ____             ____            ____       
/\  _`\          /\  _`\         /\  _`\     
\ \,\L\_\  __  __\ \ \/\_\    ___\ \,\L\_\   
 \/_\__ \ /\ \/\ \\ \ \/_/_  / __`\/_\__ \   
   /\ \L\ \ \ \_\ \\ \ \L\ \/\ \L\ \/\ \L\ \ 
   \ `\____\ \____/ \ \____/\ \____/\ `\____\
    \/_____/\/___/   \/___/  \/___/  \/_____/
");
    }
}

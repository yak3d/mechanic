using Mechanic.CLI.Application;
using Mechanic.CLI.Commands;
using Mechanic.CLI.Commands.File;
using Mechanic.Core.Contracts;
using Mechanic.Core.Repositories;
using Mechanic.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Spectre.Console.Cli;

var registrations = new ServiceCollection();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}",
        theme: AnsiConsoleTheme.Code)
    .CreateLogger();

registrations.AddLogging(builder => builder.AddSerilog());
registrations.AddSingleton<IFileService, FileService>();
registrations.AddSingleton<IProjectRepository, JsonProjectRepository>();
registrations.AddSingleton<IProjectService, ProjectService>();
if (OperatingSystem.IsWindows())
{
    registrations.AddSingleton<IRegistryService, RegistryService>();
}
registrations.AddSingleton<SteamService, WindowsSteamService>();

var registrar = new TypeRegistrar(registrations);

var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.AddCommand<InitializeCommand>("init");
    config.AddBranch("file", file =>
    {
        file.SetDescription("Allows you to add, list, remove files tracked by Mechanic.");
        file.AddCommand<FileListCommand>("ls").WithAlias("list").WithDescription("Lists all of the files managed by the Mechanic project in this directory.");
        file.AddBranch("src", src =>
        {
            src.SetDescription("Allows you to add, list, remove [b]source[/] files tracked by Mechanic.");
            src.AddCommand<FileSrcAddCommand>("add")
                .WithDescription(
                    "Adds a [b]source[/] file to track by Mechanic. It should exist in your source directory..");
        });
        file.AddBranch("game", game =>
        {
            game.SetDescription("Allows you to add, list, remove [b]game[/] files tracked by Mechanic.");
            game.AddCommand<FileGameAddCommand>("add")
                .WithDescription("Adds a [b]game[/] file to track by Mechanic.");
        });
    });
});

return app.Run(args);

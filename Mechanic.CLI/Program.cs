using Mechanic.CLI.Application;
using Mechanic.CLI.Commands;
using Mechanic.CLI.Commands.File;
using Mechanic.CLI.Contracts;
using Mechanic.CLI.Infrastructure.Logging;
using Mechanic.CLI.Models.Settings;
using Mechanic.CLI.Services;
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
    .WriteTo.SpectreConsole(
        outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}"
    )
    .CreateLogger();

registrations.AddLogging(builder => builder.AddSerilog());
registrations.AddSingleton<IFileService, FileService>();
registrations.AddSingleton<IProjectRepository, JsonProjectRepository>();
registrations.AddSingleton<IProjectService, ProjectService>();
registrations.AddSingleton<IOSService, OSService>();
registrations.AddSingleton<ILocalSettingsService, JsonLocalSettingsService>();
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
    config.AddCommand<ConfigureCommand>("configure");
    config.AddCommand<CheckCommand>("check");
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
            src.AddCommand<FileSourceFileRemoveCommand>("rm")
                .WithDescription(
                    "Removes a [b]source[/] file from tracking in Mechanic. Use either `--id` or `--path` to choose the file to remove but not both. Omit both to use interactive mode."
                )
                .WithExample(
                    "file src rm --id 4f947ccf-a191-4dda-8a12-a5b1c536dba6"
                ).WithExample(
                    @"file src rm --path textures\project\metal01.tiff"
                );
        });
        file.AddBranch("game", game =>
        {
            game.SetDescription("Allows you to add, list, remove [b]game[/] files tracked by Mechanic.");
            game.AddCommand<FileGameAddCommand>("add")
                .WithDescription("Adds a [b]game[/] file to track by Mechanic.");
            game.AddCommand<FileGameFileRemoveCommand>("rm")
                .WithDescription("Removes a [b]game[/] file from tracking in Mechanic. Use either `--id` or `--path` to choose the file to remove but not both. Omit both to use interactive mode.")
                .WithExample(
                    "file game rm --id 4f947ccf-a191-4dda-8a12-a5b1c536dba6"
                ).WithExample(
                    @"file game rm --path textures\project\metal01.dds"
                );
        });
    });
});

return app.Run(args);

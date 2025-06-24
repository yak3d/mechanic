// See https://aka.ms/new-console-template for more information

using Mechanic.CLI.Application;
using Mechanic.CLI.Commands;
using Mechanic.Core.Contracts;
using Mechanic.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

var registrations = new ServiceCollection();
registrations.AddLogging(builder =>
{
    builder.AddConsole()
        .SetMinimumLevel(LogLevel.Trace);
});
registrations.AddSingleton<IFileService, FileService>();
registrations.AddSingleton<IProjectSerializationService<string>, JsonFileProjectSerializationService>();
registrations.AddSingleton<IProjectService, ProjectService>();

var registrar = new TypeRegistrar(registrations);

var serviceProvider = registrations.BuildServiceProvider();
try
{
    var fileService = serviceProvider.GetRequiredService<IFileService>();
    Console.WriteLine("✓ IFileService resolved");
    
    var serializationService = serviceProvider.GetRequiredService<IProjectSerializationService<string>>();
    Console.WriteLine("✓ IProjectSerializationService<string> resolved");
    
    var projectService = serviceProvider.GetRequiredService<IProjectService>();
    Console.WriteLine("✓ IProjectService resolved");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Failed: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
    }
}

var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.AddCommand<InitializeCommand>("init");
});

return app.Run(args);

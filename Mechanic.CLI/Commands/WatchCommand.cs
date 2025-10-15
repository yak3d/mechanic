using System.Collections.Concurrent;
using Mechanic.CLI.Commands.File;
using Mechanic.CLI.Models;
using Mechanic.Core.Contracts;
using Mechanic.Core.Models.FileWatcher;
using Spectre.Console;
using Spectre.Console.Cli;
using SourceFileTypeExtensions = Mechanic.CLI.Models.SourceFileTypeExtensions;

namespace Mechanic.CLI.Commands;

public class WatchCommand(IFileWatcherBuilder fileWatcherBuilder, IProjectService projectService, IFileEventHandler fileEventHandler) : AsyncCommand, IObserver<FileEvent>
{
    private ConcurrentQueue<FileEvent> FileEvents { get; } = new();

    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        var project = await projectService.GetCurrentProjectAsync();
        var fileWatcher = fileWatcherBuilder.SetGamePath(project.GamePath).SetSourcePath(Directory.GetCurrentDirectory()).Build();

        fileWatcher.Subscribe(fileEventHandler);
        fileEventHandler.Subscribe(this);
        fileWatcher.StartFileWatcher();

        AnsiConsole.MarkupLine("Watching files, press Q to stop");

        while (true)
        {
            while (this.FileEvents.IsEmpty)
            {
                Thread.Sleep(1000);
            }

            while (this.FileEvents.TryDequeue(out var fileEvent))
            {
                    if (fileEvent.FileType == FileType.Game)
                    {
                        await this.HandleGameFileEvent(fileEvent);
                    }
                    if (fileEvent.FileType == FileType.Source)
                    {
                        await this.HandleSourceFileEvent(fileEvent);
                    }
            }
            var cki = Console.ReadKey(true);
            if (cki.Key == ConsoleKey.Q)
            {
                break;
            }
        }

        return 0;
    }

    private async Task HandleSourceFileEvent(FileEvent fileEvent)
    {
        switch (fileEvent.EventType)
        {
            case FileEventType.Created:
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<FileEventChoice>()
                        .Title($"The source file [green]{fileEvent.FilePath}[/] was created, add to the project?")
                        .AddChoices([FileEventChoice.Affirmative, FileEventChoice.Ignore])
                );

                if (choice == FileEventChoice.Affirmative)
                {
                    var fileType = CommonFilePrompts.AssumeOrPromptSourceFileType(fileEvent.FilePath);
                    await projectService.AddSourceFileAsync(fileEvent.FilePath, SourceFileTypeExtensions.ToDomain(fileType));
                    AnsiConsole.MarkupLine($"[green]Added[/] file [green]{fileEvent.FilePath}[/] to the project's source files.");
                }

                break;
            }
            case FileEventType.Deleted:
            {
                if (await projectService.FileExistsWithPathAsync(fileEvent.FilePath))
                {
                    var choice = AnsiConsole.Prompt(
                        new SelectionPrompt<FileEventChoice>()
                            .Title(
                                $"The source file [green]{fileEvent.FilePath}[/] was deleted, remove from the project?") // todo: should mention if its an fbx or whatever we probably don't wanna remove it
                            .AddChoices([FileEventChoice.Affirmative, FileEventChoice.Ignore])
                            .UseConverter(eventChoice => eventChoice switch
                            {
                                FileEventChoice.Affirmative => "Remove",
                                FileEventChoice.Ignore => FileEventChoice.Ignore.ToString(),
                                _ => throw new ArgumentOutOfRangeException(nameof(eventChoice), eventChoice,
                                    null)
                            })
                    );

                    if (choice == FileEventChoice.Affirmative)
                    {
                        await projectService.RemoveSourceFileByPathAsync(fileEvent.FilePath);
                        AnsiConsole.MarkupLine(
                            $"[red]Removed[/] file [green]{fileEvent.FilePath}[/] from the project's source files.");
                    }
                }

                break;
            }
            case FileEventType.Changed:
            case FileEventType.Renamed:
                break;
            default:
                throw new ArgumentOutOfRangeException($"The file event type {fileEvent.EventType} is not supported.");
        }
    }

    private async Task HandleGameFileEvent(FileEvent fileEvent)
    {
        switch (fileEvent.EventType)
        {
            case FileEventType.Created:
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<FileEventChoice>()
                        .Title($"The file [magenta]{fileEvent.FilePath}[/] was created, add to the project?")
                        .AddChoices([FileEventChoice.Affirmative, FileEventChoice.Ignore])
                );

                if (choice == FileEventChoice.Affirmative)
                {
                    var fileType = CommonFilePrompts.AssumeOrPromptGameFileType(fileEvent.FilePath);
                    await projectService.AddGameFileAsync(fileEvent.FilePath, fileType.ToDomain());
                    AnsiConsole.MarkupLine($"Added file [magenta]{fileEvent.FilePath}[/] to the project's game files.");
                }

                break;
            }
            case FileEventType.Deleted:
            {
                if (await projectService.FileExistsWithPathAsync(fileEvent.FilePath))
                {
                    var choice = AnsiConsole.Prompt(
                        new SelectionPrompt<FileEventChoice>()
                            .Title(
                                $"The game file [magenta]{fileEvent.FilePath}[/] was deleted, remove from the project?") // todo: should mention if its an fbx or whatever we probably don't wanna remove it
                            .AddChoices([FileEventChoice.Affirmative, FileEventChoice.Ignore])
                            .UseConverter(eventChoice => eventChoice switch
                            {
                                FileEventChoice.Affirmative => "Remove",
                                FileEventChoice.Ignore => FileEventChoice.Ignore.ToString(),
                                _ => throw new ArgumentOutOfRangeException(nameof(eventChoice), eventChoice,
                                    null)
                            })
                    );

                    if (choice == FileEventChoice.Affirmative)
                    {
                        await projectService.RemoveGameFileByPathAsync(fileEvent.FilePath);
                        AnsiConsole.MarkupLine(
                            $"[red]Removed[/] file [magenta]{fileEvent.FilePath}[/] from the project's source files.");
                    }
                }

                break;
            }
            case FileEventType.Changed:
            case FileEventType.Renamed:
                break;
            default:
                throw new ArgumentOutOfRangeException($"Event type {fileEvent.EventType} is not supported.");
        }
    }

    public void OnCompleted() => throw new NotImplementedException();

    public void OnError(Exception error) => throw new NotImplementedException();

    public void OnNext(FileEvent value) => this.FileEvents.Enqueue(value);
}

internal enum FileEventChoice
{
    Affirmative,
    Ignore
}

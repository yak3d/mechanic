using Mechanic.CLI.Contracts;
using Mechanic.CLI.Infrastructure.Logging;
using Mechanic.CLI.Models.Settings;
using Mechanic.Core.Contracts;
using Mechanic.Core.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mechanic.CLI.Commands;

public class ConfigureCommand(ILogger<ConfigureCommand> logger, IOSService osService, ILocalSettingsService settingsService) : AsyncCommand<ConfigureCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await HandleSpriggit();

        await HandlePyro();

        return 0;
    }

    private async Task HandlePyro()
    {
        var pyroInPath = await osService.ExecutableIsInPathAsync("pyro");
        var shouldPrompt = pyroInPath.Match(
            Right: isInPath => !isInPath,
            Left: err =>
            {
                logger.ExecutableIsOnPath("pyro", err);
                return true;
            });

        if (shouldPrompt)
        {
            var pyroPath = await AnsiConsole.PromptAsync(new TextPrompt<string>("[blue]Enter the path to Pyro:[/] "));
            await settingsService.SaveSettingAsync(LocalSettingsOptions.SettingUsePyroInPath, false);
            await settingsService.SaveSettingAsync(LocalSettingsOptions.SettingPyroPath, pyroPath);
        }
        else
        {
            AnsiConsole.MarkupLine("Found Pyro in PATH variable. Mechanic will use the Pyro executable found in PATH.");
            await settingsService.SaveSettingAsync(LocalSettingsOptions.SettingUsePyroInPath, true);
        }
    }

    private async Task HandleSpriggit()
    {
        var spriggitInPath = await osService.ExecutableIsInPathAsync("spriggit");
        var shouldPrompt = spriggitInPath.Match(
            Right: isInPath => !isInPath,
            Left: err =>
            {
                logger.ExecutableIsOnPath("spriggit", err);
                return true;
            });

        if (shouldPrompt)
        {
            var spriggitPath = await AnsiConsole.PromptAsync(new TextPrompt<string>("[blue]Enter the path to Spriggit:[/] "));
            await settingsService.SaveSettingAsync(LocalSettingsOptions.SettingUseSpriggitInPath, false);
            await settingsService.SaveSettingAsync(LocalSettingsOptions.SettingSpriggitPath, spriggitPath);
        }
        else
        {
            AnsiConsole.MarkupLine(
                "Found Spriggit in PATH variable. Mechanic will use the Spriggit executable found in PATH.");
            await settingsService.SaveSettingAsync(LocalSettingsOptions.SettingUseSpriggitInPath, true);
        }
    }
}
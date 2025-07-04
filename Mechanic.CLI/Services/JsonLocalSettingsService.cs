using System.Text.Json;
using LanguageExt;
using Mechanic.CLI.Contracts;
using Mechanic.CLI.Errors;
using Mechanic.CLI.Infrastructure.Logging;
using Mechanic.CLI.Models.Settings;
using Mechanic.Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using static LanguageExt.Prelude;

namespace Mechanic.CLI.Services;

public class JsonLocalSettingsService : ILocalSettingsService
{
    private readonly string _configDirectory = OperatingSystem.IsWindows() 
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mechanic", "config")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "mechanic");
    
    private readonly string _configFilePath;
    private readonly ILogger<JsonLocalSettingsService> _logger;
    private readonly IFileService _fileService;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public JsonLocalSettingsService(ILogger<JsonLocalSettingsService> logger,
        IFileService fileService,
        IOptions<LocalSettingsOptions> options)
    {
        _logger = logger;
        _fileService = fileService;
        _configFilePath = Path.Combine(_configDirectory, "config.json");
    }

    private async Task<Dictionary<string, object>> LoadSettingsAsync()
    {
        try
        {
            var settingsFileContent = await _fileService.ReadAllText(_configFilePath);

            if (string.IsNullOrWhiteSpace(settingsFileContent))
            {
                return new Dictionary<string, object>();
            }

            return JsonSerializer.Deserialize<Dictionary<string, object>>(settingsFileContent) ?? new Dictionary<string, object>();
        }
        catch (Exception)
        {
            return new Dictionary<string, object>();
        }
    }
    
    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        var settings = await LoadSettingsAsync();
        if (!settings.TryGetValue(key, out var obj)) return default;
        return obj switch
        {
            JsonElement jsonElement => jsonElement.Deserialize<T>(JsonOptions),
            string jsonString => JsonSerializer.Deserialize<T>(jsonString, JsonOptions),
            _ => default
        };
    }

    public async Task<Option<SettingsError>> SaveSettingAsync<T>(string key, T value)
    {
        if (value is null)
        {
            return new Option<SettingsError>(Some(new SettingsValueWasNullError()));
        }
        
        var settings = await LoadSettingsAsync();
        settings[key] = value;
    
        var jsonContent = JsonSerializer.Serialize(settings, JsonOptions);

        try
        {
            if (!await _fileService.DirectoryExists(_configDirectory))
            {
                await _fileService.CreateDirectoryAsync(_configDirectory);
            }
        }
        catch (Exception ex)
        {
            _logger.FailedToCreateDirectoryWithException(ex);
            return new Option<SettingsError>(Some(new FailedToMakeSettingsDirectoryError()));
        }

        try
        {
            await _fileService.WriteAllText(_configFilePath, jsonContent);
        }
        catch (Exception ex)
        {
            _logger.FailedToWriteToSettingsFile(_configFilePath, ex);
            return new Option<SettingsError>(Some(new FailedToSaveSettingsError()));
        }

        return None;
    }
}
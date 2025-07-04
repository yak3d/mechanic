using LanguageExt;
using Mechanic.CLI.Errors;

namespace Mechanic.CLI.Contracts;

public interface ILocalSettingsService
{
    Task<T?> ReadSettingAsync<T>(string key);
    Task<Option<SettingsError>> SaveSettingAsync<T>(string key, T value);
}

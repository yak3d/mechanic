namespace Mechanic.Core.Contracts;

public interface IRegistryService
{
    object? GetValue(string keyName, string? valueName, object? defaultValue);
}

namespace Mechanic.Core.Contracts;

public interface IRegistryService
{
    public object? GetValue(string keyName, string? valueName, object? defaultValue);
}

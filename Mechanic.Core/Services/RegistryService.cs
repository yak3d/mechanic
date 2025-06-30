namespace Mechanic.Core.Services;

using System.Runtime.Versioning;
using Contracts;
using Microsoft.Win32;

[SupportedOSPlatform("windows")]
public class RegistryService : IRegistryService
{
    public object? GetValue(string keyName, string? valueName, object? defaultValue) => Registry.GetValue(keyName, valueName, defaultValue);
}

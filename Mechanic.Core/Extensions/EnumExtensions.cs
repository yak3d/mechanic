namespace Mechanic.Core.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// Attempts to parse an enum and if it fails, returns null
    /// </summary>
    /// <param name="value">The string value to parse</param>
    /// <typeparam name="T">The type of the enum to parse into</typeparam>
    /// <returns>The correct enum value or null if the parsing fails</returns>
    public static T? TryParseEnum<T>(this string value) where T : struct, Enum => Enum.TryParse<T>(value, true, out var result) ? result : null;
}

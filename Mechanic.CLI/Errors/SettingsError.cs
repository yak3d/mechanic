namespace Mechanic.CLI.Errors;

public record SettingsError;
public record SettingsValueWasNullError : SettingsError;
public record FailedToMakeSettingsDirectoryError : SettingsError;
public record FailedToSaveSettingsError : SettingsError;